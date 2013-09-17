using System;

namespace BB.Caching.InMemory
{
    public interface ICache
    {
        /// <summary>
        /// Tries to get the <see cref="TObject"/> stored at <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGet<TObject>(string key, out TObject value);

        /// <summary>
        /// Sets <paramref name="value"/> at <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(string key, object value);

        /// <summary>
        /// Sets <paramref name="value"/> at <paramref name="key"/>, expiring after
        /// <paramref name="absoluteExpiration"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="absoluteExpiration"></param>
        void Set(string key, object value, TimeSpan absoluteExpiration);

        /// <summary>
        /// Sets <paramref name="value"/> at <paramref name="key"/>, expiring after
        /// <paramref name="slidingExpiration"/>.
        /// <remarks>
        /// Sliding expirations only work when they are a minimum of 1 second long.
        /// http://social.msdn.microsoft.com/Forums/vstudio/en-US/b5f56edd-ce71-40e2-9d9a-ba32df74489e/cant-make-sliding-expiration-on-memorycache-work-bug#e4cc5d7e-3e8a-49ee-99a0-5515bdb2f1c7
        /// </remarks>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingExpiration"></param>
        void SetSliding(string key, object value, TimeSpan slidingExpiration);

        /// <summary>
        /// Deletes any data stored at <paramref name="key"/>, if the key exists.
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);

        /// <summary>
        /// Checks to see if the <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(string key);

        /// <summary>
        /// Gets the number of items currently in the cache.
        /// </summary>
        /// <returns></returns>
        long GetCount();

        /// <summary>
        /// Clears the cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the data stored at <paramref name="key"/>, and updates the data to <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        bool TryGetSet<TObject>(string key, object value, out TObject store);

        /// <summary>
        /// Gets the data stored at <paramref name="key"/> and updates the data to <paramref name="value"/>, expiring
        /// after <paramref name="absoluteExpiration"/>.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        bool TryGetSet<TObject>(string key, object value, TimeSpan absoluteExpiration, out TObject store);

        /// <summary>
        /// Gets the data stored at <paramref name="key"/> and updates the data to <paramref name="value"/>, expiring
        /// after <paramref name="slidingExpiration"/>.
        /// <remarks>
        /// Sliding expirations only work when they are a minimum of 1 second long.
        /// http://social.msdn.microsoft.com/Forums/vstudio/en-US/b5f56edd-ce71-40e2-9d9a-ba32df74489e/cant-make-sliding-expiration-on-memorycache-work-bug#e4cc5d7e-3e8a-49ee-99a0-5515bdb2f1c7
        /// </remarks>
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingExpiration"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        bool TryGetSetSliding<TObject>(string key, object value, TimeSpan slidingExpiration, out TObject store);
    }
}