using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;

namespace SQLConsole
{
    public class DatabaseItem
    {
        public DatabaseItem()
        {
            this.Tables = new ObservableCollection<TableItem>();
        }

        public string DatabaseName { get; set; }

        public ObservableCollection<TableItem> Tables { get; set; }

    }

    public class TableItem
    {
        public string TableName { get; set; }
    }
}
