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
            btnRun.Click += OnRunClick;
            btnRefresh.Click += OnRefreshDatabasesClick;
            SetClickHandler();
        }

        #region StateHandling

        private void SetLoginTextBoxStates(bool state)
        {
            pbPassword.IsEnabled = state;
            tbHostname.IsEnabled = state;
            tbUsername.IsEnabled = state;
            tbPort.IsEnabled = state;
        }

        #endregion

        #region ClickHandling

        private void SetClickHandler(bool connectionState = false)
        {
            if (connectionState)
            {
                btnConnect.Click -= OnConnectClick;
                btnConnect.Click += OnDisconnectClick;
            }
            else
            {
                btnConnect.Click += OnConnectClick;
                btnConnect.Click -= OnDisconnectClick;
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

            var query = new Query(_connectionStringToServer);
            _queryList.Add("globalQuery", query);

            try
            {
                var state = !query.GetConnectionState();

                btnConnect.Content = "Disconnect";
                btnRun.IsEnabled = !state;

                SetClickHandler(!state);
                SetLoginTextBoxStates(state);

                ReadDatabases(query);
            }
            catch (Exception exception)
            {
                _queryList.Remove("globalQuery");
                Console.WriteLine(exception);
                throw;
            }
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
            SetLoginTextBoxStates(true);
        }

        private void OnRunClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DatabaseItem db = (DatabaseItem)tvDatabases.SelectedItem;

                if (db == null)
                {
                    WriteLog("Bitte wählen Sie eine Datenbak aus!");
                    return;
                }

                var query = new Query(_connectionStringToServer + $"Database={db.DatabaseName}");
                query.SQL.Add(new TextRange(rtSqlEditor.Document.ContentStart, rtSqlEditor.Document.ContentEnd).Text);

                if (query.IsOperationalSqlStatement())
                {
                    var rowCount = query.ExecSql();
                    WriteLog($"{rowCount} rows updated!");
                    return;
                }

                var reader = query.Open();
                
                RenderDataTable(reader);
                reader.Close();
            }
            catch (MySqlException exception)
            {
                WriteLog(exception.ToString());
            }

        }

        private void OnRefreshDatabasesClick(object sender, RoutedEventArgs e)
        {
            ClearTreeView();

            var query = _queryList["globalQuery"];
            ReadDatabases(query);
        }

        private void TvDatabases_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //TODO
        }

        #endregion

        #region TreeView

        private void AddDatabasesToTreeView(MySqlDataReader reader)
        {
            ClearTreeView();
            while (reader.Read())
            {
                var datatbasename = reader.GetString("database");
                var query = new Query(_connectionStringToServer);
                
                query.SQL.Clear();
                query.SQL.Add($"USE {datatbasename}; SHOW TABLES;");
                var tableReader = query.Open();
                var tableList = new ObservableCollection<TableItem>();

                while (tableReader.Read())
                {
                    var table = tableReader.GetString($"Tables_in_{datatbasename}");
                    tableList.Add(new TableItem()
                    {
                        TableName = table
                    });
                }
                tableReader.Close();

                query.Disconnect();

                tvDatabases.Items.Add(
                    new DatabaseItem()
                    {
                        DatabaseName = reader.GetString("database"),
                        Tables = tableList
                    });
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
                query.SQL.Clear();
                query.SQL.Add("SHOW DATABASES;");
                var reader = query.Open();
                AddDatabasesToTreeView(reader);
                reader.Close();
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

        #region LogicalFunctions

        #endregion
    }
}
