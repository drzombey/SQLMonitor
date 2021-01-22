using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Data;
using MySql.Data.MySqlClient;
using SQLConsole.DB;


namespace SQLConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string hostname, port, username, password;
        private string _connectionstring;
        private List<string> _databases;
        public MainWindow()
        {
            InitializeComponent();
            _databases = new List<string>();
            btnRun.IsEnabled = false;
            tbHostname.Text = "localhost";
            tbPort.Text = "3306";
            tbUsername.Text = "root";
            SetClickHandler();
        }

        #region ClickHandling

        private void SetClickHandler(bool connectionState = false)
        {
            btnRun.Click += OnRunClick;
            btnConnect.Click += OnConnectClick;
            if (connectionState)
            {
                btnConnect.Click += OnDisconnectClick;
            }
        }

        private void OnConnectClick(object sender, RoutedEventArgs e)
        {
            hostname = tbHostname.Text;
            port = tbPort.Text;
            username = tbUsername.Text;
            password = pbPassword.Password;
            _connectionstring = $"Server={hostname};Port={port};Uid='{username}';Password='{password}';";
            using (var query = new Query())
            {
                var state = !query.Connect(_connectionstring);
                btnConnect.IsEnabled = state;
                btnRun.IsEnabled = !state;
                ReadDatabases(query);
            }
        }

        private void OnDisconnectClick(object sender, RoutedEventArgs e)
        {
            //TODO Disconnect routine
        }

        private void OnRunClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var query = new Query())
                {
                    DatabaseItem db = (DatabaseItem)tvDatabases.SelectedItem;
                    if (db == null)
                    {
                        WriteLog("Please select a database!");
                        return;
                    }
                    query.Connect(_connectionstring + $"Database={db.DatabaseName}");
                    query.Add(new TextRange(rtSqlEditor.Document.ContentStart, rtSqlEditor.Document.ContentEnd).Text);
                    var reader = query.Open();
                    RenderDataTable(reader);
                }
            }
            catch (MySqlException exception)
            {
                WriteLog(exception.ToString());
            }

        }

        #endregion



        private void ReadDatabases(Query query)
        {
            try
            {
                query.Add("SHOW DATABASES;");
                var reader = query.Open();
                //RenderDataTable(reader);
                AddDatabasesToTreeView(reader);
            }
            catch (MySqlException exception)
            {
                WriteLog(exception.ToString());
            }
        }

        private void AddDatabasesToTreeView(MySqlDataReader reader)
        {
            while (reader.Read())
            {
               var datatbasename = reader.GetString("database");
                tvDatabases.Items.Add(
                    new DatabaseItem()
                    {
                        DatabaseName = reader.GetString("database"),
                    });

                using (var query = new Query())
                {
                    query.Connect(_connectionstring + $"Database={datatbasename}");

                }
            }
        }

        private void RenderDataTable(MySqlDataReader reader)
        {
            try
            {
                DataTable dtTable = new DataTable();
                dtTable.Load(reader);
                dgTable.ItemsSource = dtTable.DefaultView;
            }
            catch (MySqlException exception)
            {
                WriteLog(exception.ToString());
            }
        }

        private void WriteLog(string log)
        {
            rtConsoleLog.Document.Blocks.Clear();
            rtConsoleLog.Document.Blocks.Add(new Paragraph(new Run(log)));
        }
    }
}
