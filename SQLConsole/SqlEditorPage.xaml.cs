using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using SQLConsole.DB;

namespace SQLConsole
{
    /// <summary>
    /// Interaktionslogik für SqlEditorPage.xaml
    /// </summary>
    public partial class SqlEditorPage : Page
    {
        private readonly string _databaseName;
        private Query _globalQuery;
        public SqlEditorPage(string databaseName)
        {
            InitializeComponent();
            _databaseName = databaseName;
            _globalQuery = new Query(ConnectionDataObject.Instance.ConnectionStringToServer + $"Database={_databaseName}");
        }

        ~SqlEditorPage()
        {
            _globalQuery.Disconnect();
        }

        private void OnRunClick(object sender, RoutedEventArgs e)
        {
            RunQuery(new TextRange(rtSqlEditor.Document.ContentStart, rtSqlEditor.Document.ContentEnd).Text);
        }

        private void RunQuery(string sql)
        {
            try
            {
                _globalQuery.SQL.Add(sql);

                if (_globalQuery.IsOperationalSqlStatement())
                {
                    var rowCount = _globalQuery.ExecSql();
                    WriteLog($"{rowCount} rows updated!");
                    return;
                }
                tabItemDataTable.IsSelected = true;
                RenderDataTable(_globalQuery.Open());
                _globalQuery.Close();
            }
            catch (MySqlException exception)
            {
                WriteLog(exception.ToString());
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
