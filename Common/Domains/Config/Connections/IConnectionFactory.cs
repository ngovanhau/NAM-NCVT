using BE.Common.Domains.Config.Models;
using System.Data;

namespace BE.Common.Domains.Config.Connections
{
    public interface IConnectionFactory
    {
        IDbConnection Open(EnvContext env);
    }
}
