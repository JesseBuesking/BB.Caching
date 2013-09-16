using System;
using System.Threading.Tasks;
using BookSleeve;

namespace BB.Caching.Connection
{
    /// <summary>
    /// A Redis connection that takes care of re-creating itself when it closes and guarantees to be open.
    /// </summary>
    public class SafeRedisConnection : IDisposable
    {
        private readonly string _host;

        private readonly int _port;

        private readonly int _ioTimeout;

        private readonly string _password;

        private readonly int _maxUnsent;

        private readonly bool _allowAdmin;

        private readonly int _syncTimeout;

        private RedisConnection _connection;

        private readonly Object _lock = new Object();

        public SafeRedisConnection(string host, int port = 6379, int ioTimeout = -1, string password = null,
            int maxUnsent = 2147483647, bool allowAdmin = false, int syncTimeout = 10000)
        {
            this._host = host;
            this._port = port;
            this._ioTimeout = ioTimeout;
            this._password = password;
            this._maxUnsent = maxUnsent;
            this._allowAdmin = allowAdmin;
            this._syncTimeout = syncTimeout;

            this.CreateConnection();
        }

        private bool CreateConnection() {
            lock (this._lock)
            {
                if (null != this._connection)
                    this._connection.Dispose();

                this._connection = new RedisConnection(this._host, this._port, this._ioTimeout, this._password,
                    this._maxUnsent, this._allowAdmin, this._syncTimeout);

                if (this._connection.State != RedisConnectionBase.ConnectionState.New)
                    throw new Exception("connection should be new");

                Task openAsync = this._connection.Open();

                try
                {
                    this._connection.Wait(openAsync);
                }
                catch (TimeoutException)
                {
                    return false;
                }
            }
            return true;
        }

        public RedisConnection GetConnection()
        {
            if (this._connection.State != RedisConnectionBase.ConnectionState.Open)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (this.CreateConnection())
                        break;
                }
            }

            return this._connection;
        }

        public void Dispose()
        {
            this._connection.Dispose();
        }
    }
}