using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BB.Caching.Connection;
using BookSleeve;

namespace BB.Caching
{
    public static partial class Cache
    {
        /// <summary>
        /// Manages all pub-sub related workflows.
        /// </summary>
        public static class PubSub
        {
            private class PubSubSingleton
            {
                private static readonly Lazy<PubSubSingleton> _lazy = new Lazy<PubSubSingleton>(
                    () => new PubSubSingleton(), LazyThreadSafetyMode.ExecutionAndPublication);

                public static PubSubSingleton Instance
                {
                    get { return PubSubSingleton._lazy.Value; }
                }

                /// <summary>
                /// A connection specifically used for transmitting pub/sub information.
                /// TODO: consider relying on the existing connections + consistent hashing (pub/sub would then happen across
                /// all the redis instances that are currently running => scales up; minus for cache/invalidate since it'll
                /// always hash to the same box)
                /// </summary>
                private SafeRedisConnection _pubSubConnection;

                /// <summary>
                /// Keeps open all subscriptions that have been made.
                /// </summary>
                private RedisSubscriberConnection Subscriptions
                {
                    get
                    {
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
                        if (null == this._subscriptions)
// ReSharper restore ConvertIfStatementToNullCoalescingExpression
                            this._subscriptions = this.GetConnection().GetOpenSubscriberChannel();
                        return this._subscriptions;
                    }
                    set { this._subscriptions = value; }
                }

                private RedisSubscriberConnection _subscriptions;

                /// <summary>
                /// All of the active subscriptions that are key-unspecific.
                /// </summary>
                private Dictionary<string, Action<string>> ActiveKeylessSubscriptions
                {
                    get;
                    set;
                }

                /// <summary>
                /// All of the active subscriptions that target specific keys.
                /// </summary>
                private Dictionary<string, Dictionary<string, Action<string>>> ActiveKeyedSubscriptions
                {
                    get;
                    set;
                }

                private PubSubSingleton()
                {
                    this.ActiveKeyedSubscriptions = new Dictionary<string, Dictionary<string, Action<string>>>();
                    this.ActiveKeylessSubscriptions = new Dictionary<string, Action<string>>();
                }

                /// <summary>
                /// Configures the singleton.
                /// </summary>
                /// <param name="connection"></param>
                public void Init(SafeRedisConnection connection)
                {
                    this._pubSubConnection = connection;
                }

                /// <summary>
                /// Gets a connection, automatically re-establishing all active subscriptions if the connection was
                /// re-created.
                /// </summary>
                /// <returns></returns>
                private RedisConnection GetConnection()
                {
                    var connection = this._pubSubConnection.GetConnection();
                    if (this._pubSubConnection.WasReCreated)
                    {
                        foreach (var kvp in this.ActiveKeylessSubscriptions)
                            this.Sub(kvp.Key, kvp.Value);

                        foreach (var kvp in this.ActiveKeyedSubscriptions)
                            foreach (var subKvp in kvp.Value)
                                this.Sub(kvp.Key, subKvp.Key, subKvp.Value);

                        this._subscriptions = null;
                    }
                    return connection;
                }

                /// <summary>
                /// Creates a subscription to a channel.
                /// </summary>
                /// <param name="channel">The channel of the subscription.</param>
                /// <param name="subscriptionCallback">The callback method.</param>
                /// <returns></returns>
                public Task Sub(string channel, Action<string> subscriptionCallback)
                {
                    // Check to make sure we're not re-creating an existing subscription first.
                    if (this.ActiveKeylessSubscriptions.ContainsKey(channel))
                        throw new Exception(string.Format("subscription to channel {0} already exists", channel));

                    // Let's add this subscription to our cache. We'll need to remember it so that we can re-subscribe
                    // if we lose our connection at any point.
                    this.ActiveKeylessSubscriptions[channel] = subscriptionCallback;
                    return this.Subscriptions.Subscribe(channel, (sameAsChannel, data) =>
                        {
                            string value = Encoding.UTF8.GetString(data);
                            subscriptionCallback(value);
                        });
                }

