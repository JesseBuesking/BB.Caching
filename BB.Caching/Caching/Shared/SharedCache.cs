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
        /// Contains all the keys that have already been invalidated.
        /// <para>
        /// Since the server that calls for cache invalidation immediately removes the object from it's own cache,
        /// we track the key here so that it doesn't re-remove it. (This allows the caller to immediately store a new
        /// value without having to worry about it getting removed when the pub-sub callback to remove the key is
        /// called)
        /// </para>
        /// TODO not used; find a tool that'll find this out on it's own and run on the solution
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
        /// The connection group used by the analytics methods. The analytics methods are built atop redis' bitwise
        /// operations, so they need to be performed against the same box in order to get good performance.
        /// </summary>
        private ConnectionGroup _analyticsConnectionGroup;

        /// <summary>
        /// Prevents a default instance of the <see cref="SharedCache"/> class from being created.
        /// </summary>
        private SharedCache()
        {
            this._consistentHashRing.Init(null);
        }

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
        /// Sets the <paramref name="connectionGroup"/> used by the analytics methods.
        /// </summary>
        /// <param name="connectionGroup">
        /// The connection group.
        /// </param>
        public void SetAnalyticsConnectionGroup(ConnectionGroup connectionGroup)
        {
            this._analyticsConnectionGroup = connectionGroup;
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
            return this._consistentHashRing.GetNode(key).GetReadMultiplexer();
        }

        /// <summary>
        /// Gets the read-write connection for the key supplied.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A read-write <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer GetWriteConnection(RedisKey key)
        {
            return this._consistentHashRing.GetNode(key).GetWriteMultiplexer();
        }

        /// <summary>
        /// Gets a read-only connection used by analytics methods.
        /// </summary>
        /// <returns>
        /// A read-only <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer GetAnalyticsReadConnection()
        {
            return this._analyticsConnectionGroup.GetReadMultiplexer();
        }

        /// <summary>
        /// Gets the read-write connection used by analytics methods.
        /// </summary>
        /// <returns>
        /// A read-write <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer GetAnalyticsWriteConnection()
        {
            return this._analyticsConnectionGroup.GetWriteMultiplexer();
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

            return result.ToDictionary(r => r.Key.GetReadMultiplexer(), r => r.Value.ToArray());
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
        public Dictionary<ConnectionMultiplexer, RedisKey[]> GetWriteConnection(RedisKey[] keys)
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

            return result.ToDictionary(r => r.Key.GetWriteMultiplexer(), r => r.Value.ToArray());
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
                .GetReadMultiplexer();
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
                .Select(n => n.GetReadMultiplexer())
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
                .Select(n => n.GetWriteMultiplexer())
                .Distinct()
                .ToArray();
        }
    }
}