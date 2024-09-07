using System.Text.Json;
using System.Text.Json.Nodes;
using BlogRulesEngine.Actions;
using BlogRulesEngine.Builder;
using BlogRulesEngine.Extensions;
using BlogRulesEngine.RuleStores;
using BlogRulesEngine.Services;
using Microsoft.AspNetCore.Mvc;
using RulesEngine.ExpressionBuilders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRulesEngine(configure =>
{
    configure.Services.AddTransient<RuleExpressionParser>();
    configure.AddCustomAction<UpdateOperationAction>("UpdateOperation")
        .AddCustomAction<SampleAction>()
        .AddLocalRuleStore(builder.Configuration.GetSection("RULESENGINE:LOCALRULESTORE"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/execute", async ([FromServices] IRuleService ruleService, [FromQuery] string workflowName, [FromBody] JsonElement body) =>
{
    app.Logger.LogInformation("Executing rules");

    app.Logger.LogDebug("Parsing body to JsonNode");
    var jsonNode = JsonNode.Parse(body.GetRawText());
    if (jsonNode is null)
        return Results.BadRequest();

    var output = await ruleService.ExecuteRulesAsync(workflowName, jsonNode);
    return Results.Ok(output);
})
    .WithOpenApi();

app.MapPut("/refreshcache", async ([FromServices] IEnumerable<IRuleStoreCacheable> ruleStores) =>
{
    app.Logger.LogInformation("Refreshing cacheable rule stores");
    List<Task> tasks = [];
    foreach (var rs in ruleStores)
    {
        tasks.Add(rs.RefreshCacheAsync());
    }
    await Task.WhenAll(tasks);

    return Results.NoContent();
});

await app.RunAsync();
