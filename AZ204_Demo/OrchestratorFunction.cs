using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AZ204_Functions_Demo
{
    public static class OrchestratorFunction
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        [FunctionName("OrchestratorFunction")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            // Call GetAllCategories function
            var categories = await context.CallActivityAsync<List<Category>>("GetAllCategoriesActivity", null);

            // Call LogAndEchoFunction passing the data from GetAllCategories
            await context.CallActivityAsync("LogAndEchoFunction", categories);

            // Loop through each category and call SaveCategoryFunction
            var tasks = new List<Task>();
            var maxId = categories.Max(x => x.Id) + 1;

            for (int i = 1; i <= 2; i++)
            {
                var category = new Category
                {
                    Id = maxId,
                    Name = $"random-category-name-{maxId}",
                    IsActive = true
                };

                tasks.Add(context.CallActivityAsync("SaveCategoryFunction", category));
                ++maxId;
            }
            await Task.WhenAll(tasks);

            // Call GetAllCategories function
            categories = await context.CallActivityAsync<List<Category>>("GetAllCategoriesActivity", null);

            // Call LogAndEchoFunction passing the data from GetAllCategories
            await context.CallActivityAsync("LogAndEchoFunction", categories);

            tasks = new List<Task>();
            maxId = categories.Max(x => x.Id);

            //delete the added ones
            for (int i = maxId-1; i <= maxId; i++)
            {
                tasks.Add(context.CallActivityAsync("DeleteCategory", i));

            }

            // Wait for all SaveCategoryFunction calls to finish
            await Task.WhenAll(tasks);

            categories = await context.CallActivityAsync<List<Category>>("GetAllCategoriesActivity", null);

            // Call LogAndEchoFunction passing the data from GetAllCategories
            await context.CallActivityAsync("LogAndEchoFunction", categories);
        }

        [FunctionName("LogAndEchoFunction")]
        public static IActionResult RunLogAndEchoFunction(
         [ActivityTrigger] IDurableActivityContext context,
         ILogger log)
        {
            log.LogInformation("C# Activity function processed a request.");

            var data = context.GetInput<List<Category>>();
            // Log the data
            var dataJson = JsonConvert.SerializeObject(data);
            log.LogInformation($"Received data: {dataJson}");

            // Return the same data back to the caller
            return new OkObjectResult(data);
        }       

        [FunctionName("OrchestratorFunction_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
    [DurableClient] IDurableOrchestrationClient starter,
    ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("OrchestratorFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            // Call the OrchestratorFunction, passing in the instance ID
            var result = await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, TimeSpan.FromSeconds(10));


            return result;
        }

        [FunctionName("SaveCategoryFunction")]
        public static async Task SaveCategoryFunction(
    [ActivityTrigger] IDurableActivityContext context,
    ILogger log)
        {
            log.LogInformation("SaveCategoryFunction processed a request.");

            Category category = context.GetInput<Category>();

            log.LogInformation($"Category name: {category.Name}");

            // Save the category to the specified URL
            using (var httpClient = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(category);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.PostAsync("https://essentialproducts-api.azurewebsites.net/api/Category", content);

                if (!response.IsSuccessStatusCode)
                {
                    log.LogError($"Error saving category. Status code: {response.StatusCode}");
                    throw new Exception("Error saving category.");
                }
            }
        }

        [FunctionName("DeleteCategory")]
        public static async Task<IActionResult> DeleteCategory(
    [ActivityTrigger] IDurableActivityContext context,
    ILogger log)
        {
            using (var httpClient = new HttpClient())
            {
                var id = context.GetInput<int>();
                var apiUrl = $"https://essentialproducts-api.azurewebsites.net/api/Category/{id}";
                var response = await httpClient.DeleteAsync(apiUrl);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new NotFoundResult();
                }
                else if (!response.IsSuccessStatusCode)
                {
                    return new StatusCodeResult((int)response.StatusCode);
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    log.LogInformation($"Deleted category id {id}");
                }
                return new OkResult();
            }
        }

        [FunctionName("GetAllCategoriesActivity")]
        public static async Task<List<Category>> GetAllCategoriesActivity(
     [ActivityTrigger] object input,
     ILogger log)
        {
            try
            {
                var response = await _httpClient.GetAsync("https://essentialproducts-api.azurewebsites.net/api/Category/All");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<Category[]>(content);

                return categories.ToList();
            }
            catch (HttpRequestException ex)
            {
                log.LogError(ex, "An error occurred while calling the API.");
                return new List<Category>();
            }
        }


        public class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
        }
    }
}