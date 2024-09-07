using Microsoft.Extensions.Logging;
using RulesEngine.Actions;
using RulesEngine.Models;

namespace BlogRulesEngine.Actions
{
    public class SampleAction : ActionBase
    {
        private const string ContextKeyName = "Name";
        private readonly ILogger<SampleAction> _logger;

        public SampleAction(ILogger<SampleAction> logger)
        {
            _logger = logger;
        }

        public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            _logger.LogInformation("Executing sample action");
            var name = context.GetContext<string>(ContextKeyName);
            return new ValueTask<object>($"Hello {name}");
        }
    }
}
