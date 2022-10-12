using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionsForTesting;

public static class GreetingsDurableFunction
{
    [FunctionName("GreetingsDurableFunction")]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        await context.CallActivityAsync<string>("ActivityFunction_Hello", "Tokyo");
        await context.CallActivityAsync<string>("ActivityFunction_Hello", "Seattle");
        await context.CallActivityAsync<string>("ActivityFunction_Hello", "London");

        await context.CallSubOrchestratorAsync("GoodbyeDurableFunction", null);
    }

    [FunctionName("ActivityFunction_Hello")]
    public static string SayHello([ActivityTrigger] string name, ILogger log)
    {
        log.LogInformation($"Saying hello to {name}.");
        return $"Hello {name}!";
    }

    [FunctionName("GreetingsTriggerFunction_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        var instanceId = await starter.StartNewAsync("GreetingsDurableFunction");

        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}