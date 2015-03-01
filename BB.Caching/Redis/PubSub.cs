namespace BB.Caching.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    /// Manages all pub-sub related workflows.
    /// </summary>
    public static class PubSub
    {
        /// <summary>
        /// Exception when a channel is already subscribed.
        /// </summary>
        public class ChannelAlreadySubscribedException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChannelAlreadySubscribedException"/> class.
            /// </summary>
            /// <param name="message">
            /// The message.
            /// </param>
            public ChannelAlreadySubscribedException(string message)
                : base(message)
            {
            }
        }

        /// <summary>
        /// A singleton to hold open a pub sub instance.
        /// </summary>
        private class PubSubSingleton
        {
            /// <summary>
            /// Lazily loads the instance.
            /// </summary>
            private static readonly Lazy<PubSubSingleton> _Lazy = new Lazy<PubSubSingleton>(
                () => new PubSubSingleton(), LazyThreadSafetyMode.ExecutionAndPublication);

            /// <summary>
            /// Gets the instance.
            /// </summary>
            public static PubSubSingleton Instance
            {
                get { return PubSubSingleton._Lazy.Value; }
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
                    {
                        // ReSharper restore ConvertIfStatementToNullCoalescingExpression
                        this._subscriptions = this.GetConnection().GetSubscriber();
                    }

                    return this._subscriptions;
                }
            }

            /// <summary>
            /// The subscriptions.
            /// </summary>
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

            /// <summary>
            /// Prevents a default instance of the <see cref="PubSubSingleton"/> class from being created.
            /// </summary>
            private PubSubSingleton()
            {
                this.ActiveKeyedSubscriptions = new Dictionary<string, Dictionary<string, Action<string>>>();
                this.ActiveKeylessSubscriptions = new Dictionary<string, Action<string>>();
            }

            /// <summary>
            /// Configures the singleton.
            /// </summary>
            /// <param name="connection">
            /// The connection.
            /// </param>
            public void Init(ConnectionMultiplexer connection)
            {
                if (connection == null)
                {
                    throw new ArgumentNullException("connection");
                }

                this._pubSubConnection = connection;
            }

            /// <summary>
            /// Creates a subscription to a channel.
            /// </summary>
            /// <param name="channel">
            /// The channel of the subscription.
            /// </param>
            /// <param name="subscriptionCallback">
            /// The callback method.
            /// </param>
            public void Subscribe(string channel, Action<string> subscriptionCallback)
            {
                // Check to make sure we're not re-creating an existing subscription first.
                if (this.ActiveKeylessSubscriptions.ContainsKey(channel))
                {
                    throw new ChannelAlreadySubscribedException(
                        string.Format("subscription to channel {0} already exists", channel));
                }

                // Let's add this subscription to our cache. We'll need to remember it so that we can re-subscribe
                // if we lose our connection at any point.
                this.ActiveKeylessSubscriptions[channel] = subscriptionCallback;
                this.Subscriptions.Subscribe(
                    channel,
                    (sameAsChannel, data) =>
                    {
                        string value = Encoding.UTF8.GetString(data);
                        subscriptionCallback(value);
                    });
            }

            /// <summary>
            /// Creates a subscription to a channel.
            /// </summary>
            /// <param name="channel">
            /// The channel of the subscription.
            /// </param>
            /// <param name="subscriptionCallback">
            /// The callback method.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/> (void).
            /// </returns>
            public Task SubscribeAsync(string channel, Action<string> subscriptionCallback)
            {
                // Check to make sure we're not re-creating an existing subscription first.
                if (this.ActiveKeylessSubscriptions.ContainsKey(channel))
                {
                    throw new ChannelAlreadySubscribedException(
                        string.Format("subscription to channel {0} already exists", channel));
                }

                // Let's add this subscription to our cache. We'll need to remember it so that we can re-subscribe
                // if we lose our connection at any point.
                this.ActiveKeylessSubscriptions[channel] = subscriptionCallback;
                return this.Subscriptions.SubscribeAsync(
                    channel,
                    (sameAsChannel, data) =>
                    {
                        string value = Encoding.UTF8.GetString(data);
                        subscriptionCallback(value);
                    });
            }

            /// <summary>
            /// Creates a subscription to a channel for a specific key.
            /// </summary>
            /// <param name="channel">
            /// The channel of the subscription.
            /// </param>
            /// <param name="key">
            /// The key to target.
            /// </param>
            /// <param name="subscriptionCallback">
            /// The callback method.
            /// </param>
            public void Subscribe(string channel, string key, Action<string> subscriptionCallback)
            {
                // Check to make sure we're not re-creating an existing subscription first.
                Dictionary<string, Action<string>> result;
                if (this.ActiveKeyedSubscriptions.TryGetValue(channel, out result))
                {
                    if (result.ContainsKey(key))
                    {
                        throw new Exception(
                            string.Format("subscription to channel {0} for key {1} already exists", channel, key));
                    }
                }

                // Let's add this subscription to our cache. We'll need to remember it so that we can re-subscribe
                // if we lose our connection at any point.
                if (!this.ActiveKeyedSubscriptions.ContainsKey(channel))
                {
                    this.ActiveKeyedSubscriptions[channel] = new Dictionary<string, Action<string>>();
                }

                this.ActiveKeyedSubscriptions[channel][key] = subscriptionCallback;

                this.Subscriptions.Subscribe(
                    channel,
                    (sameAsChannel, data) =>
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
            /// Creates a subscription to a channel for a specific key.
            /// </summary>
            /// <param name="channel">
            /// The channel of the subscription.
            /// </param>
            /// <param name="key">
            /// The key to target.
            /// </param>
            /// <param name="subscriptionCallback">
            /// The callback method.
            /// </param>
            /// <returns>
            /// The <see cref="Task"/> (void).
            /// </returns>
            public Task SubscribeAsync(string channel, string key, Action<string> subscriptionCallback)
            {
                // Check to make sure we're not re-creating an existing subscription first.
                Dictionary<string, Action<string>> result;
                if (this.ActiveKeyedSubscriptions.TryGetValue(channel, out result))
                {
                    if (result.ContainsKey(key))
                    {
                        throw new Exception(
                            string.Format("subscription to channel {0} for key {1} already exists", channel, key));
                    }
                }

                // Let's add this subscription to our cache. We'll need to remember it so that we can re-subscribe
                // if we lose our connection at any point.
                if (!this.ActiveKeyedSubscriptions.ContainsKey(channel))
                {
                    this.ActiveKeyedSubscriptions[channel] = new Dictionary<string, Action<string>>();
                }

                this.ActiveKeyedSubscriptions[channel][key] = subscriptionCallback;

                return this.Subscriptions.SubscribeAsync(
                    channel,
                    (sameAsChannel, data) =>
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
                        {
                            return;
                        }

                        string pubData = value.Substring(idx + 1, value.Length - (idx + 1));
                        subscriptionCallback(pubData);
                    });
            }

            /// <summary>
            /// Publishes a message to a channel.
            /// </summary>
            /// <param name="channel">The channel to publish to.</param>
            /// <param name="value">The value of the message.</param>
            /// <returns>
            /// The number of clients that received the message.
            /// </returns>
            public long Publish(string channel, string value)
            {
                return this.Subscriptions
                    .Publish(channel, value);
            }

            /// <summary>
            /// Publishes a message to a channel.
            /// </summary>
            /// <param name="channel">
            /// The channel to publish to.
            /// </param>
            /// <param name="value">
            /// The value of the message.
            /// </param>
            /// <returns>
            /// The number of clients that received the message.
            /// </returns>
            public Task<long> PublishAsync(string channel, string value)
            {
                return this.Subscriptions
                    .PublishAsync(channel, value);
            }

            /// <summary>
            /// Publishes a message to a channel for a specific key.
            /// </summary>
            /// <param name="channel">
            /// The channel to publish to.
            /// </param>
            /// <param name="key">
            /// The key to target.
            /// </param>
            /// <param name="value">
            /// The value of the message.
            /// </param>
            /// <returns>
            /// The number of clients that received the message.
            /// </returns>
            public long Publish(string channel, string key, string value)
            {
                string concat = key + ":" + value;
                return this.Subscriptions
                    .Publish(channel, concat);
            }

            /// <summary>
            /// Publishes a message to a channel for a specific key.
            /// </summary>
            /// <param name="channel">
            /// The channel to publish to.
            /// </param>
            /// <param name="key">
            /// The key to target.
            /// </param>
            /// <param name="value">
            /// The value of the message.
            /// </param>
            /// <returns>
            /// The number of clients that received the message.
            /// </returns>
            public Task<long> PublishAsync(string channel, string key, string value)
            {
                string concat = key + ":" + value;
                return this.Subscriptions
                    .PublishAsync(channel, concat);
            }

            /// <summary>
            /// Gets a connection, automatically re-establishing all active subscriptions if the connection was
            /// re-created.
            /// </summary>
            /// <returns>The current connection.</returns>
            private ConnectionMultiplexer GetConnection()
            {
                var connection = this._pubSubConnection;
                return connection;
            }
        }

        /// <summary>
        /// Configures the pub sub instance.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
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
        public static void Subscribe(string channel, Action<string> subscriptionCallback)
        {
            PubSubSingleton.Instance.Subscribe(channel, subscriptionCallback);
        }

        /// <summary>
        /// Creates a subscription to a channel.
        /// </summary>
        /// <param name="channel">The channel of the subscription.</param>
        /// <param name="subscriptionCallback">The callback method.</param>
        /// <returns></returns>
        public static Task SubscribeAsync(string channel, Action<string> subscriptionCallback)
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
        public static void Subscribe(string channel, string key, Action<string> subscriptionCallback)
        {
            PubSubSingleton.Instance.Subscribe(channel, key, subscriptionCallback);
        }

        /// <summary>
        /// Creates a subscription to a channel for a specific key.
        /// </summary>
        /// <param name="channel">The channel of the subscription.</param>
        /// <param name="key">The key to target.</param>
        /// <param name="subscriptionCallback">The callback method.</param>
        /// <returns></returns>
        public static Task SubscribeAsync(string channel, string key, Action<string> subscriptionCallback)
        {
            return PubSubSingleton.Instance.SubscribeAsync(channel, key, subscriptionCallback);
        }

        /// <summary>
        /// Publishes a message to a channel.
        /// </summary>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="value">The value of the message.</param>
        /// <returns></returns>
        public static long Publish(string channel, string value)
        {
            return PubSubSingleton.Instance.Publish(channel, value);
        }

        /// <summary>
        /// Publishes a message to a channel.
        /// </summary>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="value">The value of the message.</param>
        /// <returns></returns>
        public static Task<long> PublishAsync(string channel, string value)
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
        public static long Publish(string channel, string key, string value)
        {
            return PubSubSingleton.Instance.Publish(channel, key, value);
        }

        /// <summary>
        /// Publishes a message to a channel for a specific key.
        /// </summary>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="key">The key to target.</param>
        /// <param name="value">The value of the message.</param>
        /// <returns></returns>
        public static Task<long> PublishAsync(string channel, string key, string value)
        {
            return PubSubSingleton.Instance.PublishAsync(channel, key, value);
        }
    }
}