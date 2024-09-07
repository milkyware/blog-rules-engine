using Azure.Storage.Blobs;
using EasyCaching.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RulesEngine.Models;

namespace BlogRulesEngine.RuleStores
{
    public class AzureBlobRuleStore : IRuleStoreCacheable
    {
        private readonly ILogger<AzureBlobRuleStore> _logger;
        private readonly IEasyCachingProvider _cache;
        private readonly AzureBlobRuleStoreOptions _options;
        private readonly BlobServiceClient _serviceClient;

        private const string CacheWorkflowsKey = "WORKFLOWS";
        private static TimeSpan s_cacheExpiration = TimeSpan.FromDays(1);

        public AzureBlobRuleStore(ILogger<AzureBlobRuleStore> logger,
            IOptions<AzureBlobRuleStoreOptions> options,
            IEasyCachingProviderFactory cachingFactory,
            BlobServiceClient serviceClient)
        {
            _logger = logger;
            _cache = cachingFactory.GetCachingProvider(nameof(AzureBlobRuleStore));
            _options = options.Value;
            _serviceClient = serviceClient;
        }

        public async Task<IEnumerable<Workflow>> GetWorkflowsAsync()
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                { nameof(_options.ContainerName), _options.ContainerName! }
            }))
            {
                _logger.LogInformation("Loading rules from Azure blob storage");
                _logger.LogDebug("Getting workflow blob container");
                var containerClient = _serviceClient.GetBlobContainerClient(_options.ContainerName);

                _logger.LogDebug("Ensuring blob container exists");
                try
                {
                    await containerClient.CreateIfNotExistsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ensure blob container exists");
                    throw;
                }

                _logger.LogDebug("Getting workflows from blobs");
                var blobs = containerClient.GetBlobsAsync();
                var tasks = new List<Task<Workflow?>>();
                await foreach (var b in blobs)
                {
                    _logger.LogDebug("Getting blob {blobName}", b.Name);
                    var blobClient = containerClient.GetBlobClient(b.Name);

                    tasks.Add(GetWorkflowBlobAsync(blobClient));
                }
                var workflows = (await Task.WhenAll(tasks))
                    .Where(t => t is not null);

                _logger.LogDebug("Returning workflows");
                return workflows;
            }
        }

        public Task RefreshCacheAsync()
        {
            _logger.LogInformation("Refreshing cache");
            return _cache.FlushAsync();
        }

        private async Task<Workflow?> GetWorkflowBlobAsync(BlobClient blobClient)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
                {
                    { "blobName", blobClient.Name }
                }))
            {
                _logger.LogDebug("Getting workflow from {blobName}", blobClient.Name);

                try
                {
                    var workflow = await _cache.GetAsync<Workflow>(blobClient.Name, async () =>
                    {
                        _logger.LogDebug("Reading blob");
                        using var stream = await blobClient.OpenReadAsync();
                        using var sr = new StreamReader(stream);

                        var json = await sr.ReadToEndAsync();
                        _logger.LogTrace("json={json}", json);

                        var workflow = JsonConvert.DeserializeObject<Workflow>(json);
                        _logger.LogInformation("Caching workflow from {blobName}", blobClient.Name);

                        return workflow;
                    }, s_cacheExpiration);

                    _logger.LogDebug("Returning workflow for blob {blobName}", blobClient.Name);
                    return workflow.Value;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to retrieve workflow from Azure blob {blobName}", blobClient.Name);
                    return null;
                }
            }
        }
    }
}
