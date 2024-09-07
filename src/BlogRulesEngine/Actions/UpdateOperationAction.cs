using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using RulesEngine.Actions;
using RulesEngine.Models;

namespace BlogRulesEngine.Actions
{
    public class UpdateOperationAction(ILogger<UpdateOperationAction> logger) : ActionBase
    {
        private const string ContextKeyOperation = "Operation";
        private readonly ILogger<UpdateOperationAction> _logger = logger;

        public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            _logger.LogInformation("Executing {action}", nameof(UpdateOperationAction));
            var value = context.GetContext<string>(ContextKeyOperation);
            var input = (JsonObject)ruleParameters.First().Value;
            input[ContextKeyOperation] = value;

            return new ValueTask<object>(input);
        }
    }
}
