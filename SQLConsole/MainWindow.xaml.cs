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
using System.Windows.Input;
using Microsoft.VisualBasic.CompilerServices;
using MySql.Data.MySqlClient;
using SQLConsole.DB;


namespace SQLConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Query _globalQuery;

        public MainWindow()
        {
            InitializeComponent();
            btnRun.IsEnabled = false;

            var connectionObject = ConnectionDataObject.Instance;
            
            tbHostname.Text = connectionObject.Hostname;
            tbPort.Text = connectionObject.Port.ToString();
            tbUsername.Text = connectionObject.Username;

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
            var connectionObject = ConnectionDataObject.Instance;

            connectionObject.Hostname = tbHostname.Text;
            connectionObject.Port = int.Parse(tbPort.Text);
            connectionObject.Username = tbUsername.Text;
            connectionObject.Password = pbPassword.Password;

            _globalQuery = new Query(connectionObject.ConnectionStringToServer);

            try
            {
                var state = !_globalQuery.GetConnectionState();

                btnConnect.Content = "Disconnect";
                btnRun.IsEnabled = !state;

                SetClickHandler(!state);
                SetLoginTextBoxStates(state);

                ReadAndAddDatabasesWithTablesToTreeView();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private void OnDisconnectClick(object sender, RoutedEventArgs e)
        {
            ClearTreeView();

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

                var query = new Query(ConnectionDataObject.Instance.ConnectionStringToServer + $"Database={db.DatabaseName}");
                query.SQL.Add(new TextRange(rtSqlEditor.Document.ContentStart, rtSqlEditor.Document.ContentEnd).Text);

                if (query.IsOperationalSqlStatement())
                {
                    var rowCount = query.ExecSql();
                    WriteLog($"{rowCount} rows updated!");
                    return;
                }

                RenderDataTable(query.Open());
                query.Close();
            }
            catch (MySqlException exception)
            {
                WriteLog(exception.ToString());
            }

        }

        private void OnRefreshDatabasesClick(object sender, RoutedEventArgs e)
        {
            ClearTreeView();
            ReadAndAddDatabasesWithTablesToTreeView();
        }

        #endregion

        #region TreeView

        private void ReadAndAddDatabasesWithTablesToTreeView()
        {
            ClearTreeView();
            _globalQuery.SQL.Clear();
            _globalQuery.SQL.Add("SHOW DATABASES;");
            
            var reader = _globalQuery.Open();

            while (reader.Read())
            {
                var database = reader.GetString("database");

                tvDatabases.Items.Add(
                    new DatabaseItem()
                    {
                        DatabaseName = reader.GetString("database"),
                        Tables = ReadTablesFromDatabase(database)
                    });
            }

            _globalQuery.Close();
        }

        private ObservableCollection<TableItem> ReadTablesFromDatabase(string database)
        {
            var query = new Query(ConnectionDataObject.Instance.ConnectionStringToServer);

            try
            {
                query.SQL.Clear();
                query.SQL.Add($"USE {database}; SHOW TABLES;");

                var tableReader = query.Open();
                var tableList = new ObservableCollection<TableItem>();

                while (tableReader.Read())
                {
                    var table = tableReader.GetString($"Tables_in_{database}");
                    tableList.Add(new TableItem()
                    {
                        TableName = table
                    });
                }

                query.Close();
                query.Disconnect();

                return tableList;
            }
            catch (MySqlException exception)
            {
                WriteLog(exception.ToString());
                throw;
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

        private void WriteLog(string log)
        {
            rtConsoleLog.Document.Blocks.Clear();
            rtConsoleLog.Document.Blocks.Add(new Paragraph(new Run(log)));
        }

        #endregion

        #region LogicalFunctions

        #endregion

        private void TvDatabases_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (tvDatabases.Items.Count == 0)
            {
                return;
            }

            var item = tvDatabases.SelectedItem;

            if (item.GetType() == typeof(DatabaseItem))
            {
                var databaseItem = (DatabaseItem) item;
                var sqlEditorWindow = new SqlEditor(databaseItem.DatabaseName);
                sqlEditorWindow.Show();
            }
        }
    }
}
