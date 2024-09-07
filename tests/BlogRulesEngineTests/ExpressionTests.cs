using System.Text.Json.Nodes;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;

namespace BlogRulesEngineTests
{
    public class ExpressionTests
    {
        [Fact]
        public void SimpleExpressionTest()
        {
            // Act
            var actual = new RuleExpressionParser()
                .Evaluate<int>("3 + 5", []);

            //Assert
            actual.Should()
                .Be(8);
        }

        [Fact]
        public void SimpleParameterExpressionTest()
        {
            // Act
            var actual = new RuleExpressionParser()
                .Evaluate<string>("\"Hello \" + input1",
                [
                    new RuleParameter("input1", "World")
                ]);

            //Assert
            actual.Should()
                .Be("Hello World");
        }

        [Fact]
        public void JsonNodeExpressionTest()
        {
            // Arrange
            var json = """
                {
                    "FormType": "SampleFormType",
                    "Operation": "SampleOperation"
                }
                """;
            var jsonNode = JsonNode.Parse(json);
            var expression = "string(input1[\"FormType\"]) == \"SampleFormType\" && string(input1[\"Operation\"]) == \"SampleOperation\"";

            // Act
            var actual = new RuleExpressionParser()
                .Evaluate<bool>(expression,
                [
                    new RuleParameter("input1", jsonNode)
                ]);

            //Assert
            actual.Should()
                .Be(true);
        }
    }
}