                /// <summary>
                /// Creates a subscription to a channel for a specific key.
                /// </summary>
                /// <param name="channel">The channel of the subscription.</param>
                /// <param name="key">The key to target.</param>
                /// <param name="subscriptionCallback">The callback method.</param>
                /// <returns></returns>
                public Task Sub(string channel, string key, Action<string> subscriptionCallback)
                {
                    // Check to make sure we're not re-creating an existing subscription first.
                    Dictionary<string, Action<string>> result;
                    if (this.ActiveKeyedSubscriptions.TryGetValue(channel, out result))
                    {
                        if (result.ContainsKey(key))
                            throw new Exception(
                                string.Format("subscription to channel {0} for key {1} already exists", channel, key));
                    }

                    // Let's add this subscription to our cache. We'll need to remember it so that we can re-subscribe
                    // if we lose our connection at any point.
                    if (!this.ActiveKeyedSubscriptions.ContainsKey(channel))
                        this.ActiveKeyedSubscriptions[channel] = new Dictionary<string, Action<string>>();

                    this.ActiveKeyedSubscriptions[channel][key] = subscriptionCallback;

                    return this.Subscriptions.Subscribe(channel, (sameAsChannel, data) =>
                        {
                            string value = Encoding.UTF8.GetString(data);
                            int idx = value.IndexOf(':');
                            if (0 > idx)
                            {
                                string message = string.Format("channel {0} expects data in key:value format", channel);
                                throw new Exception(message);
                            }

                            string pubKey = value.Substring(0, idx);
                            if (key != pubKey)
                                return;

                            string pubData = value.Substring(idx + 1, value.Length - (idx + 1));
                            subscriptionCallback(pubData);
                        });
                }

                /// <summary>
                /// Publishes a message to a channel.
                /// </summary>
                /// <param name="channel">The channel to publish to.</param>
                /// <param name="value">The value of the message.</param>
                /// <returns></returns>
                public Task<long> Pub(string channel, string value)
                {
                    return this.GetConnection()
                        .Publish(channel, value);
                }

                /// <summary>
                /// Publishes a message to a channel for a specific key.
                /// </summary>
                /// <param name="channel">The channel to publish to.</param>
                /// <param name="key">The key to target.</param>
                /// <param name="value">The value of the message.</param>
                /// <returns></returns>
                public Task<long> Pub(string channel, string key, string value)
                {
                    string concat = key + ":" + value;
                    return this.GetConnection()
                        .Publish(channel, concat);
                }
            }

            public static void Configure(SafeRedisConnection safeRedisConnection)
            {
                PubSubSingleton.Instance.Init(safeRedisConnection);
            }

            /// <summary>
            /// Use this to separate your messages if you plan on sending multiple at once.
            /// </summary>
            public const string MultipleMessageSeparator = "||";

            /// <summary>
            /// Creates a subscription to a channel.
            /// </summary>
            /// <param name="channel">The channel of the subscription.</param>
            /// <param name="subscriptionCallback">The callback method.</param>
            /// <returns></returns>
            public static Task Subscribe(string channel, Action<string> subscriptionCallback)
            {
                return PubSubSingleton.Instance.Sub(channel, subscriptionCallback);
            }

            /// <summary>
            /// Creates a subscription to a channel for a specific key.
            /// </summary>
            /// <param name="channel">The channel of the subscription.</param>
            /// <param name="key">The key to target.</param>
            /// <param name="subscriptionCallback">The callback method.</param>
            /// <returns></returns>
            public static Task Subscribe(string channel, string key, Action<string> subscriptionCallback)
            {
                return PubSubSingleton.Instance.Sub(channel, key, subscriptionCallback);
            }

            /// <summary>
            /// Publishes a message to a channel.
            /// </summary>
            /// <param name="channel">The channel to publish to.</param>
            /// <param name="value">The value of the message.</param>
            /// <returns></returns>
            public static Task<long> Publish(string channel, string value)
            {
                return PubSubSingleton.Instance.Pub(channel, value);
            }

            /// <summary>
            /// Publishes a message to a channel for a specific key.
            /// </summary>
            /// <param name="channel">The channel to publish to.</param>
            /// <param name="key">The key to target.</param>
            /// <param name="value">The value of the message.</param>
            /// <returns></returns>
            public static Task<long> Publish(string channel, string key, string value)
            {
                return PubSubSingleton.Instance.Pub(channel, key, value);
            }
        }
    }
}