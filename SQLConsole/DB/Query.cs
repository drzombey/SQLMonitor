using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace SQLConsole.DB
{
    class Query : IDisposable
    {
        public string SQL { get; set; }
        private readonly MySqlConnection _connection;

        public Query()
        {
            _connection = new MySqlConnection();
        }
        ~ Query()
        {
            Dispose(false);
        }

        public void Add(string query)
        {
            SQL += query;
        }
        public void Clear()
        {
            SQL = "";
        }
        public bool Connect(string connectionString)
        {
            try
            {
                _connection.ConnectionString = connectionString;
                _connection.Open();
                return _connection.Ping();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                throw;
            }

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
            try
            {
                var cmd = new MySqlCommand
                {
                    CommandText = SQL,
                    Connection = _connection
                };
                return cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public MySqlDataReader Open()
        {
            try
            {
                var cmd = new MySqlCommand
                {
                    CommandText = SQL,
                    Connection = _connection
                };
                var reader = cmd.ExecuteReader();
                return reader;
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
