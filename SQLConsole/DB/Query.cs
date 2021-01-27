using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace SQLConsole.DB
{
    class Query : IDisposable
    {
        public Sql SQL { get; set; }
        public string ConnectionString { get; set; }
        private readonly MySqlConnection _connection;
        private MySqlDataReader _reader;
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
                _reader = _command.ExecuteReader();
                return _reader;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Close()
        {
            if (_reader != null)
            {
                if (_reader.IsClosed)
                {
                    return;
                }

                _reader.Close();
            }
        }

        public bool IsOperationalSqlStatement()
        {
            var sql = SQL.GetSQL();

            List<string> statementTypes = new List<string>()
            {
                "INSERT", "UPDATE", "DELETE"
            };

            foreach (var statementType in statementTypes)
            {
                var result = sql.ToLower().StartsWith(statementType.ToLower());
                if (result)
                {
                    return true;
                }
            }

            return false;
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
