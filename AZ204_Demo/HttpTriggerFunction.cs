using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Net;

namespace AZ204_Functions_Demo
{
    public static class HttpTriggerFunction
    {
        private static readonly HttpClient _httpClient = new HttpClient();


        [FunctionName("HttpTrigger_SaveCategoryFunction")]
        public static async Task<IActionResult> SaveCategoryFunction(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Category")] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Category category = JsonConvert.DeserializeObject<Category>(requestBody);

            log.LogInformation($"Category name: {category.Name}");

            // Save the category to the specified URL
            using (var httpClient = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(category);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.PostAsync("https://essentialproducts-api.azurewebsites.net/api/Category", content);

                if (response.IsSuccessStatusCode)
                {
                    return new OkObjectResult("Category saved successfully.");
                }
                else
                {
                    log.LogError($"Error saving category. Status code: {response.StatusCode}");
                    return new StatusCodeResult(500);
                }
            }
        }

        [FunctionName("HttpTrigger_DeleteCategory")]
        public static async Task<IActionResult> DeleteCategory(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Category/{id}")] HttpRequest req,
        string id,
        ILogger log)
        {
            using (var httpClient = new HttpClient())
            {
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
                return new OkResult();
            }
        }

        [FunctionName("HttpTrigger_GetAllCategories")]
        public static async Task<IActionResult> GetAllCategories(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
        {
            try
            {
                var response = await _httpClient.GetAsync("https://essentialproducts-api.azurewebsites.net/api/Category/All");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<Category[]>(content);

                return new OkObjectResult(categories);
            }
            catch (HttpRequestException ex)
            {
                log.LogError(ex, "An error occurred while calling the API.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("HttpTrigger_LogAndEchoFunction")]
        public static async Task<IActionResult> LogAndEcho(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Log the request body
            log.LogInformation($"Received data: {requestBody}");

            // Return the same data back to the caller
            return new OkObjectResult(requestBody);
        }

        [FunctionName("MyTimerFunction")]
        public static void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        private class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
