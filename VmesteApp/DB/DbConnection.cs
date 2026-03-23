using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VmesteApp.DB
{
    public class DbConnection
    {
        private static string connString = "Host=localhost;Username=postgres;Password=0702;Database=postgres;SearchPath=VmesteDB";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connString);
        }
    }
}
