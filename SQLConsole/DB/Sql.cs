using System;
using System.Collections.Generic;
using System.Text;

namespace SQLConsole.DB
{
    public class Sql
    {
        private string SqlText;

        public void Add(string sql)
        {
            SqlText += sql;
        }

        public void Clear()
        {
            SqlText = string.Empty;
        }

        public string GetSQL()
        {
            return SqlText;
        }

    }
}
