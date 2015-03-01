using System;
using System.Threading.Tasks;
using BB.Caching.Redis.Lua;
using StackExchange.Redis;

namespace BB.Caching.Redis
{
    public static class Statistics
    {
        /// <summary>
        /// Loads the underlying Lua script(s) onto all necessary servers.
        /// </summary>
        public static void ScriptLoad()
        {
            var setScript = ScriptLoader.Instance["SetStatistic"];
            var getScript = ScriptLoader.Instance["GetStatistic"];
            var connections = SharedCache.Instance.GetAllWriteConnections();

            foreach (var connection in connections)
            {
                foreach (var endpoint in connection.GetEndPoints())
                {
                    Statistics._getStatisticHash = connection.GetServer(endpoint).ScriptLoad(getScript);
                    Statistics._setStatisticHash = connection.GetServer(endpoint).ScriptLoad(setScript);
                }
            }
        }

        private static byte[] SetStatisticHash
        {
            get { return _setStatisticHash; }
        }

        private static byte[] _setStatisticHash;

        private static byte[] GetStatisticHash
        {
            get { return _getStatisticHash; }
        }

        private static byte[] _getStatisticHash;

        public class Stats
        {
            public long NumberOfValues
            {
                get;
                private set;
            }

            private readonly double _sumOfValues;

            private readonly double _sumOfValuesSquared;

            public double MinimumValue
            {
                get;
                private set;
            }

            public double MaximumValue
            {
                get;
                private set;
            }

            public double Mean
            {
                get
                {
                    if (0 >= this.NumberOfValues)
                        return 0.0f;
                    return this._sumOfValues / this.NumberOfValues;
                }
            }

            public double PopulationVariance
            {
                get
                {
                    if (1 >= this.NumberOfValues)
                        return 0.0;
                    return (this._sumOfValuesSquared - (this._sumOfValues * this.Mean)) / (this.NumberOfValues - 1.0);
                }
            }

            public double PopulationStandardDeviation
            {
                get { return Math.Sqrt(this.PopulationVariance); }
            }

            // ReSharper disable MemberCanBePrivate.Global
            public double Variance
            // ReSharper restore MemberCanBePrivate.Global
            {
                get
                {
                    if (1 >= this.NumberOfValues)
                        return 0.0;
                    return (this._sumOfValuesSquared - (this._sumOfValues * this.Mean)) / (this.NumberOfValues);
                }
            }

            // ReSharper disable MemberCanBePrivate.Global
            public double StandardDeviation
            // ReSharper restore MemberCanBePrivate.Global
            {
                get { return Math.Sqrt(this.Variance); }
            }

            public Stats(long numberOfValues, double sumOfValues, double sumOfValuesSquared, double minimum,
                double maximum)
            {
                this.NumberOfValues = numberOfValues;
                this._sumOfValues = sumOfValues;
                this._sumOfValuesSquared = sumOfValuesSquared;
                this.MinimumValue = minimum;
                this.MaximumValue = maximum;
            }

            public override string ToString()
            {
                return
                    string.Format(
                        "MaximumValue: {0}, Mean: {1}, MinimumValue: {2}, NumberOfValues: {3}, PopulationStandardDeviation: {4}, PopulationVariance: {5}, StandardDeviation: {6}, Variance: {7}, SumOfValues: {8}, SumOfValuesSquared: {9}",
                        this.MaximumValue, this.Mean, this.MinimumValue, this.NumberOfValues,
                        this.PopulationStandardDeviation, this.PopulationVariance, this.StandardDeviation,
                        this.Variance,
                        this._sumOfValues, this._sumOfValuesSquared);
            }
        }

        public static void SetStatistic(string key, double value)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs = { value };

            var connections = SharedCache.Instance.GetWriteConnections(key);
            foreach (var connection in connections)
            {
                connection.GetDatabase(SharedCache.Instance.Db)
                    .ScriptEvaluate(Statistics.SetStatisticHash, keyArgs, valueArgs);
            }
        }

        public static async Task SetStatisticAsync(string key, double value)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs = { value };

            var connections = SharedCache.Instance.GetWriteConnections(key);
            foreach (var connection in connections)
            {
                await connection.GetDatabase(SharedCache.Instance.Db)
                    .ScriptEvaluateAsync(Statistics.SetStatisticHash, keyArgs, valueArgs);
            }
        }

        public static Stats GetStatistics(string key)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs = new RedisValue[0];

            var connections = SharedCache.Instance.GetWriteConnections(key);
            RedisResult result = null;
            foreach (var connection in connections)
            {
                result = connection.GetDatabase(SharedCache.Instance.Db)
                    .ScriptEvaluate(Statistics.GetStatisticHash, keyArgs, valueArgs);
            }

            if (null == result)
            {
                return null;
            }

            RedisResult[] res = (RedisResult[]) result;
            long numberOfValues = (long)res[0];
            double sumOfValues = (double)res[1];
            double sumOfValuesSquared = (double)res[2];
            double minimum = (double)res[3];
            double maximum = (double)res[4];

            return new Stats(numberOfValues, sumOfValues, sumOfValuesSquared, minimum, maximum);
        }

        public static async Task<Stats> GetStatisticsAsync(string key)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs = new RedisValue[0];

            var connections = SharedCache.Instance.GetWriteConnections(key);
            RedisResult result = null;
            foreach (var connection in connections)
            {
                var task = await connection.GetDatabase(SharedCache.Instance.Db)
                    .ScriptEvaluateAsync(Statistics.GetStatisticHash, keyArgs, valueArgs);

                if (null == result)
                    result = task;
            }

            if (null == result)
                return null;

            RedisResult[] res = (RedisResult[]) result;
            long numberOfValues = (long)res[0];
            double sumOfValues = (double)res[1];
            double sumOfValuesSquared = (double)res[2];
            double minimum = (double)res[3];
            double maximum = (double)res[4];

            return new Stats(numberOfValues, sumOfValues, sumOfValuesSquared, minimum, maximum);
        }
    }
}