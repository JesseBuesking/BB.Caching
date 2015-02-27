using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BB.Caching.Redis;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    public class SharedCache
    {
        private static readonly Lazy<SharedCache> _lazy = new Lazy<SharedCache>(
            () => new SharedCache(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static SharedCache Instance
        {
            get { return SharedCache._lazy.Value; }
        }

        public int Db
        {
            get { return 0; }
        }

        private readonly Random _random = new Random(0);

        private readonly Hashing.ConsistentHashRing<ConnectionGroup> _consistentHashRing =
            new Hashing.ConsistentHashRing<ConnectionGroup>();

        /// <summary>
        /// The channel used to publish and subscribe to cache invalidation requests.
        /// </summary>
        internal const string CACHE_INVALIDATION_CHANNEL = "cache/invalidate";

        /// <summary>
        /// The channel used to publish and subscribe to multiple cache invalidation requests.
        /// </summary>
        internal const string CACHE_MULTIPLE_INVALIDATION_CHANNEL = "cache/m-invalidate";

        /// <summary>
        /// Contains all the keys that have already been invalidated.
        /// <para>
        /// Since the server that calls for cache invalidation immediately removes the object from it's own cache,
        /// we track the key here so that it doesn't re-remove it. (This allows the caller to immediately store a new
        /// value without having to worry about it getting removed when the pub-sub callback to remove the key is
        /// called)
        /// </para>
        /// </summary>
        internal readonly HashSet<string> AlreadyInvalidated = new HashSet<string>();

        private SharedCache()
        {
            this._consistentHashRing.Init(null);
        }

        public void AddRedisConnectionGroup(ConnectionGroup connectionGroup)
        {
            this._consistentHashRing.Add(connectionGroup);
        }

// ReSharper disable UnusedMember.Global
        public void RemoveRedisConnectionWrapper(ConnectionGroup connectionGroup)
// ReSharper restore UnusedMember.Global
        {
            this._consistentHashRing.Remove(connectionGroup);
        }

        public void SetPubSubRedisConnection()
        {
            this.SetupCacheInvalidationSubscription();
            this.SetupMultipleCacheInvalidationsSubscription();
        }

        private void SetupCacheInvalidationSubscription()
        {
            PubSub.Subscribe(SharedCache.CACHE_INVALIDATION_CHANNEL, data =>
                {
                    if (this.AlreadyInvalidated.Contains(data))
                    {
                        this.AlreadyInvalidated.Remove(data);
                    }
                    else
                    {
//                        Debug.WriteLine(String.Format("Removed key {0}.", key));
                        Cache.Memory.Strings.Delete(data);
                    }
                });
        }

        private void SetupMultipleCacheInvalidationsSubscription()
        {
            PubSub.Subscribe(SharedCache.CACHE_MULTIPLE_INVALIDATION_CHANNEL, data =>
                {
                    string[] keys = data.Split(new[] {PubSub.MULTIPLE_MESSAGE_SEPARATOR}, StringSplitOptions.None);

                    foreach (string key in keys)
                    {
                        if (this.AlreadyInvalidated.Contains(key))
                        {
                            this.AlreadyInvalidated.Remove(key);
                        }
                        else
                        {
//                            Debug.WriteLine(String.Format("Removed key {0}.", key));
                            Cache.Memory.Strings.Delete(key);
                        }
                    }
                });
        }

// ReSharper disable MemberCanBePrivate.Global
        public ConnectionMultiplexer GetReadConnection(RedisKey key)
// ReSharper restore MemberCanBePrivate.Global
        {
            return this._consistentHashRing.GetNode(key).GetReadConnection();
        }

        public ConnectionMultiplexer[] GetWriteConnections(RedisKey key)
        {
            return this._consistentHashRing.GetNode(key).GetWriteConnections();
        }

// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable UnusedMember.Global
        public Dictionary<ConnectionMultiplexer, RedisKey[]> GetReadConnections(RedisKey[] keys)
// ReSharper restore UnusedMember.Global
// ReSharper restore ParameterTypeCanBeEnumerable.Global
        {
            var result = new Dictionary<ConnectionGroup, List<RedisKey>>();
            foreach (RedisKey key in keys)
            {
                var connection = this._consistentHashRing.GetNode(key);
                if (!result.ContainsKey(connection))
                    result[connection] = new List<RedisKey> {key};
                else
                    result[connection].Add(key);
            }

            return result.ToDictionary(r => r.Key.GetReadConnection(), r => r.Value.ToArray());
        }

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ParameterTypeCanBeEnumerable.Global
        public Dictionary<ConnectionMultiplexer[], RedisKey[]> GetWriteConnections(RedisKey[] keys)
// ReSharper restore ParameterTypeCanBeEnumerable.Global
// ReSharper restore MemberCanBePrivate.Global
        {
            var result = new Dictionary<ConnectionGroup, List<RedisKey>>();
            foreach (RedisKey key in keys)
            {
                var connection = this._consistentHashRing.GetNode(key);
                if (!result.ContainsKey(connection))
                    result[connection] = new List<RedisKey> {key};
                else
                    result[connection].Add(key);
            }

            return result.ToDictionary(r => r.Key.GetWriteConnections(), r => r.Value.ToArray());
        }

// ReSharper disable MemberCanBePrivate.Global
        public ConnectionMultiplexer GetRandomReadConnection()
// ReSharper restore MemberCanBePrivate.Global
        {
            return this._consistentHashRing.GetAvailableNodes()
                .ElementAt(this._random.Next(0, this._consistentHashRing.GetAvailableNodes().Count))
                .GetReadConnection();
        }

// ReSharper disable MemberCanBePrivate.Global
        public ConnectionMultiplexer[] GetAllReadConnections()
// ReSharper restore MemberCanBePrivate.Global
        {
            return this._consistentHashRing.GetAvailableNodes()
                .Select(n => n.GetReadConnection())
                .ToArray();
        }

// ReSharper disable ReturnTypeCanBeEnumerable.Global
        public ConnectionMultiplexer[] GetAllWriteConnections()
// ReSharper restore ReturnTypeCanBeEnumerable.Global
        {
            return this._consistentHashRing.GetAvailableNodes()
                .SelectMany(n => n.GetWriteConnections())
                .Distinct()
                .ToArray();
        }
    }
}