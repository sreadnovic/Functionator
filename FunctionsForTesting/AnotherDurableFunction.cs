using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace FunctionsForTesting;

public static class AnotherDurableFunction
{
    [FunctionName("AnotherDurableFunction")]
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
}