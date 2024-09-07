using System.ComponentModel.DataAnnotations;
using Azure.Identity;
using Azure.Storage.Blobs;
using BlogRulesEngine.RuleStores;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BlogRulesEngine.Builder
{
    public static class RulesEngineBuilderExtensions
    {
        public static IRulesEngineBuilder AddAzureBlobRuleStore(this IRulesEngineBuilder builder, IConfigurationSection configuration)
        {
            builder.Services.AddOptions<AzureBlobRuleStoreOptions>()
                .Bind(configuration)
                .ValidateOnStart();

            builder.Services.AddEasyCaching(setup =>
            {
                setup.UseInMemory(nameof(AzureBlobRuleStore));
            });
            builder.Services.AddAzureClients(configure =>
            {
                configure.UseCredential(new DefaultAzureCredential());
                configure.AddClient<BlobServiceClient, BlobClientOptions>((clientOptions, credential, sp) =>
                {
                    var options = sp.GetRequiredService<IOptions<AzureBlobRuleStoreOptions>>().Value;

                    BlobServiceClient serviceClient;
                    if (!string.IsNullOrWhiteSpace(options.AccountName))
                    {
                        serviceClient = new BlobServiceClient(new Uri($"https://{options.AccountName}.blob.core.windows.net"), credential, clientOptions);
                        return serviceClient;
                    }

                    serviceClient = new BlobServiceClient(options.ConnectionString, clientOptions);
                    return serviceClient;
                });
            });
            builder.AddRuleStoreCacheable<AzureBlobRuleStore>();

            return builder;
        }

        public static IRulesEngineBuilder AddLocalRuleStore(this IRulesEngineBuilder builder, IConfigurationSection configuration)
        {
            builder.Services.AddOptions<LocalRuleStoreOptions>()
                .Bind(configuration)
                .ValidateDataAnnotations()
                .Validate(options =>
                {
                    var pathExists = Path.Exists(options.Path);
                    if (!pathExists)
                        throw new ValidationException($"Path {options.Path} does not exist");

                    return true;
                })
                .ValidateOnStart();

            builder.Services.AddEasyCaching(setup =>
            {
                setup.UseInMemory(nameof(LocalRuleStore));
            });
            builder.AddRuleStoreCacheable<LocalRuleStore>();

            return builder;
        }
    }
}
