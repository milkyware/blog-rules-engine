using RulesEngine.Models;

namespace BlogRulesEngine.RuleStores
{
    public interface IRuleStore
    {
        Task<IEnumerable<Workflow>> GetWorkflowsAsync();
    }
}
