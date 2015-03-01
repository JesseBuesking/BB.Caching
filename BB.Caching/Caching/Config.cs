using System.Collections.Generic;
using System.Threading.Tasks;
using BB.Caching.Caching;
using BB.Caching.Compression;
using BB.Caching.Redis;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
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
                MemoryValue<TType> value = Cache.Memory.Strings.Get<TType>(KEY_PREFIX + key);
                if (value.Exists)
                {
                    return value.Value;
                }

                Task<RedisValue> byteArrayWrapper = Cache.Shared.Hashes.GetAsync(Config.KEY_PREFIX, key);
                if (byteArrayWrapper.Result.IsNull)
                {
                    return default(TType);
                }

                byte[] bytes = byteArrayWrapper.Result;
                TType result = Compress.Compression.Decompress<TType>(bytes);
                return result;
            }

            public static async Task<TType> GetAsync<TType>(string key)
            {
                MemoryValue<TType> value = await Cache.Memory.Strings.GetAsync<TType>(KEY_PREFIX + key);
                if (value.Exists)
                {
                    return value.Value;
                }

                var byteArrayWrapper = Cache.Shared.Hashes.GetAsync(Config.KEY_PREFIX, key);
                if (byteArrayWrapper.Result.IsNull)
                {
                    return default(TType);
                }

                byte[] bytes = await byteArrayWrapper;
                TType result = await Compress.Compression.DecompressAsync<TType>(bytes);
                return result;
            }

            public static void Set<TType>(string key, TType value, bool broadcast = true)
            {
                byte[] compact = Cache.Memory.Strings.Set(KEY_PREFIX + key, value);
                Cache.Shared.Hashes.Set(KEY_PREFIX, key, compact);
                if (broadcast)
                {
                    PubSub.Publish(Config.CACHE_CONFIG_CHANGE_CHANNEL, key);
                }
            }

            public static async Task SetAsync<TType>(string key, TType value, bool broadcast = true)
            {
                var compacted = await Cache.Memory.Strings.SetAsync(KEY_PREFIX + key, value);
                await Cache.Shared.Hashes.SetAsync(KEY_PREFIX, key, compacted);
                if (broadcast)
                {
                    await PubSub.PublishAsync(Config.CACHE_CONFIG_CHANGE_CHANNEL, key);
                }
            }

            public static void Delete(string key, bool broadcast = true)
            {
                Cache.Shared.Keys.Invalidate(KEY_PREFIX + key);
                Cache.Shared.Hashes.Delete(KEY_PREFIX, key);
                if (broadcast)
                {
                    Config.PublishRemoval(key);
                }
            }

            public static async Task DeleteAsync(string key, bool broadcast = true)
            {
                await Cache.Shared.Keys.InvalidateAsync(KEY_PREFIX + key);
                await Cache.Shared.Hashes.DeleteAsync(KEY_PREFIX, key);
                if (broadcast)
                {
                    await Config.PublishRemovalAsync(key);
                }
            }

            private static void PublishRemoval(string configKey)
            {
                Config._alreadyRemoved.Add(configKey);
                PubSub.Publish(Config.CACHE_CONFIG_REMOVED_CHANNEL, configKey);
            }

            private static Task PublishRemovalAsync(string configKey)
            {
                Config._alreadyRemoved.Add(configKey);
                return PubSub.PublishAsync(Config.CACHE_CONFIG_REMOVED_CHANNEL, configKey);
            }

            private static void SetupSubscribeRemoval()
            {
                PubSub.SubscribeAsync(Config.CACHE_CONFIG_REMOVED_CHANNEL, data =>
                    {
                        if (Config._alreadyRemoved.Contains(data))
                        {
                            Config._alreadyRemoved.Remove(data);
                        }
                        else
                        {
                            Config.Delete(data, false);
                        }
                    });
            }
        }
    }
}