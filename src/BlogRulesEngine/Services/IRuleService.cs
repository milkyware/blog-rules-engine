namespace BlogRulesEngine.Services
{
    public interface IRuleService
    {
        Task<object?> ExecuteRulesAsync(string workflowName, object input);
    }
}
