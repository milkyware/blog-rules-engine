using System.Text;
using BlogRulesEngine.RuleStores;
using Microsoft.Extensions.Logging;
using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace BlogRulesEngine.Services
{
    public class RuleService : IRuleService
    {
        private readonly ILogger<RuleService> _logger;
        private readonly IRulesEngine _rulesEngine;
        private readonly IEnumerable<IRuleStore> _ruleStores;

        private readonly Task _initialized;

        public RuleService(ILogger<RuleService> logger, IRulesEngine rulesEngine, IEnumerable<IRuleStore> ruleStores)
        {
            _logger = logger;
            _rulesEngine = rulesEngine;
            _ruleStores = ruleStores;

            _initialized = InitializeRulesEngineAsync();
        }

        public async Task<object?> ExecuteRulesAsync(string workflowName, object input)
        {
            _logger.LogInformation("Evaluating rules");

            await _initialized;

            var workflowExists = _rulesEngine.ContainsWorkflow(workflowName);
            _logger.LogTrace("workflowExists={workflowExists}", workflowExists);
            if (!workflowExists)
            {
                _logger.LogWarning("Workflows {workflowName} not found", workflowName);
                return null;
            }

            var results = await _rulesEngine.ExecuteAllRulesAsync(workflowName, input);

            _logger.LogTrace("Assessing rule exceptions");
            var exceptions = results.Select(r => r.ExceptionMessage)
                .Where(e => !string.IsNullOrWhiteSpace(e));
            if (exceptions.Count() > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Rule exceptions:");
                foreach (var e in exceptions)
                {
                    sb.AppendLine(e);
                }
                _logger.LogWarning(sb.ToString());
            }

            _logger.LogDebug("Assessing success");
            var success = results.Any(r => r.IsSuccess);
            _logger.LogTrace("success={success}", success);

            object output;
            if (success)
                output = results.First(r => r.IsSuccess).ActionResult.Output;
            else
                output = input;

            _logger.LogInformation("Returning evaluated output");
            return output;
        }

        #region Private Helpers

        private async Task InitializeRulesEngineAsync()
        {
            var tasks = new List<Task<IEnumerable<Workflow>>>();
            foreach (var rs in _ruleStores)
            {
                tasks.Add(rs.GetWorkflowsAsync());
            }
            var workflows = (await Task.WhenAll(tasks))
                .SelectMany(w => w)
                .ToArray();
            _rulesEngine.AddOrUpdateWorkflow(workflows);
        }

        #endregion Private Helpers
    }
}
