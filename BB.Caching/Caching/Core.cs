using System;
using System.Threading.Tasks;
using BB.Caching.Caching;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    public static partial class Cache
    {
        public static void Set(string key, object value)
        {
            throw new NotImplementedException();
        }

        public static Task SetAsync(string key, object value)
        {
            throw new NotImplementedException();
        }

        public static void Set(string key, object value, TimeSpan absoluteExpiration)
        {
            throw new NotImplementedException();
        }

        public static Task SetAsync(string key, object value, TimeSpan absoluteExpiration)
        {
            throw new NotImplementedException();
        }

        public static void SetSliding(string key, object value, TimeSpan slidingExpiration)
        {
            throw new NotImplementedException();
        }

        public static Task SetSlidingAsync(string key, object value, TimeSpan slidingExpiration)
        {
            throw new NotImplementedException();
        }

        public static MemoryValue<TObject> Get<TObject>(string key)
        {
            throw new NotImplementedException();
        }

        public static Task<MemoryValue<TObject>> GetAsync<TObject>(string key)
        {
            throw new NotImplementedException();
        }

        public static bool Exists(string key)
        {
            throw new NotImplementedException();
        }

        public static Task<bool> ExistsAsync(string key)
        {
            throw new NotImplementedException();
        }

        public static MemoryValue<TObject> GetSet<TObject>(string key, object value)
        {
            throw new NotImplementedException();
        }

        public static Task<MemoryValue<TObject>> GetSetAsync<TObject>(string key, object value)
        {
            throw new NotImplementedException();
        }

        public static MemoryValue<TObject> GetSet<TObject>(string key, object value, TimeSpan absoluteExpiration)
        {
            throw new NotImplementedException();
        }

        //public static Task<MemoryValue> GetSetAsync(string key, object value, TimeSpan absoluteExpiration)
        //{
        //    throw new NotImplementedException();
        //}

        //public static MemoryValue GetSetSliding(string key, object value, TimeSpan slidingExpiration)
        //{
        //    throw new NotImplementedException();
        //}

        //public static Task<MemoryValue> GetSetSlidingAsync(string key, object value, TimeSpan slidingExpiration)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
