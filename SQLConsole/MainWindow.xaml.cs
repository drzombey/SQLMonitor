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
using System.Windows.Media;
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

            var connectionObject = ConnectionDataObject.Instance;
            
            tbHostname.Text = connectionObject.Hostname;
            tbPort.Text = connectionObject.Port.ToString();
            tbUsername.Text = connectionObject.Username;

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

            SetClickHandler();
            SetLoginTextBoxStates(true);
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
                throw;
            }
        }

        private void ClearTreeView()
        {
            tvDatabases.Items.Clear();
        }

        #endregion

        #region DataView


        #endregion

        #region LogicalFunctions

        #endregion

        private void TvDatabases_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = tvDatabases.SelectedItem;
            if (tvDatabases.Items.Count == 0 || item == null)
            {
                return;
            }

            if (item.GetType() == typeof(DatabaseItem))
            {
                var databaseItem = (DatabaseItem) item;
                OpenNewEditorFrame(databaseItem.DatabaseName);
            }
        }

        private void OpenNewEditorFrame(string database)
        {
            var sqlEditorPage = new SqlEditorPage(database)
            {
                ShowsNavigationUI = false
            };

            var frame = new Frame()
            {
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.LightGray,
                Content = sqlEditorPage
            };

            var tabItem = new TabItem()
            {
                Content = frame,
                Header = $"DB: {database}"
            };

            tabControl.Items.Add(tabItem);
        }
    }
}
