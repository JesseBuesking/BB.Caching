using System;
using System.Text;
using System.Threading.Tasks;
using BB.Caching.Shared;
using BB.Caching.Utilities;

namespace BB.Caching
{
    public static partial class Cache
    {
        public static class Stats
        {
            public static void Prepare()
            {
                var connections = SharedCache.Instance.GetAllWriteConnections();
                foreach (var connection in connections)
                    connection.Scripting.Prepare(new[]
                        {
                            Stats.SetStatisticScript,
                            Stats.GetStatisticScript
                        });
            }

            private static string SetStatisticScript
            {
                get { return Lua.Instance["SetStatistic"]; }
            }

            private static string GetStatisticScript
            {
                get { return Lua.Instance["GetStatistics"]; }
            }

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

                public double Variance
                {
                    get
                    {
                        if (1 >= this.NumberOfValues)
                            return 0.0;
                        return (this._sumOfValuesSquared - (this._sumOfValues*this.Mean))/(this.NumberOfValues);
                    }
                }

                public double StandardDeviation
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
                string[] keyArgs = new[] {key};
                object[] valueArgs = new object[] {value};

                var connections = SharedCache.Instance.GetWriteConnections(key);
                foreach (var connection in connections)
                {
                    var task = connection.Scripting.Eval(SharedCache.Instance.Db, Stats.SetStatisticScript,
                        keyArgs, valueArgs, true, false, SharedCache.Instance.QueueJump);
                }

                return Task.FromResult(false);
            }

            public static Task<Statistics> GetStatistics(string key)
            {
                string[] keyArgs = new[] {key};

                var connections = SharedCache.Instance.GetWriteConnections(key);
                Task<object> result = null;
                foreach (var connection in connections)
                {
                    var task = connection.Scripting.Eval(SharedCache.Instance.Db, Stats.GetStatisticScript,
                        keyArgs, new object[0], true, false, SharedCache.Instance.QueueJump);
                    if (null == result)
                        result = task;
                }

                return Task.Run(async () =>
                    {
                        if (null == result)
                            return null;

                        object[] res = (object[]) await result;
                        long numberOfValues;
                        byte[] bytes = res[0] as byte[];
                        if (null != bytes)
                            numberOfValues = long.Parse(Encoding.UTF8.GetString(bytes));
                        else
                            numberOfValues = (long) res[0];

                        double sumOfValues;
                        bytes = res[1] as byte[];
                        if (null != bytes)
                            sumOfValues = double.Parse(Encoding.UTF8.GetString(bytes));
                        else
                            sumOfValues = res[1] is double ? (double) res[1] : (long) res[1];

                        double sumOfValuesSquared;
                        bytes = res[2] as byte[];
                        if (null != bytes)
                            sumOfValuesSquared = double.Parse(Encoding.UTF8.GetString((byte[]) res[2]));
                        else
                            sumOfValuesSquared = res[2] is double ? (double) res[2] : (long) res[2];

                        double minimum;
                        bytes = res[3] as byte[];
                        if (null != bytes)
                            minimum = double.Parse(Encoding.UTF8.GetString((byte[]) res[3]));
                        else
                            minimum = res[3] is double ? (double) res[3] : (long) res[3];

                        double maximum;
                        bytes = res[4] as byte[];
                        if (null != bytes)
                            maximum = double.Parse(Encoding.UTF8.GetString((byte[]) res[4]));
                        else
                            maximum = res[4] is double ? (double) res[4] : (long) res[4];

                        return new Statistics(numberOfValues, sumOfValues, sumOfValuesSquared, minimum, maximum);
                    });
            }
        }
    }
}