using BlogRulesEngine.RuleStores;
using BlogRulesEngine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RulesEngine.Actions;
using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace BlogRulesEngine.Builder
{
    public class RulesEngineBuilder : IRulesEngineBuilder
    {
        public IServiceCollection Services { get; }

        internal RulesEngineBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IRulesEngineBuilder AddCustomAction<T>()
            where T : ActionBase
        {
            return AddCustomAction<T>(typeof(T).Name);
        }

        public IRulesEngineBuilder AddCustomAction<T>(string name)
            where T : ActionBase
        {
            Services.AddSingleton<Tuple<string, Func<ActionBase>>>(sp => new(name, () => sp.GetRequiredService<T>()));
            Services.TryAddTransient<T>();
            return this;
        }

        public IRulesEngineBuilder AddRuleStoreCacheable<T>()
            where T : class, IRuleStoreCacheable
        {
            Services.TryAddTransient<T>();
            Services.AddTransient<IRuleStoreCacheable>(sp => sp.GetRequiredService<T>());
            return AddRuleStore<T>();
        }

        public IRulesEngineBuilder AddRuleStore<T>()
            where T : class, IRuleStore
        {
            Services.TryAddTransient<T>();
            Services.AddTransient<IRuleStore>(sp => sp.GetRequiredService<T>());
            return this;
        }

        internal void Build()
        {
            Services.AddTransient<IRulesEngine>(sp =>
            {
                var customActions = sp.GetServices<Tuple<string, Func<ActionBase>>>();

                var settings = new ReSettings()
                {
                    CustomActions = customActions.ToDictionary(ca => ca.Item1, ca => ca.Item2)
                };

                var rulesEngine = new RulesEngine.RulesEngine(settings);
                return rulesEngine;
            });
            Services.AddTransient<IRuleService, RuleService>();
        }
    }
}
