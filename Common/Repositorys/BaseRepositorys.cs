using BE.Common.Domains.Config.Interfaces;
using BE.Common.Domains.Config.Connections;
using BE.Common.Domains.Config.Models;
using System.Data;

namespace BE.Common.Domains.Config
{
    public abstract class BaseRepository
    {
        private readonly IEnvResolver _resolver;
        private readonly IConnectionFactory _factory;

        protected BaseRepository(IEnvResolver resolver, IConnectionFactory factory)
        {
            _resolver = resolver;
            _factory = factory;
        }

        protected EnvContext CurrentEnv() => _resolver.Resolve();

        protected IDbConnection GetConnection()
        {
            var env = _resolver.Resolve();
            return _factory.Open(env);
        }
    }
}