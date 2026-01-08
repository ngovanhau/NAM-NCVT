using Microsoft.Data.SqlClient;
using System.Data;
using BE.Common.Domains.Config.Models;

namespace BE.Common.Domains.Config.Connections
{
    public class SqlServerConnectionFactory : IConnectionFactory
    {
        public IDbConnection Open(EnvContext env)
        {
            var conn = new SqlConnection(env.ConnectionString);
            conn.Open();
            return conn;
        }
    }
}
