using RulesEngine.Extensions;

namespace BlogRulesEngineTests
{
    public class WorkflowTests
    {
        [Theory]
        [InlineData("Hello there", "General Kenobi")]
        [InlineData("Eyup", "These aren't the droids you're looking for")]
        public async Task SampleWorkflowTest(string input, string expected)
        {
            // Arrange
            var workflowJson = """
                {
                  "$schema": "https://raw.githubusercontent.com/microsoft/RulesEngine/main/schema/workflow-schema.json",
                  "WorkflowName": "SampleWorkflow",
                  "Rules": [
                    {
                      "RuleName": "GeneralGrevious",
                      "RuleExpressionType": "LambdaExpression",
                      "Expression": "input1 == \"Hello there\"",
                      "SuccessEvent": "General Kenobi"
                    },
                    {
                      "RuleName": "Droids",
                      "RuleExpressionType": "LambdaExpression",
                      "Expression": "input1 != \"Hello there\"",
                      "SuccessEvent": "These aren't the droids you're looking for"
                    }
                  ]
                }
                """;
            var rulesEngine = new RulesEngine.RulesEngine([workflowJson]);

            // Act
            var results = await rulesEngine.ExecuteAllRulesAsync("SampleWorkflow", input);

            // Arrange
            var output = string.Empty;
            results.OnSuccess(eventName => output = eventName);
            output.Should()
                .Be(expected);
        }
    }
}
