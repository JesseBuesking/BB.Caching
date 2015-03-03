// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using BB.Caching.Redis;

    /// <summary>
    /// Contains the core methods for caching data in memory, redis, or both.
    /// </summary>
    public static partial class Cache
    {
        /// <summary>
        /// Calls any necessary preparation logic by the caching system. Before calling this, you need to either:
        /// 1. Programmatically set up your connections before calling this, manually adding connection groups by
        ///    calling SharedCache.Instance.AddRedisConnectionGroup(...);
        /// 2. Call Cache.LoadFromConfig(...) to load your settings from a configuration file.
        /// </summary>
        public static void Prepare()
        {
            // free of side-effects
            Cache.SubscribeCacheDeleteChannel();
            RateLimiter.ScriptLoad();
            Statistics.ScriptLoad();
        }

        /// <summary>
        /// Loads BB.Caching settings from you app config file, or from the <paramref name="section"/> supplied.
        /// </summary>
        /// <param name="section">
        /// An optional <see cref="ConfigurationSection"/> to load configuration data from. If this is left as null,
        /// the method will try to load your configuration settings from the current projects app config file.
        /// </param>
        /// <param name="establishConnections">
        /// Talks with the redis instances defined in the connection strings to establish connections. Testing classes
        /// will set this to false.
        /// </param>
        /// <returns>
        /// The <see cref="ConnectionGroup"/> objects that were loaded. This is returned for testing purposes.
        /// </returns>
        public static List<ConnectionGroup> LoadFromConfig(
            ConfigurationSection section = null, bool establishConnections = true)
        {
            var connectionGroups = new List<ConnectionGroup>();

            // if the section is not supplied, look it up in the current configuration file
            if (section == null)
            {
                section = (ConfigurationSection)ConfigurationManager.GetSection("BB.Caching");
            }

            // try to cast the object to our configuration type
            var customSection = section as BB.Caching.Configuration;
            if (customSection == null)
            {
                return connectionGroups;
            }

            // create actual ConnectionGroup instances from the configuration data
            foreach (GroupElement group in customSection.ConnectionGroups)
            {
                string name = group.Name;
                var connectionGroup = new ConnectionGroup(name);
                connectionGroups.Add(connectionGroup);

                foreach (ConnectionElement connection in group.ConnectionCollections)
                {
                    string type = connection.Type;
                    string connectionString = connection.ConnectionString;

                    switch (type)
                    {
                        case "read":
                        {
                            connectionGroup.AddReadConnection(connectionString, establishConnections);
                            break;
                        }

                        case "write":
                        {
                            connectionGroup.AddWriteConnection(connectionString, establishConnections);
                            break;
                        }

                        default:
                        {
                            throw new Exception(string.Format("type {0} is not supported", type));
                        }
                    }
                }
            }

            // load each ConnectionGroup for use
            foreach (var group in connectionGroups)
            {
                SharedCache.Instance.AddRedisConnectionGroup(group);
            }

            return connectionGroups;
        }
    }
}