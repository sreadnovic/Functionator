using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionsForTesting;

public static class GoodbyeDurableFunction
{
    [FunctionName("GoodbyeDurableFunction")]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        await context.CallActivityAsync<string>("ActivityFunction_GoodBye", "Tokyo");
        await context.CallActivityAsync<string>("ActivityFunction_GoodBye", "Seattle");
        await context.CallActivityAsync<string>("ActivityFunction_GoodBye", "London");
    }

    [FunctionName("ActivityFunction_GoodBye")]
    public static string SayHello([ActivityTrigger] string name, ILogger log)
    {
        log.LogInformation($"Saying good bye to {name}.");
        return $"Good bye {name}!";
    }

    [FunctionName("GoodbyeTriggerFunction_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        var instanceId = await starter.StartNewAsync("GoodbyeDurableFunction");

        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}