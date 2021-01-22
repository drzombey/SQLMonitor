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
        private string _hostname, _port, _username, _password;
        private string _connectionStringToServer;
        private List<string> _databases;
        private Dictionary<string, Query> _queryList;
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
            _hostname = tbHostname.Text;
            _port = tbPort.Text;
            _username = tbUsername.Text;
            _password = pbPassword.Password;
            _connectionStringToServer = $"Server={_hostname};Port={_port};Uid='{_username}';Password='{_password}';";
            _queryList = new Dictionary<string, Query>();

            var query = new Query();
            _queryList.Add("serverQuery", query);
            var state = !query.Connect(_connectionStringToServer);
            btnConnect.Content = "Disconnect";
            SetClickHandler(!state);
            btnRun.IsEnabled = !state;
            ReadDatabases(query);
            
        }

        private void OnDisconnectClick(object sender, RoutedEventArgs e)
        {
            ClearTreeView();
            
            foreach (var pair in _queryList)
            {
                pair.Value.Disconnect();
            }

            btnConnect.Content = "Connect";
            btnRun.IsEnabled = false;
            SetClickHandler();
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
                    query.Connect(_connectionStringToServer + $"Database={db.DatabaseName}");
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

        #region TreeView

        private void AddDatabasesToTreeView(MySqlDataReader reader)
        {
            ClearTreeView();
            while (reader.Read())
            {
                var datatbasename = reader.GetString("database");
                tvDatabases.Items.Add(
                    new DatabaseItem()
                    {
                        DatabaseName = reader.GetString("database"),
                    });
                var query = new Query();
                query.Connect(_connectionStringToServer + $"Database={datatbasename}");
                _queryList.Add(datatbasename, query);
            }
        }

        private void ClearTreeView()
        {
            tvDatabases.Items.Clear();
        }

        #endregion

        #region DataView

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

        private void ReadDatabases(Query query)
        {
            try
            {
                query.Add("SHOW DATABASES;");
                var reader = query.Open();
                AddDatabasesToTreeView(reader);
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

        #endregion

    }
}
