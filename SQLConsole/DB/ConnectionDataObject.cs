using System;
using System.Collections.Generic;
using System.Text;

namespace SQLConsole.DB
{
    public class ConnectionDataObject
    {
        public ConnectionDataObject()
        {
            Hostname = "localhost";
            Port = 3306;
            Username = String.Empty;
            Password = String.Empty;
        }

        private static ConnectionDataObject _instance = null;

        public static ConnectionDataObject Instance
        {
            get
            {
                lock (PadLock)
                {
                    return _instance ??= new ConnectionDataObject();
                }
            }
        }

        private static readonly object PadLock = new object();
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectionStringToServer => $"Server={Hostname};Port={Port};Uid='{Username}';Password='{Password}';";
    }
}
