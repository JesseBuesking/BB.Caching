using System;

namespace BB.Caching
{
    // TODO create a separate counters project?
    public interface ICounter
    {
        /*
         * flushBy: max amount of time to wait before flushing to redis. used to reduce the number of calls to redis so
         *   long as we allow up to TimeSpan worth of data being lost if the server crashes.
         *   
         * note: could write changes to file in-between redis calls, but that would add some complexity.
         */

        void Increment(string key);

        void Increment(string key, TimeSpan flushBy);

        void Increment(string key, int amount);
        
        void Increment(string key, int amount, TimeSpan flushBy);

        void Increment(string key, double amount);
        
        void Increment(string key, double amount, TimeSpan flushBy);

        void Decrement(string key);

        void Decrement(string key, TimeSpan flushBy);

        void Decrement(string key, int amount);

        void Decrement(string key, int amount, TimeSpan flushBy);

        void Decrement(string key, double amount);

        void Decrement(string key, double amount, TimeSpan flushBy);

        // Do min, max, mean, sdev, variance like in genetics project
        void UpdateStatistics(string key, int value);

        // Do min, max, mean, sdev, variance like in genetics project
        void UpdateStatistics(string key, double value);

//        // Do min, max, mean, sdev, variance like in genetics project
//        Statistics GetStatistics(string key);
    }
}
