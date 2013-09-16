using System.Collections.Generic;
using System.Linq;
using BookSleeve;

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
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// When creating the read pool, read connections are added with this weight.
        /// </summary>
        public int ReadWeight = 2;

        /// <summary>
        /// When creating the read pool, write connections are added with this weight.
        /// </summary>
        public int WriteWeight = 1;

        /// <summary>
        /// The connections that we can write to. (Master(s))
        /// </summary>
        private List<SafeRedisConnection> _writeConnections;

        /// <summary>
        /// The slaves we can read from. (Slave(s))
        /// </summary>
        private List<SafeRedisConnection> _readConnections;

        /// <summary>
        /// The pool of read connections to select from.
        /// <remarks>
        /// Will include both read and write connections, and will round-robin on the available connections.
        /// </remarks>
        /// </summary>
        private List<SafeRedisConnection> _readPool;

        /// <summary>
        /// The last-used read pool connection. This is used for round-robining the connections.
        /// </summary>
        private int _readPoolIndex = -1;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultWriteConnection"></param>
        public RedisConnectionGroup(string name, SafeRedisConnection defaultWriteConnection)
        {
            this.Name = name;
            this.AddWriteConnection(defaultWriteConnection);
        }

        /// <summary>
        /// Adds a write connection.
        /// </summary>
        /// <param name="connection"></param>
        public void AddWriteConnection(SafeRedisConnection connection)
        {
            if (null == this._writeConnections)
                this._writeConnections = new List<SafeRedisConnection> {connection};
            else
                this._writeConnections.Add(connection);

            this.UpdateReadPool();
        }

        /// <summary>
        /// Adds a read connection.
        /// </summary>
        /// <param name="connection"></param>
        public void AddReadConnection(SafeRedisConnection connection)
        {
            if (null == this._readConnections)
                this._readConnections = new List<SafeRedisConnection> {connection};
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

            this._readPool = new List<SafeRedisConnection>(writeCount*this.WriteWeight + readCount*this.ReadWeight);

            int readIndex = 0;
            int writeIndex = 0;

            while (readIndex < readCount || writeIndex < writeCount)
            {
                if (null != this._readConnections)
                {
                    for (int i = 0; i < this.ReadWeight; i++)
                        this._readPool.Add(this._readConnections[readIndex]);
                }

                if (null != this._writeConnections)
                {
                    for (int i = 0; i < this.WriteWeight; i++)
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
        public RedisConnection GetReadConnection()
        {
            this._readPoolIndex = (this._readPoolIndex + 1)%this._readPool.Count;
            return this._readPool[this._readPoolIndex].GetConnection();
        }

        /// <summary>
        /// Retrieves the available write connections.
        /// </summary>
        /// <returns></returns>
        public RedisConnection[] GetWriteConnections()
        {
            return this._writeConnections.Select(s => s.GetConnection()).ToArray();
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