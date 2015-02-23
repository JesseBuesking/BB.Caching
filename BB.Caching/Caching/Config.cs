using System.Collections.Generic;
using System.Threading.Tasks;
using BB.Caching.Compression;
using BB.Caching.Redis;
using StackExchange.Redis;
using InMemoryCache = BB.Caching.Caching.Memory.InMemoryCache;

namespace BB.Caching
{
    public static partial class Cache
    {
        public static class Config
        {
            /// <summary>
            /// The channel used to publish and subscribe to configuration removal notifications.
            /// </summary>
            private const string CACHE_CONFIG_REMOVED_CHANNEL = "cache/config-remove";

            /// <summary>
            /// The channel used to publish and subscribe to configuration change notifications.
            /// </summary>
            private const string CACHE_CONFIG_CHANGE_CHANNEL = "cache/config-change";

            private static readonly HashSet<string> _alreadyRemoved = new HashSet<string>();

            private const string KEY_PREFIX = "bb.cache.config";

            public static void SetupSubscriptions()
            {
                Config.SetupSubscribeRemoval();
            }

            public static TType Get<TType>(string key)
            {
                TType value;
                if (InMemoryCache.TryGetDecompact(KEY_PREFIX + key, out value))
                    return value;

                Task<RedisValue> byteArrayWrapper = Cache.Shared.Hashes.GetByteArray(Config.KEY_PREFIX, key);
                if (byteArrayWrapper.Result.IsNull)
                    return default(TType);

                byte[] bytes = byteArrayWrapper.Result;
                value = Compress.Compaction.Decompact<TType>(bytes);
                return value;
            }

            public static Task<TType> GetAsync<TType>(string key)
            {
                var decompactedWrapper = InMemoryCache.TryGetDecompactAsync<TType>(KEY_PREFIX + key);
                if (!decompactedWrapper.IsNil)
                    return decompactedWrapper.ValueAsync;

                var result = Task.Run(async () =>
                    {
                        var byteArrayWrapper = Cache.Shared.Hashes.GetByteArray(Config.KEY_PREFIX, key);
                        if (byteArrayWrapper.Result.IsNull)
                            return default(TType);

                        byte[] byteArray = await byteArrayWrapper;
                        TType value = await Compress.Compaction.DecompactAsync<TType>(byteArray);
                        return value;
                    });
                return result;
            }

            public static void Set<TType>(string key, TType value, bool broadcast = true)
            {
                byte[] compact = InMemoryCache.SetCompact(KEY_PREFIX + key, value);
                Cache.Shared.Hashes.Set(KEY_PREFIX, key, compact).Wait();
                if (broadcast)
                    PubSub.Publish(Config.CACHE_CONFIG_CHANGE_CHANNEL, key).Wait();
            }

            public static Task SetAsync<TType>(string key, TType value, bool broadcast = true)
            {
                return Task.Run(async () =>
                    {
                        var compacted = await InMemoryCache.SetCompactAsync(KEY_PREFIX + key, value);
                        await Cache.Shared.Hashes.Set(KEY_PREFIX, key, compacted);
                        if (broadcast)
                            await PubSub.Publish(Config.CACHE_CONFIG_CHANGE_CHANNEL, key);
                    });
            }

            public static void Remove(string key, bool broadcast = true)
            {
                Task.Run(async () =>
                    {
                        await Cache.Shared.Keys.Invalidate(KEY_PREFIX + key);
                        await Cache.Shared.Hashes.Remove(KEY_PREFIX, key);
                        if (broadcast)
                            await Config.PublishRemoval(key);
                    }).Wait();
            }

            public static Task RemoveAsync(string key, bool broadcast = true)
            {
                return Task.Run(async () =>
                    {
                        await Cache.Shared.Keys.Invalidate(KEY_PREFIX + key);
                        await Cache.Shared.Hashes.Remove(KEY_PREFIX, key);
                        if (broadcast)
                            await Config.PublishRemoval(key);
                    });
            }

            private static void SetupSubscribeRemoval()
            {
                PubSub.Subscribe(Config.CACHE_CONFIG_REMOVED_CHANNEL, data =>
                    {
                        if (Config._alreadyRemoved.Contains(data))
                        {
                            Config._alreadyRemoved.Remove(data);
                        }
                        else
                        {
                            Config.Remove(data, false);
                        }
                    });
            }

            private static Task PublishRemoval(string configKey)
            {
                Config._alreadyRemoved.Add(configKey);
                return PubSub.Publish(Config.CACHE_CONFIG_REMOVED_CHANNEL, configKey);
            }
        }
    }
}