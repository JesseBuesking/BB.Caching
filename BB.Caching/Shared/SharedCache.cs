using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BB.Caching.Connection;
using BB.Caching.Hashing;
using BookSleeve;

namespace BB.Caching.Shared
{
    public partial class SharedCache
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

        public bool QueueJump
        {
            get { return false; }
        }

        private readonly Random _random = new Random(0);

        private readonly ConsistentHashRing<RedisConnectionGroup> _consistentHashRing =
            new ConsistentHashRing<RedisConnectionGroup>();

        /// <summary>
        /// The channel used to publish and subscribe to cache invalidation requests.
        /// </summary>
        private const string _cacheInvalidationChannel = "cache/invalidate";

        /// <summary>
        /// The channel used to publish and subscribe to multiple cache invalidation requests.
        /// </summary>
        private const string _cacheMultipleInvalidationChannel = "cache/m-invalidate";

        /// <summary>
        /// Contains all the keys that have already been invalidated.
        /// <para>
        /// Since the server that calls for cache invalidation immediately removes the object from it's own cache,
        /// we track the key here so that it doesn't re-remove it. (This allows the caller to immediately store a new
        /// value without having to worry about it getting removed when the pub-sub callback to remove the key is
        /// called)
        /// </para>
        /// </summary>
        private readonly HashSet<string> _alreadyInvalidated = new HashSet<string>();

        private SharedCache()
        {
            this._consistentHashRing.Init(null, Murmur3.Instance);
        }

        public void AddRedisConnectionGroup(RedisConnectionGroup redisConnectionGroup)
        {
            this._consistentHashRing.Add(redisConnectionGroup);
        }

// ReSharper disable UnusedMember.Global
        public void RemoveRedisConnectionWrapper(RedisConnectionGroup redisConnectionGroup)
// ReSharper restore UnusedMember.Global
        {
            this._consistentHashRing.Remove(redisConnectionGroup);
        }

        public void SetPubSubRedisConnection()
        {
            this.SetupCacheInvalidationSubscription();
            this.SetupMultipleCacheInvalidationsSubscription();
        }

        private void SetupCacheInvalidationSubscription()
        {
            Cache.PubSub.Subscribe(SharedCache._cacheInvalidationChannel, data =>
                {
                    if (this._alreadyInvalidated.Contains(data))
                    {
                        this._alreadyInvalidated.Remove(data);
                    }
                    else
                    {
//                        Debug.WriteLine(String.Format("Removed key {0}.", key));
                        Cache.Memory.Remove(data);
                    }
                });
        }

        private void SetupMultipleCacheInvalidationsSubscription()
        {
            Cache.PubSub.Subscribe(SharedCache._cacheMultipleInvalidationChannel, data =>
                {
                    string[] keys = data.Split(new[] {Cache.PubSub.MultipleMessageSeparator}, StringSplitOptions.None);

                    foreach (string key in keys)
                    {
                        if (this._alreadyInvalidated.Contains(key))
                        {
                            this._alreadyInvalidated.Remove(key);
                        }
                        else
                        {
//                            Debug.WriteLine(String.Format("Removed key {0}.", key));
                            Cache.Memory.Remove(key);
                        }
                    }
                });
        }

// ReSharper disable MemberCanBePrivate.Global
        public RedisConnection GetReadConnection(string key)
// ReSharper restore MemberCanBePrivate.Global
        {
            return this._consistentHashRing.GetNode(key).GetReadConnection();
        }

        public RedisConnection[] GetWriteConnections(string key)
        {
            return this._consistentHashRing.GetNode(key).GetWriteConnections();
        }

// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable UnusedMember.Global
        public Dictionary<RedisConnection, string[]> GetReadConnections(string[] keys)
// ReSharper restore UnusedMember.Global
// ReSharper restore ParameterTypeCanBeEnumerable.Global
        {
            var result = new Dictionary<RedisConnectionGroup, List<string>>();
            foreach (string key in keys)
            {
                var connection = this._consistentHashRing.GetNode(key);
                if (!result.ContainsKey(connection))
                    result[connection] = new List<string> {key};
                else
                    result[connection].Add(key);
            }

            return result.ToDictionary(r => r.Key.GetReadConnection(), r => r.Value.ToArray());
        }

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ParameterTypeCanBeEnumerable.Global
        public Dictionary<RedisConnection[], string[]> GetWriteConnections(string[] keys)
// ReSharper restore ParameterTypeCanBeEnumerable.Global
// ReSharper restore MemberCanBePrivate.Global
        {
            var result = new Dictionary<RedisConnectionGroup, List<string>>();
            foreach (string key in keys)
            {
                var connection = this._consistentHashRing.GetNode(key);
                if (!result.ContainsKey(connection))
                    result[connection] = new List<string> {key};
                else
                    result[connection].Add(key);
            }

            return result.ToDictionary(r => r.Key.GetWriteConnections(), r => r.Value.ToArray());
        }

// ReSharper disable MemberCanBePrivate.Global
        public RedisConnection GetRandomReadConnection()
// ReSharper restore MemberCanBePrivate.Global
        {
            return this._consistentHashRing.GetAvailableNodes()
                .ElementAt(this._random.Next(0, this._consistentHashRing.GetAvailableNodes().Count))
                .GetReadConnection();
        }

// ReSharper disable MemberCanBePrivate.Global
        public RedisConnection[] GetAllReadConnections()
// ReSharper restore MemberCanBePrivate.Global
        {
            return this._consistentHashRing.GetAvailableNodes()
                .Select(n => n.GetReadConnection())
                .ToArray();
        }

// ReSharper disable ReturnTypeCanBeEnumerable.Global
        public RedisConnection[] GetAllWriteConnections()
// ReSharper restore ReturnTypeCanBeEnumerable.Global
        {
            return this._consistentHashRing.GetAvailableNodes()
                .SelectMany(n => n.GetWriteConnections())
                .Distinct()
                .ToArray();
        }
    }
}