using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KudrDiplom
{
    internal class DataBase
    {
        SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-33KIH7T\SQLEXPRESS;Initial Catalog=PeresvetDB-KUDRYASHOV;Persist Security Info=True;User ID=sisadmin;Password=sasasa123");
        public void openConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
        }
        public void closeConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }
        public SqlConnection getConnection()
        {
            return sqlConnection;
        }
        public async Task openConnectionAsync()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }
        }
    }
}
