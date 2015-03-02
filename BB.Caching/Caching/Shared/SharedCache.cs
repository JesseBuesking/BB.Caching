// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using BB.Caching.Redis;

    using StackExchange.Redis;

    /// <summary>
    /// The shared / L2 / redis cache.
    /// </summary>
    public class SharedCache
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static SharedCache Instance
        {
            get { return SharedCache._Lazy.Value; }
        }

        /// <summary>
        /// Gets the db.
        /// </summary>
        public int Db
        {
            get { return 0; }
        }

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

        /// <summary>
        /// Lazily loads the instance.
        /// </summary>
        private static readonly Lazy<SharedCache> _Lazy = new Lazy<SharedCache>(
            () => new SharedCache(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Creates a shared instance of a random number generator.
        /// </summary>
        private readonly Random _random = new Random(0);

        /// <summary>
        /// The consistent hash ring used to distribute keys across multiple nodes.
        /// </summary>
        private readonly Hashing.ConsistentHashRing<ConnectionGroup> _consistentHashRing =
            new Hashing.ConsistentHashRing<ConnectionGroup>();

        /// <summary>
        /// Prevents a default instance of the <see cref="SharedCache"/> class from being created.
        /// </summary>
        private SharedCache()
        {
            this._consistentHashRing.Init(null);
        }

        /// <summary>
        /// Adds a <paramref name="connectionGroup"/> to the hash ring.
        /// </summary>
        /// <param name="connectionGroup">
        /// The connection group.
        /// </param>
        public void AddRedisConnectionGroup(ConnectionGroup connectionGroup)
        {
            this._consistentHashRing.Add(connectionGroup);
        }

        /// <summary>
        /// Removes a <paramref name="connectionGroup"/> to the hash ring.
        /// </summary>
        /// <param name="connectionGroup">
        /// The connection group.
        /// </param>
        public void RemoveRedisConnectionGroup(ConnectionGroup connectionGroup)
        {
            this._consistentHashRing.Remove(connectionGroup);
        }

        /// <summary>
        /// Setups up the pub-sub subscriptions used by the cache.
        /// </summary>
        public void SetPubSubRedisConnection()
        {
            this.SetupCacheInvalidationSubscription();
            this.SetupMultipleCacheInvalidationsSubscription();
        }

        /// <summary>
        /// Gets a read-only connection.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A read-only <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer GetReadConnection(RedisKey key)
        {
            return this._consistentHashRing.GetNode(key).GetReadConnection();
        }

        /// <summary>
        /// Gets all read-write connections for the key supplied.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A read-only <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer[] GetWriteConnections(RedisKey key)
        {
            return this._consistentHashRing.GetNode(key).GetWriteConnections();
        }

        /// <summary>
        /// Gets read-only connections for each of the keys supplied.
        /// </summary>
        /// <param name="keys">
        /// The keys to get read connections for.
        /// </param>
        /// <returns>
        /// A dictionary of read-only connections for each key.
        /// </returns>
        public Dictionary<ConnectionMultiplexer, RedisKey[]> GetReadConnections(RedisKey[] keys)
        {
            var result = new Dictionary<ConnectionGroup, List<RedisKey>>();
            foreach (RedisKey key in keys)
            {
                var connection = this._consistentHashRing.GetNode(key);
                if (!result.ContainsKey(connection))
                {
                    result[connection] = new List<RedisKey> { key };
                }
                else
                {
                    result[connection].Add(key);
                }
            }

            return result.ToDictionary(r => r.Key.GetReadConnection(), r => r.Value.ToArray());
        }

        /// <summary>
        /// Gets read-write connections for each of the keys supplied.
        /// </summary>
        /// <param name="keys">
        /// The keys to get read-write connections for.
        /// </param>
        /// <returns>
        /// A dictionary of read-write connections for each key.
        /// </returns>
        public Dictionary<ConnectionMultiplexer[], RedisKey[]> GetWriteConnections(RedisKey[] keys)
        {
            var result = new Dictionary<ConnectionGroup, List<RedisKey>>();
            foreach (RedisKey key in keys)
            {
                var connection = this._consistentHashRing.GetNode(key);
                if (!result.ContainsKey(connection))
                {
                    result[connection] = new List<RedisKey> { key };
                }
                else
                {
                    result[connection].Add(key);
                }
            }

            return result.ToDictionary(r => r.Key.GetWriteConnections(), r => r.Value.ToArray());
        }

        /// <summary>
        /// Gets a random read-only connection.
        /// </summary>
        /// <returns>
        /// A read-only <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer GetRandomReadConnection()
        {
            return this._consistentHashRing.GetAvailableNodes()
                .ElementAt(this._random.Next(0, this._consistentHashRing.GetAvailableNodes().Count))
                .GetReadConnection();
        }

        /// <summary>
        /// Gets all read-only connections.
        /// </summary>
        /// <returns>
        /// An array of read-only <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer[] GetAllReadConnections()
        {
            return this._consistentHashRing.GetAvailableNodes()
                .Select(n => n.GetReadConnection())
                .ToArray();
        }

        /// <summary>
        /// Gets all read-write connections.
        /// </summary>
        /// <returns>
        /// An array of read-write <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer[] GetAllWriteConnections()
        {
            return this._consistentHashRing.GetAvailableNodes()
                .SelectMany(n => n.GetWriteConnections())
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Sets up the cache-invalidation pub-sub subscription.
        /// </summary>
        private void SetupCacheInvalidationSubscription()
        {
            PubSub.SubscribeAsync(
                SharedCache.CACHE_INVALIDATION_CHANNEL,
                data =>
                {
                    if (this.AlreadyInvalidated.Contains(data))
                    {
                        this.AlreadyInvalidated.Remove(data);
                    }
                    else
                    {
                        Cache.Memory.Strings.Delete(data);
                    }
                });
        }

        /// <summary>
        /// Sets up the multiple-cache-invalidations pub-sub subscription.
        /// </summary>
        private void SetupMultipleCacheInvalidationsSubscription()
        {
            PubSub.SubscribeAsync(
                SharedCache.CACHE_MULTIPLE_INVALIDATION_CHANNEL,
                data =>
                {
                    string[] keys = data.Split(new[] { PubSub.MULTIPLE_MESSAGE_SEPARATOR }, StringSplitOptions.None);

                    foreach (string key in keys)
                    {
                        if (this.AlreadyInvalidated.Contains(key))
                        {
                            this.AlreadyInvalidated.Remove(key);
                        }
                        else
                        {
                            Cache.Memory.Strings.Delete(key);
                        }
                    }
                });
        }
    }
}