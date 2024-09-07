using EasyCaching.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RulesEngine.Models;

namespace BlogRulesEngine.RuleStores
{
    public class LocalRuleStore(ILogger<LocalRuleStore> logger,
        IOptions<LocalRuleStoreOptions> options,
        IEasyCachingProviderFactory cachingFactory) : IRuleStoreCacheable
    {
        private readonly ILogger<LocalRuleStore> _logger = logger;
        private readonly IEasyCachingProvider _cache = cachingFactory.GetCachingProvider(nameof(LocalRuleStore));
        private readonly LocalRuleStoreOptions _options = options.Value;

        public async Task<IEnumerable<Workflow>> GetWorkflowsAsync()
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {  nameof(_options.Path), _options.Path! }
            }))
            {
                _logger.LogInformation("Getting local workflows from {Path}", _options.Path);

                var workflowPaths = GetLocalWorkflowPaths();

                List<Task<Workflow?>> tasks = [];
                foreach (var wp in workflowPaths)
                {
                    tasks.Add(GetWorkflowAsync(wp));
                }

                _logger.LogDebug("Waiting for all workflows to be retrieved");
                var workflows = (await Task.WhenAll(tasks))?
                    .Where(w => w is not null);

                _logger.LogInformation("Returning workflows");
                return workflows;
            }
        }

        public Task RefreshCacheAsync()
        {
            _logger.LogInformation("Refreshing cache");
            return _cache.FlushAsync();
        }

        #region Private Helpers

        private async Task<Workflow?> GetWorkflowAsync(string workflowPath)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                { "WorkflowPath", workflowPath }
            }))
            {
                _logger.LogDebug("Getting workflow for {workflowPath}", workflowPath);

                try
                {
                    var workflow = await _cache.GetAsync(workflowPath, async () =>
                    {
                        var json = await File.ReadAllTextAsync(workflowPath);
                        var workflow = JsonConvert.DeserializeObject<Workflow>(json);

                        _logger.LogInformation("Caching workflow");
                        return workflow!;
                    }, _options.CacheExpiration);

                    _logger.LogDebug("Returning workflow for {workflowPath}", workflowPath);
                    return workflow.Value;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to retrieve workflow from {path}", workflowPath);
                    return null;
                }
            }
        }

        private IEnumerable<string> GetLocalWorkflowPaths()
        {
            _logger.LogDebug("Getting local workflows paths");

            _logger.LogDebug("Checking if configured path is a directory or file");
            var isDir = Directory.Exists(_options.Path);
            _logger.LogTrace("isDir={isDir}", isDir);

            string[] paths;
            if (isDir)
            {
                _logger.LogDebug("Configured path is a directory, getting workflows");
                paths = Directory.GetFiles(_options.Path!);
            }
            else
            {
                _logger.LogDebug("Configured path is a file, returning file path");
                paths =
                [
                    _options.Path!
                ];
            }
            _logger.LogTrace("paths={paths}", paths);

            _logger.LogDebug("Returning workflows paths");
            return paths;
        }

        #endregion Private Helpers
    }
}
