using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SQLConsole
{
    /// <summary>
    /// Interaktionslogik für SqlEditor.xaml
    /// </summary>
    public partial class SqlEditor : Window
    {
        private readonly string _databaseName;
        public SqlEditor(string databaseName)
        {
            InitializeComponent();
            _databaseName = databaseName;
        }
    }
}
