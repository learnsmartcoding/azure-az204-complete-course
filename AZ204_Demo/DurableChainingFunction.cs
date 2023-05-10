using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AZ204_Functions_Demo
{
    public static class DurableChainingFunction
    {
        [FunctionName("Chaining_HttpStart")]
        public static async Task<IActionResult> Chaining_HttpStart(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
    [DurableClient] IDurableOrchestrationClient starter,
    ILogger log)
        {
            string instanceId = await starter.StartNewAsync("Chaining", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Chaining")]
        public static async Task<string> Run(
    [OrchestrationTrigger] IDurableOrchestrationContext context,
    ILogger log)
        {
            try
            {
                // Call Activity Function 1
                string input1 = null; // Replace with your input
                string result1 = await context.CallActivityAsync<string>("GetInput", input1);

                // Call Activity Function 2 with the result of Activity Function 1
                string input2 = result1; // Replace with your input
                string result2 = await context.CallActivityAsync<string>("ProcessInput", input2);

                // Call Activity Function 3 with the result of Activity Function 2
                string input3 = result2; // Replace with your input
                string result3 = await context.CallActivityAsync<string>("TransformData", input3);

                // Call Activity Function 4 with the result of Activity Function 3
                string input4 = result3; // Replace with your input
                string finalResult = await context.CallActivityAsync<string>("PersistData", input4);

                return finalResult;
            }
            catch (Exception ex)
            {
                // Error handling or compensation goes here.
                log.LogError(ex, "An error occurred in the orchestration.");
                throw;
            }
        }

        [FunctionName("GetInput")]
        public static string GetInput([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"Executing GetInput with input: {input}");
            // Do some work
            return "Result of GetInput";
        }

        [FunctionName("ProcessInput")]
        public static string ProcessInput([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"Executing ProcessInput with input: {input}");
            // Do some work with input
            return "Result of ProcessInput";
        }

        [FunctionName("TransformData")]
        public static string TransformData([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"Executing TransformData with input: {input}");
            // Do some work with input
            return "Result of TransformData";
        }

        [FunctionName("PersistData")]
        public static string PersistData([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"Executing PersistData with input: {input}");
            // Do some work with input
            return "Result of PersistData";
        }


    }
}