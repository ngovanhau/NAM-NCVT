using System.Reflection;
using BE.Common.Application.CustomAttributes;

namespace BE.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                    !(a.IsDynamic) &&
                    a.FullName is string n &&
                    !n.StartsWith("System.", StringComparison.OrdinalIgnoreCase) &&
                    !n.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) &&
                    !n.StartsWith("Serilog", StringComparison.OrdinalIgnoreCase) &&
                    !n.StartsWith("Swashbuckle", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(c => c.WithAttribute<ScopedServiceAttribute>()) 
                    .AsSelf()
                    .WithScopedLifetime()
            );

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var repoTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("Repository")
                            && t.IsClass
                            && !t.IsAbstract
                            && t.Namespace != null
                            && t.Namespace.Contains("BE.Repositorys"));

            foreach (var repoType in repoTypes)
            {
                services.AddScoped(repoType);
            }

            return services;
        }
    }
}
