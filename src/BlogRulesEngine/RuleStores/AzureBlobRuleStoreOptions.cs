namespace BlogRulesEngine.RuleStores
{
    public class AzureBlobRuleStoreOptions
    {
        public string? AccountName { get; set; }
        public string? ContainerName { get; set; } = "workflows";
        public string? ConnectionString { get; set; } = "UseDevelopmentStorage=true";
    }
}
