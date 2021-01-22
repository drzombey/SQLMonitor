using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace SQLConsole.DB
{
    class Query : IDisposable
    {
        public Sql SQL { get; set; }
        public string ConnectionString { get; set; }
        private readonly MySqlConnection _connection;
        private MySqlCommand _command;

        public Query(string connectionString)
        {
            ConnectionString = connectionString;

            SQL = new Sql();

            _connection = new MySqlConnection
            {
                ConnectionString = connectionString
            };

            _command = new MySqlCommand();
            _command.Connection = _connection;

            _connection.Open();
        }

        ~ Query()
        {
            Dispose(false);
        }

        private bool Connect()
        {
            try
            {
                _connection.ConnectionString = ConnectionString;
                _connection.Open();
                return _connection.Ping();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool GetConnectionState()
        {
            return _connection.Ping();
        }

        public void Disconnect()
        {
            try
            {
                _connection.Close();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public int ExecSql()
        {
            if (!_connection.Ping())
            {
                Connect();
            }

            try
            {
                _command.CommandText = SQL.GetSQL();
                return _command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public MySqlDataReader Open()
        {
            if (!_connection.Ping())
            {
                Connect();
            }

            try
            {
                _command.CommandText = SQL.GetSQL();
                return _command.ExecuteReader();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _connection?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
