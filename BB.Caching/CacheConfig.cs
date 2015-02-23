using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace BB.Caching
{
    public static partial class Cache
    {
        public static class Config
        {
            /// <summary>
            /// The channel used to publish and subscribe to configuration removal notifications.
            /// </summary>
            private const string _cacheConfigRemovedChannel = "cache/config-remove";

            /// <summary>
            /// The channel used to publish and subscribe to configuration change notifications.
            /// </summary>
            private const string _cacheConfigChangeChannel = "cache/config-change";

            private static readonly HashSet<string> _alreadyRemoved = new HashSet<string>();

            private const string _keyPrefix = "bb.cache.config";

// ReSharper disable MemberHidesStaticFromOuterClass
            public static void Prepare()
// ReSharper restore MemberHidesStaticFromOuterClass
            {
                Config.SetupSubscribeRemoval();
            }

            public static TType Get<TType>(string key)
            {
                TType value;
                if (Cache.Memory.TryGetDecompact(_keyPrefix + key, out value))
                    return value;

                Task<RedisValue> byteArrayWrapper = Cache.Shared.Hashes.GetByteArray(Config._keyPrefix, key);
                if (byteArrayWrapper.Result.IsNull)
                    return default(TType);

                byte[] bytes = byteArrayWrapper.Result;
                value = Cache.Compaction.Decompact<TType>(bytes);
                return value;
            }

            public static Task<TType> GetAsync<TType>(string key)
            {
                var decompactedWrapper = Cache.Memory.TryGetDecompactAsync<TType>(_keyPrefix + key);
                if (!decompactedWrapper.IsNil)
                    return decompactedWrapper.ValueAsync;

                var result = Task.Run(async () =>
                    {
                        var byteArrayWrapper = Cache.Shared.Hashes.GetByteArray(Config._keyPrefix, key);
                        if (byteArrayWrapper.Result.IsNull)
                            return default(TType);

                        byte[] byteArray = await byteArrayWrapper;
                        TType value = await Cache.Compaction.DecompactAsync<TType>(byteArray);
                        return value;
                    });
                return result;
            }

            public static void Set<TType>(string key, TType value, bool broadcast = true)
            {
                byte[] compact = Cache.Memory.SetCompact(_keyPrefix + key, value);
                Cache.Shared.Hashes.Set(_keyPrefix, key, compact).Wait();
                if (broadcast)
                    Cache.PubSub.Publish(Config._cacheConfigChangeChannel, key).Wait();
            }

            public static Task SetAsync<TType>(string key, TType value, bool broadcast = true)
            {
                return Task.Run(async () =>
                    {
                        var compacted = await Cache.Memory.SetCompactAsync(_keyPrefix + key, value);
                        await Cache.Shared.Hashes.Set(_keyPrefix, key, compacted);
                        if (broadcast)
                            await Cache.PubSub.Publish(Config._cacheConfigChangeChannel, key);
                    });
            }

            public static void Remove(string key, bool broadcast = true)
            {
                Task.Run(async () =>
                    {
                        await Cache.Shared.Keys.Invalidate(_keyPrefix + key);
                        await Cache.Shared.Hashes.Remove(_keyPrefix, key);
                        if (broadcast)
                            await Config.PublishRemoval(key);
                    }).Wait();
            }

            public static Task RemoveAsync(string key, bool broadcast = true)
            {
                return Task.Run(async () =>
                    {
                        await Cache.Shared.Keys.Invalidate(_keyPrefix + key);
                        await Cache.Shared.Hashes.Remove(_keyPrefix, key);
                        if (broadcast)
                            await Config.PublishRemoval(key);
                    });
            }

            private static void SetupSubscribeRemoval()
            {
                Cache.PubSub.Subscribe(Config._cacheConfigRemovedChannel, data =>
                    {
                        if (Config._alreadyRemoved.Contains(data))
                            Config._alreadyRemoved.Remove(data);
                        else
                            Config.Remove(data, false);
                    });
            }

            private static Task PublishRemoval(string configKey)
            {
                Config._alreadyRemoved.Add(configKey);
                return Cache.PubSub.Publish(Config._cacheConfigRemovedChannel, configKey);
            }
        }
    }
}