using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BB.Caching.Shared;

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

            public static void Prepare()
            {
                Config.SetupSubscribeRemoval();
            }

            public static TType Get<TType>(string key)
            {
                TType value;
                if (Cache.Memory.TryGetDecompact(_keyPrefix + key, out value))
                    return value;

                var byteArrayWrapper = Cache.Shared.Hashes.GetByteArray(Config._keyPrefix, key);
                if (byteArrayWrapper.IsNil)
                    return default(TType);

                byte[] bytes = byteArrayWrapper.Value;
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
                        if (byteArrayWrapper.IsNil)
                            return default(TType);

                        byte[] byteArray = await byteArrayWrapper.ValueAsync;
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
                    Config.PublishChange(key).Wait();
            }

            public static Task SetAsync<TType>(string key, TType value, bool broadcast = true)
            {
                return Task.Run(async () =>
                    {
                        var compacted = await Cache.Memory.SetCompactAsync(_keyPrefix + key, value);
                        await Cache.Shared.Hashes.Set(_keyPrefix, key, compacted);
                        if (broadcast)
                            await Config.PublishChange(key);
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

            public static Task SubscribeChange(string configKey, Action subscriptionCallback)
            {
                return SharedCache.Instance.RedisChannelSubscribe(Config._cacheConfigChangeChannel, (channel, data) =>
                    {
                        string key = Encoding.UTF8.GetString(data);
                        if (key == configKey)
                            subscriptionCallback();
                    });
            }

            private static void SetupSubscribeRemoval()
            {
                SharedCache.Instance.RedisChannelSubscribe(Config._cacheConfigRemovedChannel, (channel, data) =>
                    {
                        string key = Encoding.UTF8.GetString(data);
                        if (Config._alreadyRemoved.Contains(key))
                            Config._alreadyRemoved.Remove(key);
                        else
                            Config.Remove(key, false);
                    });
            }

            private static Task PublishRemoval(string configKey)
            {
                Config._alreadyRemoved.Add(configKey);
                return SharedCache.Instance.RedisChannelPublish(Config._cacheConfigRemovedChannel, configKey);
            }

            private static Task PublishChange(string configKey)
            {
                return SharedCache.Instance.RedisChannelPublish(Config._cacheConfigChangeChannel, configKey);
            }
        }
    }
}