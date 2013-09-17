using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// A connection specifically used for transmitting pub/sub information.
        /// TODO: consider relying on the existing connections + consistent hashing (pub/sub would then happen across
        /// all the redis instances that are currently running => scales up; minus for cache/invalidate since it'll
        /// always hash to the same box)
        /// </summary>
        private SafeRedisConnection PubSubConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Keeps open all subscription that have been made.
        /// </summary>
        private RedisSubscriberConnection _subscriptions;

        /// <summary>
        /// The channel used to publish and subscribe to cache invalidation requests.
        /// </summary>
        private const string _cacheInvalidationChannel = "cache/invalidate";

        /// <summary>
        /// The channel used to publish and subscribe to multiple cache invalidation requests.
        /// </summary>
        private const string _cacheMultipleInvalidationChannel = "cache/m-invalidate";

        /// <summary>
        /// The separator used to separate multiple keys for cache invalidation.
        /// </summary>
        private const string _cacheMultipleInvalidationSeparator = "||";

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

        public void RemoveRedisConnectionWrapper(RedisConnectionGroup redisConnectionGroup)
        {
            this._consistentHashRing.Remove(redisConnectionGroup);
        }

        public void SetPubSubRedisConnection(SafeRedisConnection safeRedisConnection)
        {
            this.PubSubConnection = safeRedisConnection;

            this.SetupCacheInvalidationSubscription();
            this.SetupMultipleCacheInvalidationsSubscription();
        }

        private void SetupCacheInvalidationSubscription()
        {
            this.RedisChannelSubscribe(SharedCache._cacheInvalidationChannel, (channel, data) =>
                {
                    string key = Encoding.UTF8.GetString(data);
                    if (this._alreadyInvalidated.Contains(key))
                    {
                        this._alreadyInvalidated.Remove(key);
                    }
                    else
                    {
// ReSharper disable RedundantStringFormatCall
//                        Debug.WriteLine(String.Format("Removed key {0}.", key));
// ReSharper restore RedundantStringFormatCall
                        Cache.Memory.Remove(key);
                    }
                });
        }

        private void SetupMultipleCacheInvalidationsSubscription()
        {
            this.RedisChannelSubscribe(SharedCache._cacheMultipleInvalidationChannel, (channel, data) =>
                {
                    string multipleKeys = Encoding.UTF8.GetString(data);
                    string[] keys = multipleKeys.Split(
                        new[] {SharedCache._cacheMultipleInvalidationSeparator}, StringSplitOptions.None);

                    foreach (string key in keys)
                    {
                        if (this._alreadyInvalidated.Contains(key))
                        {
                            this._alreadyInvalidated.Remove(key);
                        }
                        else
                        {
// ReSharper disable RedundantStringFormatCall
//                            Debug.WriteLine(String.Format("Removed key {0}.", key));
// ReSharper restore RedundantStringFormatCall
                            Cache.Memory.Remove(key);
                        }
                    }
                });
        }

        public RedisConnection GetReadConnection(string key)
        {
            return this._consistentHashRing.GetNode(key).GetReadConnection();
        }

        public RedisConnection[] GetWriteConnections(string key)
        {
            return this._consistentHashRing.GetNode(key).GetWriteConnections();
        }

        public Dictionary<RedisConnection, string[]> GetReadConnections(string[] keys)
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

        public Dictionary<RedisConnection[], string[]> GetWriteConnections(string[] keys)
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

        public RedisConnection GetRandomReadConnection()
        {
            return this._consistentHashRing.GetAvailableNodes()
                .ElementAt(this._random.Next(0, this._consistentHashRing.GetAvailableNodes().Count))
                .GetReadConnection();
        }

        public RedisConnection[] GetAllReadConnections()
        {
            return this._consistentHashRing.GetAvailableNodes()
                .Select(n => n.GetReadConnection())
                .ToArray();
        }

        public RedisConnection[] GetAllWriteConnections()
        {
            return this._consistentHashRing.GetAvailableNodes()
                .SelectMany(n => n.GetWriteConnections())
                .Distinct()
                .ToArray();
        }

        public Task RedisChannelSubscribe(string channel, Action<string, byte[]> subscriptionCallback)
        {
            if (null == this._subscriptions)
                this._subscriptions = this.PubSubConnection.GetConnection().GetOpenSubscriberChannel();

            return this._subscriptions.Subscribe(channel, subscriptionCallback);
        }

        public Task<long> RedisChannelPublish(string channel, string value)
        {
            return this.PubSubConnection.GetConnection().Publish(channel, value);
        }

        public Task RedisSubscribe(string channel, string key, Action subscriptionCallback)
        {
            if (null == this._subscriptions)
                this._subscriptions = this.PubSubConnection.GetConnection().GetOpenSubscriberChannel();

            return this._subscriptions.Subscribe(channel, (_, subValue) =>
                {
                    string s = Encoding.UTF8.GetString(subValue);
                    if (s == key)
                        subscriptionCallback();
                });
        }
    }
}