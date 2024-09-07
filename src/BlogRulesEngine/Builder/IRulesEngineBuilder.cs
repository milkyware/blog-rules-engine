using BlogRulesEngine.RuleStores;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine.Actions;

namespace BlogRulesEngine.Builder
{
    public interface IRulesEngineBuilder
    {
        IServiceCollection Services { get; }

        IRulesEngineBuilder AddCustomAction<T>(string name) where T : ActionBase;

        IRulesEngineBuilder AddCustomAction<T>() where T : ActionBase;

        IRulesEngineBuilder AddRuleStore<T>() where T : class, IRuleStore;

        IRulesEngineBuilder AddRuleStoreCacheable<T>() where T : class, IRuleStoreCacheable;
    }
}
