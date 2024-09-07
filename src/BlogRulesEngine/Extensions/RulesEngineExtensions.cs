using Newtonsoft.Json;
using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace BlogRulesEngine
{
    public static class RulesEngineExtensions
    {
        public static void AddWorkflow(this IRulesEngine rulesEngine, params string[] jsonConfig)
        {
            var workflows = jsonConfig.Select(JsonConvert.DeserializeObject<Workflow>).ToArray();
            rulesEngine.AddWorkflow(workflows);
        }

        public static void AddOrUpdateWorkflow(this IRulesEngine rulesEngine, params string[] jsonConfig)
        {
            var workflows = jsonConfig.Select(JsonConvert.DeserializeObject<Workflow>).ToArray();
            rulesEngine.AddOrUpdateWorkflow(workflows);
        }
    }
}