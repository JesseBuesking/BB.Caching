namespace BB.Caching.Redis
{
    using System.Collections.Generic;

    using StackExchange.Redis;

    /// <summary>
    /// A group of connections for Redis.
    /// <remarks>
    /// Used to group a master-slave setup so that we can treat the master as the single write connection
    /// (/ read connection) and the slave(s) as the read connection(s).
    /// </remarks>
    /// </summary>
    public class ConnectionGroup
    {
        /// <summary>
        /// When creating the read pool, read connections are added with this weight.
        /// </summary>
        private const int READ_WEIGHT = 2;

        /// <summary>
        /// When creating the read pool, write connections are added with this weight.
        /// </summary>
        private const int WRITE_WEIGHT = 1;

        /// <summary>
        /// The pool of read connections to select from.
        /// <remarks>
        /// Will include both read and write connections, and will round-robin on the available connections.
        /// </remarks>
        /// </summary>
        private List<ConnectionMultiplexer> _readPool;

        /// <summary>
        /// The last-used read pool connection. This is used for round-robining the connections.
        /// </summary>
        private int _readPoolIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionGroup"/> class. 
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public ConnectionGroup(string name)
        {
            this.Name = name;
            this.ReadConnections = new List<string>();
            this.WriteConnections = new List<string>();
            this.ReadMultiplexers = new List<ConnectionMultiplexer>();
            this.WriteMultiplexers = new List<ConnectionMultiplexer>();
        }

        /// <summary>
        /// The name of this redis connection wrapper.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The multiplexers that we can write to. (Master(s))
        /// </summary>
        public List<ConnectionMultiplexer> WriteMultiplexers { get; private set; }

        /// <summary>
        /// The connections that we can write to. (Master(s))
        /// </summary>
        public List<string> WriteConnections { get; private set; }

        /// <summary>
        /// The slave multiplexers we can read from. (Slave(s))
        /// </summary>
        public List<ConnectionMultiplexer> ReadMultiplexers { get; private set; }

        /// <summary>
        /// The slave connections we can read from. (Slave(s))
        /// </summary>
        public List<string> ReadConnections { get; private set; }

        /// <summary>
        /// Adds a write connection.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <param name="establishConnection">
        /// Talks with the redis instance defined in the connection string to establish a connection.
        /// </param>
        public void AddWriteConnection(string connection, bool establishConnection = true)
        {
            this.WriteConnections.Add(connection);

            if (establishConnection)
            {
                var multiplexer = ConnectionMultiplexer.Connect(connection);
                this.WriteMultiplexers.Add(multiplexer);
            }

            this.UpdateReadPool();
        }

        /// <summary>
        /// Adds a read connection.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <param name="establishConnection">
        /// Talks with the redis instance defined in the connection string to establish a connection.
        /// </param>
        public void AddReadConnection(string connection, bool establishConnection = true)
        {
            this.ReadConnections.Add(connection);

            if (establishConnection)
            {
                var multiplexer = ConnectionMultiplexer.Connect(connection);
                this.ReadMultiplexers.Add(multiplexer);
            }

            this.UpdateReadPool();
        }

        /// <summary>
        /// Retrieves a connection from the read pool on a round-robin basis.
        /// </summary>
        /// <returns>
        /// The read <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer GetReadMultiplexer()
        {
            this._readPoolIndex = (this._readPoolIndex + 1) % this._readPool.Count;
            return this._readPool[this._readPoolIndex];
        }

        /// <summary>
        /// Retrieves the available write connections.
        /// </summary>
        /// <returns>
        /// The array of write <see><cref>ConnectionMultiplexer[]</cref></see>.
        /// </returns>
        public ConnectionMultiplexer[] GetWriteMultiplexers()
        {
            return this.WriteMultiplexers.ToArray();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Updates the read pool with connections.
        /// </summary>
        private void UpdateReadPool()
        {
            int writeCount = this.WriteMultiplexers.Count;
            int readCount = this.ReadMultiplexers.Count;

            this._readPool = new List<ConnectionMultiplexer>((writeCount * WRITE_WEIGHT) + (readCount * READ_WEIGHT));

            int readIndex = 0;
            int writeIndex = 0;

            while (readIndex < readCount || writeIndex < writeCount)
            {
                if (readCount > 0)
                {
                    for (int i = 0; i < READ_WEIGHT; i++)
                    {
                        this._readPool.Add(this.ReadMultiplexers[readIndex]);
                    }
                }

                if (writeCount > 0)
                {
                    for (int i = 0; i < WRITE_WEIGHT; i++)
                    {
                        this._readPool.Add(this.WriteMultiplexers[writeIndex]);
                    }
                }

                ++readIndex;
                ++writeIndex;
            }
        }
    }
}