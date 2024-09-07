using BlogRulesEngine.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlogRulesEngine.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRulesEngine(this IServiceCollection services, Action<IRulesEngineBuilder>? configure = null)
        {
            var builder = new RulesEngineBuilder(services);

            if (configure is not null)
                configure.Invoke(builder);

            builder.Build();
            return services;
        }
    }
}