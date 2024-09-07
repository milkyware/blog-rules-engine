using System.ComponentModel.DataAnnotations;

namespace BlogRulesEngine.RuleStores
{
    public class LocalRuleStoreOptions
    {
        [Required]
        public string? Path { get; set; }

        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromDays(1);
    }
}
