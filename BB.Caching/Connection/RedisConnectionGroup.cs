using System.Collections.Generic;
using StackExchange.Redis;

namespace BB.Caching.Connection
{
    /// <summary>
    /// A group of connections for Redis.
    /// <remarks>
    /// Used to group a master-slave setup so that we can treat the master as the single write connection
    /// (/ read connection) and the slave(s) as the read connection(s).
    /// </remarks>
    /// </summary>
    public class RedisConnectionGroup
    {
        /// <summary>
        /// The name of this redis connection wrapper.
        /// </summary>
        private string Name
        {
            get;
            set;
        }

        /// <summary>
        /// When creating the read pool, read connections are added with this weight.
        /// </summary>
        private const int _readWeight = 2;

        /// <summary>
        /// When creating the read pool, write connections are added with this weight.
        /// </summary>
        private const int _writeWeight = 1;

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
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultWriteConnection"></param>
        public RedisConnectionGroup(string name, ConnectionMultiplexer defaultWriteConnection)
        {
            this.Name = name;
            this.AddWriteConnection(defaultWriteConnection);
        }

        /// <summary>
        /// Adds a write connection.
        /// </summary>
        /// <param name="connection"></param>
// ReSharper disable MemberCanBePrivate.Global
        public void AddWriteConnection(ConnectionMultiplexer connection)
// ReSharper restore MemberCanBePrivate.Global
        {
            if (null == this._writeConnections)
                this._writeConnections = new List<ConnectionMultiplexer> {connection};
            else
                this._writeConnections.Add(connection);

            this.UpdateReadPool();
        }

        /// <summary>
        /// Adds a read connection.
        /// </summary>
        /// <param name="connection"></param>
// ReSharper disable UnusedMember.Global
        public void AddReadConnection(ConnectionMultiplexer connection)
// ReSharper restore UnusedMember.Global
        {
            if (null == this._readConnections)
                this._readConnections = new List<ConnectionMultiplexer> {connection};
            else
                this._readConnections.Add(connection);

            this.UpdateReadPool();
        }

        /// <summary>
        /// Updates the read pool with connections.
        /// </summary>
        private void UpdateReadPool()
        {
            int writeCount = null == this._writeConnections ? 0 : this._writeConnections.Count;
            int readCount = null == this._readConnections ? 0 : this._readConnections.Count;

            this._readPool = new List<ConnectionMultiplexer>(writeCount*_writeWeight + readCount*_readWeight);

            int readIndex = 0;
            int writeIndex = 0;

            while (readIndex < readCount || writeIndex < writeCount)
            {
                if (null != this._readConnections)
                {
                    for (int i = 0; i < _readWeight; i++)
                        this._readPool.Add(this._readConnections[readIndex]);
                }

                if (null != this._writeConnections)
                {
                    for (int i = 0; i < _writeWeight; i++)
                        this._readPool.Add(this._writeConnections[writeIndex]);
                }

                ++readIndex;
                ++writeIndex;
            }
        }

        /// <summary>
        /// Retrieves a connection from the read pool on a round-robin basis.
        /// </summary>
        /// <returns></returns>
        public ConnectionMultiplexer GetReadConnection()
        {
            this._readPoolIndex = (this._readPoolIndex + 1)%this._readPool.Count;
            return this._readPool[this._readPoolIndex];
        }

        /// <summary>
        /// Retrieves the available write connections.
        /// </summary>
        /// <returns></returns>
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
    }
}