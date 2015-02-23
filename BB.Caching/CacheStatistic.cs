using System;
using System.Text;
using System.Threading.Tasks;
using BB.Caching.Shared;
using BB.Caching.Utilities;
using StackExchange.Redis;

namespace BB.Caching
{
    public static partial class Cache
    {
        public static class Stats
        {
// ReSharper disable MemberHidesStaticFromOuterClass
            public static void Prepare()
// ReSharper restore MemberHidesStaticFromOuterClass
            {
                var setScript = Lua.Instance["SetStatistic"];
                var getScript = Lua.Instance["GetStatistic"];
                var connections = SharedCache.Instance.GetAllWriteConnections();

                foreach (var connection in connections)
                {
                    foreach (var endpoint in connection.GetEndPoints())
                    {
                        Stats._getStatisticHash = connection.GetServer(endpoint)
                            .ScriptLoad(getScript);
                        Stats._setStatisticHash = connection.GetServer(endpoint)
                            .ScriptLoad(setScript);
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

            public class Statistics
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
                        return this._sumOfValues/this.NumberOfValues;
                    }
                }

                public double PopulationVariance
                {
                    get
                    {
                        if (1 >= this.NumberOfValues)
                            return 0.0;
                        return (this._sumOfValuesSquared - (this._sumOfValues*this.Mean))/(this.NumberOfValues - 1.0);
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
                        return (this._sumOfValuesSquared - (this._sumOfValues*this.Mean))/(this.NumberOfValues);
                    }
                }

// ReSharper disable MemberCanBePrivate.Global
                public double StandardDeviation
// ReSharper restore MemberCanBePrivate.Global
                {
                    get { return Math.Sqrt(this.Variance); }
                }

                public Statistics(long numberOfValues, double sumOfValues, double sumOfValuesSquared, double minimum,
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

            public static Task SetStatistic(string key, double value)
            {
                RedisKey[] keyArgs = {key};
                RedisValue[] valueArgs = {value};

                var connections = SharedCache.Instance.GetWriteConnections(key);
                foreach (var connection in connections)
                {
                    connection.GetDatabase(SharedCache.Instance.Db)
                        .ScriptEvaluateAsync(
                            Stats.SetStatisticHash,
                            keys: keyArgs,
                            values: valueArgs,
                            flags: CommandFlags.None
                        );
                }

                return Task.FromResult(false);
            }

            public static Task<Statistics> GetStatistics(string key)
            {
                RedisKey[] keyArgs = {key};
                RedisValue[] valueArgs = new RedisValue[0];

                var connections = SharedCache.Instance.GetWriteConnections(key);
                Task<RedisResult> result = null;
                foreach (var connection in connections)
                {
                    var task = connection.GetDatabase(SharedCache.Instance.Db)
                        .ScriptEvaluateAsync(
                            Stats.GetStatisticHash,
                            keys: keyArgs,
                            values: valueArgs,
                            flags: CommandFlags.None
                        );
                    if (null == result)
                        result = task;
                }

                return Task.Run(async () =>
                    {
                        if (null == result)
                            return null;

                        RedisResult[] res = (RedisResult[]) await result;
                        long numberOfValues = (long) res[0];
                        double sumOfValues = (double) res[1];
                        double sumOfValuesSquared = (double) res[2];
                        double minimum = (double) res[3];
                        double maximum = (double) res[4];

                        return new Statistics(numberOfValues, sumOfValues, sumOfValuesSquared, minimum, maximum);
                    });
            }
        }
    }
}