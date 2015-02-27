using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace BB.Caching.Redis
{
    /// <summary>
    /// Manages all pub-sub related workflows.
    /// </summary>
    public static class PubSub
    {
        public class ChannelAlreadySubscribedException : Exception
        {
            public ChannelAlreadySubscribedException(string message)
                : base(message)
            {

            }
        }

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
            private ConnectionMultiplexer _pubSubConnection;

            /// <summary>
            /// Keeps open all subscriptions that have been made.
            /// </summary>
            private ISubscriber Subscriptions
            {
                get
                {
                    // ReSharper disable ConvertIfStatementToNullCoalescingExpression
                    if (null == this._subscriptions)
                        // ReSharper restore ConvertIfStatementToNullCoalescingExpression
                        this._subscriptions = this.GetConnection().GetSubscriber();
                    return this._subscriptions;
                }
            }

            private ISubscriber _subscriptions;

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
            public void Init(ConnectionMultiplexer connection)
            {
                this._pubSubConnection = connection;
            }

            /// <summary>
            /// Gets a connection, automatically re-establishing all active subscriptions if the connection was
            /// re-created.
            /// </summary>
            /// <returns></returns>
            private ConnectionMultiplexer GetConnection()
            {
                var connection = this._pubSubConnection;

                // TODO verify that we don't need to resubscribe
                //if (this._pubSubConnection.WasRecreated)
                //{
                //    foreach (var kvp in this.ActiveKeylessSubscriptions)
                //    {
                //        this.SubscribeAsync(kvp.Key, kvp.Value);
                //    }

                //    foreach (var kvp in this.ActiveKeyedSubscriptions)
                //    {
                //        foreach (var subKvp in kvp.Value)
                //        {
                //            this.SubscribeAsync(kvp.Key, subKvp.Key, subKvp.Value);
                //        }
                //    }

                //    this._subscriptions = null;
                //}
                return connection;
            }

            /// <summary>
            /// Creates a subscription to a channel.
            /// </summary>
            /// <param name="channel">The channel of the subscription.</param>
            /// <param name="subscriptionCallback">The callback method.</param>
            /// <returns></returns>
            public Task SubscribeAsync(string channel, Action<string> subscriptionCallback)
            {
                // Check to make sure we're not re-creating an existing subscription first.
                if (this.ActiveKeylessSubscriptions.ContainsKey(channel))
                {
                    throw new ChannelAlreadySubscribedException(
                        string.Format("subscription to channel {0} already exists", channel)
                    );
                }

                // Let's add this subscription to our cache. We'll need to remember it so that we can re-subscribe
                // if we lose our connection at any point.
                this.ActiveKeylessSubscriptions[channel] = subscriptionCallback;
                return this.Subscriptions.SubscribeAsync(channel, (sameAsChannel, data) =>
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
            public Task SubscribeAsync(string channel, string key, Action<string> subscriptionCallback)
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

                return this.Subscriptions.SubscribeAsync(channel, (sameAsChannel, data) =>
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
            public Task<long> PublishAsync(string channel, string value)
            {
                return this.Subscriptions
                    .PublishAsync(channel, value);
            }

            /// <summary>
            /// Publishes a message to a channel for a specific key.
            /// </summary>
            /// <param name="channel">The channel to publish to.</param>
            /// <param name="key">The key to target.</param>
            /// <param name="value">The value of the message.</param>
            /// <returns></returns>
            public Task<long> PublishAsync(string channel, string key, string value)
            {
                string concat = key + ":" + value;
                return this.Subscriptions
                    .PublishAsync(channel, concat);
            }
        }

        public static void Configure(ConnectionMultiplexer connection)
        {
            PubSubSingleton.Instance.Init(connection);
        }

        /// <summary>
        /// Use this to separate your messages if you plan on sending multiple at once.
        /// </summary>
        public const string MULTIPLE_MESSAGE_SEPARATOR = "||";

        /// <summary>
        /// Creates a subscription to a channel.
        /// </summary>
        /// <param name="channel">The channel of the subscription.</param>
        /// <param name="subscriptionCallback">The callback method.</param>
        /// <returns></returns>
        public static Task Subscribe(string channel, Action<string> subscriptionCallback)
        {
            return PubSubSingleton.Instance.SubscribeAsync(channel, subscriptionCallback);
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
            return PubSubSingleton.Instance.SubscribeAsync(channel, key, subscriptionCallback);
        }

        /// <summary>
        /// Publishes a message to a channel.
        /// </summary>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="value">The value of the message.</param>
        /// <returns></returns>
        public static Task<long> Publish(string channel, string value)
        {
            return PubSubSingleton.Instance.PublishAsync(channel, value);
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
            return PubSubSingleton.Instance.PublishAsync(channel, key, value);
        }
    }
}