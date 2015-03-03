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
        /// The connections that we can write to. (Master(s))
        /// </summary>
        private List<ConnectionMultiplexer> _writeConnections;

        /// <summary>
        /// The slaves we can read from. (Slave(s))
        /// </summary>
        private List<ConnectionMultiplexer> _readConnections;

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
        }

        /// <summary>
        /// The name of this redis connection wrapper.
        /// </summary>
        private string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Adds a write connection.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        public void AddWriteConnection(ConnectionMultiplexer connection)
        {
            if (null == this._writeConnections)
            {
                this._writeConnections = new List<ConnectionMultiplexer> { connection };
            }
            else
            {
                this._writeConnections.Add(connection);
            }

            this.UpdateReadPool();
        }

        /// <summary>
        /// Adds a read connection.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        public void AddReadConnection(ConnectionMultiplexer connection)
        {
            if (null == this._readConnections)
            {
                this._readConnections = new List<ConnectionMultiplexer> { connection };
            }
            else
            {
                this._readConnections.Add(connection);
            }

            this.UpdateReadPool();
        }

        /// <summary>
        /// Retrieves a connection from the read pool on a round-robin basis.
        /// </summary>
        /// <returns>
        /// The read <see cref="ConnectionMultiplexer"/>.
        /// </returns>
        public ConnectionMultiplexer GetReadConnection()
        {
            this._readPoolIndex = (this._readPoolIndex + 1) % this._readPool.Count;
            return this._readPool[this._readPoolIndex];
        }

        /// <summary>
        /// Retrieves the available write connections.
        /// </summary>
        /// <returns>
        /// The array of write <see>
        ///         <cref>ConnectionMultiplexer[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public ConnectionMultiplexer[] GetWriteConnections()
        {
            return this._writeConnections.ToArray();
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
            int writeCount = null == this._writeConnections ? 0 : this._writeConnections.Count;
            int readCount = null == this._readConnections ? 0 : this._readConnections.Count;

            this._readPool = new List<ConnectionMultiplexer>((writeCount * WRITE_WEIGHT) + (readCount * READ_WEIGHT));

            int readIndex = 0;
            int writeIndex = 0;

            while (readIndex < readCount || writeIndex < writeCount)
            {
                if (null != this._readConnections)
                {
                    for (int i = 0; i < READ_WEIGHT; i++)
                    {
                        this._readPool.Add(this._readConnections[readIndex]);
                    }
                }

                if (null != this._writeConnections)
                {
                    for (int i = 0; i < WRITE_WEIGHT; i++)
                    {
                        this._readPool.Add(this._writeConnections[writeIndex]);
                    }
                }

                ++readIndex;
                ++writeIndex;
            }
        }
    }
}