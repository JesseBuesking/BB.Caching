namespace BB.Caching
{
    using System.Configuration;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// A class to manage configuration via app config.
    /// </summary>
    public class Configuration : ConfigurationSection
    {
        /// <summary>
        /// The instances of connection groups.
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ConnectionGroupCollection ConnectionGroups
        {
            get
            {
                return (ConnectionGroupCollection)this[string.Empty];
            }
        }
    }

    /// <summary>
    /// A grouping of connections for a node.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class ConnectionGroupCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets the collection type.
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        /// <summary>
        /// Gets the element name.
        /// </summary>
        protected override string ElementName
        {
            get
            {
                return "connectionGroup";
            }
        }

        /// <summary>
        /// Creates a new connection group element.
        /// </summary>
        /// <returns>
        /// The <see cref="ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new GroupElement();
        }

        /// <summary>
        /// Gets the key for an element.
        /// </summary>
        /// <param name="element">
        /// An instance of a <see cref="ConfigurationElement"/>.
        /// </param>
        /// <returns>
        /// A <see cref="GroupElement"></see>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GroupElement)element).Name;
        }
    }

    /// <summary>
    /// An element representing a connection group.
    /// </summary>
    public class GroupElement : ConfigurationElement
    {
        /// <summary>
        /// A grouping of connections for a particular node.
        /// </summary>
        [ConfigurationProperty("connections", IsDefaultCollection = false)]
        public ConnectionCollection ConnectionCollections
        {
            get
            {
                return (ConnectionCollection)base["connections"];
            }
        }

        /// <summary>
        /// The name to associate to the connection group.
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
        }

        /// <summary>
        /// Whether the connection group should be assigned to the analytics connection group. If it is, then it won't
        /// be used as a general-purpose connection group for caching. If you want both, have two connection groups
        /// with the same connections, one with the analytics property and one without.
        /// </summary>
        [ConfigurationProperty("analytics")]
        public bool IsAnalytics
        {
            get
            {
                return (bool)base["analytics"];
            }
        }
    }

    /// <summary>
    /// A collection of connections for a group.
    /// </summary>
    public class ConnectionCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets the collection type.
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        /// <summary>
        /// Gets the element name.
        /// </summary>
        protected override string ElementName
        {
            get
            {
                return "connection";
            }
        }

        /// <summary>
        /// Creates a new connection group element.
        /// </summary>
        /// <returns>
        /// The <see cref="ConnectionElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnectionElement();
        }

        /// <summary>
        /// Gets the key for an element.
        /// </summary>
        /// <param name="element">
        /// An instance of a <see cref="ConfigurationElement"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectionElement"></see>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConnectionElement)element).Name;
        }
    }

    /// <summary>
    /// An element representing a connection.
    /// </summary>
    public class ConnectionElement : ConfigurationElement
    {
        /// <summary>
        /// The connection string for the current connection.
        /// </summary>
        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString
        {
            get
            {
                return (string)base["connectionString"];
            }
        }

        /// <summary>
        /// The type of the connection (read or write).
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                var type = (string)base["type"];

                Debug.Assert(
                    new[]
                    {
                        "read", "write"
                    }.Contains(type),
                    "type must be one of read, write");

                return type;
            }
        }

        /// <summary>
        /// The unique name for this connection.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
        }
    }
}
