using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using BE.Common.Domains.Config.Enum;
using BE.Common.Domains.Config.Models;
using static System.Net.WebRequestMethods;
using BE.Common.Domains.Config.Interfaces;

namespace BE.Common.Domains.Config.Resolvers
{
    public class HttpEnvResolver: IEnvResolver
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpEnvResolver(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public EnvContext Resolve()
        {
            var req = _httpContextAccessor.HttpContext?.Request;

            var raw = (req?.Query["Env_Type"].ToString()
                    ?? req?.Headers["Env-Type"].ToString()
                    ?? _configuration["Env_Type"]
                    ?? "test").Trim().ToLowerInvariant();

            var target = raw == "Prd" ? DbTarget.VIMICO : DbTarget.Test;

            string Pick(string section) => _configuration[$"{section}:{(target == DbTarget.VIMICO ? "Prd" : "Test")}"] ?? "";

            return new EnvContext
            {
                Target = target,
                ConnectionString = Pick("ConnectionStrings"),
                HostURL = Pick("HostURL"),
                VEnvironment = Pick("vEnvironment"),
                VHost = Pick("vHost"),
                VTenant = Pick("vTennant"),
                VOrg = Pick("vOrg")
            };
        }
    }
}
