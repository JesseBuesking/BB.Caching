using System;

namespace BB.Caching
{
    public interface IRateLimit
    {
        // limit key to maximum attempts within the interval of time
        void Create(string key, int maximum, TimeSpan interval);

        // allow multiple limits for a single key (aka up to 10 times per minute, 100 times per hour)
        void Create(string key, params Tuple<int, TimeSpan>[] configuration);
        
        // true if it was successful, false if it's at its max
        bool Increment(string key);

        // the next time the action can be performed for the key
        // idea: n rates were ran for the company today and they met their quota, so they should be able to request when
        //   they can begin running rates again.
        DateTime NextAllowedUtc(string key);
    }
}
