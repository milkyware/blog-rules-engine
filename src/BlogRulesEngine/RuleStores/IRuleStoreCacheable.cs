namespace BlogRulesEngine.RuleStores
{
    public interface IRuleStoreCacheable : IRuleStore
    {
        public Task RefreshCacheAsync();
    }
}
