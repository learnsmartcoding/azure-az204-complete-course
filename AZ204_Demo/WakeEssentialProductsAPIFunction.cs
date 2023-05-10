using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AZ204_Functions_Demo
{
    public class WakeEssentialProductsAPIFunction
    {
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("WakeEssentialProductsAPIFunction")]
        public async Task Run([TimerTrigger("0 */20 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var url = "https://essentialproducts-api.azurewebsites.net/api/Category/All";
            var response = await httpClient.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();

            log.LogInformation($"API response: {responseBody}");
        }
    }
}
