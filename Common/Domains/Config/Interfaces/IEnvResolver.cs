using BE.Common.Domains.Config.Models;

namespace BE.Common.Domains.Config.Interfaces
{
    public interface IEnvResolver
    {
        EnvContext Resolve();
    }
}
