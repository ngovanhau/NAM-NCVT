using BE.Common.Domains.Config.Enum;

namespace BE.Common.Domains.Config.Models
{
    public sealed class EnvContext
    {
        public DbTarget Target { get; init; }
        public string ConnectionString { get; init; } = "";
        public string HostURL { get; init; } = "";
        public string VEnvironment { get; init; } = "";
        public string VHost { get; init; } = "";
        public string VTenant { get; init; } = "";
        public string VOrg { get; init; } = "";
    }
}
