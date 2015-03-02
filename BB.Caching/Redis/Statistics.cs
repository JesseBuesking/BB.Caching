namespace BB.Caching.Redis
{
    using System;
    using System.Threading.Tasks;

    using BB.Caching.Redis.Lua;

    using StackExchange.Redis;

    /// <summary>
    /// Statistics class to track basic statistics across multiple servers using redis.
    /// </summary>
    public static class Statistics
    {
        /// <summary>
        /// SHA hash for the SetStatistic lua script.
        /// </summary>
        private static byte[] SetStatisticHash { get; set; }

        /// <summary>
        /// SHA hash for the GetStatistic lua script.
        /// </summary>
        private static byte[] GetStatisticHash { get; set; }
        
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
                    Statistics.GetStatisticHash = connection.GetServer(endpoint).ScriptLoad(getScript);
                    Statistics.SetStatisticHash = connection.GetServer(endpoint).ScriptLoad(setScript);
                }
            }
        }

        /// <summary>
        /// Sets the value at the key for a statistic we're tracking.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
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

        /// <summary>
        /// Sets the value at the key for a statistic we're tracking.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
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

        /// <summary>
        /// Gets the statistics being tracked at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Stats"/>.
        /// </returns>
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

            RedisResult[] res = (RedisResult[])result;
            long numberOfValues = (long)res[0];
            double sumOfValues = (double)res[1];
            double sumOfValuesSquared = (double)res[2];
            double minimum = (double)res[3];
            double maximum = (double)res[4];

            return new Stats(numberOfValues, sumOfValues, sumOfValuesSquared, minimum, maximum);
        }

        /// <summary>
        /// Gets the statistics being tracked at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
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
                {
                    result = task;
                }
            }

            if (null == result)
            {
                return null;
            }

            RedisResult[] res = (RedisResult[])result;
            long numberOfValues = (long)res[0];
            double sumOfValues = (double)res[1];
            double sumOfValuesSquared = (double)res[2];
            double minimum = (double)res[3];
            double maximum = (double)res[4];

            return new Stats(numberOfValues, sumOfValues, sumOfValuesSquared, minimum, maximum);
        }

        /// <summary>
        /// A class to calculate various statistics and give access to some statistical information.
        /// </summary>
        public class Stats
        {
            /// <summary>
            /// The sum of values.
            /// </summary>
            private readonly double _sumOfValues;

            /// <summary>
            /// The sum of values squared.
            /// </summary>
            private readonly double _sumOfValuesSquared;

            /// <summary>
            /// Initializes a new instance of the <see cref="Stats"/> class.
            /// </summary>
            /// <param name="numberOfValues">
            /// The number of values.
            /// </param>
            /// <param name="sumOfValues">
            /// The sum of values.
            /// </param>
            /// <param name="sumOfValuesSquared">
            /// The sum of values squared.
            /// </param>
            /// <param name="minimum">
            /// The minimum.
            /// </param>
            /// <param name="maximum">
            /// The maximum.
            /// </param>
            public Stats(
                long numberOfValues,
                double sumOfValues,
                double sumOfValuesSquared,
                double minimum,
                double maximum)
            {
                this.NumberOfValues = numberOfValues;
                this._sumOfValues = sumOfValues;
                this._sumOfValuesSquared = sumOfValuesSquared;
                this.MinimumValue = minimum;
                this.MaximumValue = maximum;
            }

            /// <summary>
            /// Gets the number of values.
            /// </summary>
            public long NumberOfValues
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the minimum value.
            /// </summary>
            public double MinimumValue
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the maximum value.
            /// </summary>
            public double MaximumValue
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the mean.
            /// </summary>
            public double Mean
            {
                get
                {
                    if (0 >= this.NumberOfValues)
                    {
                        return 0.0f;
                    }

                    return this._sumOfValues / this.NumberOfValues;
                }
            }

            /// <summary>
            /// Gets the population variance.
            /// </summary>
            public double PopulationVariance
            {
                get
                {
                    if (1 >= this.NumberOfValues)
                    {
                        return 0.0;
                    }

                    return (this._sumOfValuesSquared - (this._sumOfValues * this.Mean)) / (this.NumberOfValues - 1.0);
                }
            }

            /// <summary>
            /// Gets the population standard deviation.
            /// </summary>
            public double PopulationStandardDeviation
            {
                get { return Math.Sqrt(this.PopulationVariance); }
            }

            /// <summary>
            /// Gets the variance.
            /// </summary>
            public double Variance
            {
                get
                {
                    if (1 >= this.NumberOfValues)
                    {
                        return 0.0;
                    }

                    return (this._sumOfValuesSquared - (this._sumOfValues * this.Mean)) / this.NumberOfValues;
                }
            }

            /// <summary>
            /// Gets the standard deviation.
            /// </summary>
            public double StandardDeviation
            {
                get { return Math.Sqrt(this.Variance); }
            }

            /// <summary>
            /// The to string.
            /// </summary>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public override string ToString()
            {
                return string.Format(
                    "MaximumValue: {0}, Mean: {1}, MinimumValue: {2}, NumberOfValues: {3}, PopulationStandardDeviation: {4}, PopulationVariance: {5}, StandardDeviation: {6}, Variance: {7}, SumOfValues: {8}, SumOfValuesSquared: {9}",
                    this.MaximumValue,
                    this.Mean,
                    this.MinimumValue,
                    this.NumberOfValues,
                    this.PopulationStandardDeviation,
                    this.PopulationVariance,
                    this.StandardDeviation,
                    this.Variance,
                    this._sumOfValues,
                    this._sumOfValuesSquared);
            }
        }
    }
}