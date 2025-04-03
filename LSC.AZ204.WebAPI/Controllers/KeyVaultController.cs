using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using LSC.AZ204.WebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LSC.AZ204.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyVaultController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IKeyVaultSecretService keyVaultSecretService;

        public KeyVaultController(IConfiguration configuration, IKeyVaultSecretService keyVaultSecretService)
        {
            _configuration = configuration;
            this.keyVaultSecretService = keyVaultSecretService;
        }

        //[HttpGet("keyvault-ad")]
        [HttpGet]
        public IActionResult GetAllConfigurationItems()
        {
            var response = keyVaultSecretService.GetSecrets();
            return Ok(response);
        }

        [HttpGet("config")]
        public IActionResult GetConfigValue()
        {
            var configValue = _configuration["Secrets:MySecretKey"];
            return Ok(configValue);
        }

        [HttpGet("keyvault-identity")]
        public IActionResult GetSecretWithIdentity()
        {
            try
            {
                var keyVaultUri = _configuration["KeyVault:BaseUrl"];
                var secretKey = _configuration["Secrets:MySecretKey"];
                var managedIdentityCredential = new ManagedIdentityCredential();
                var secretClient = new SecretClient(new System.Uri(keyVaultUri), managedIdentityCredential);
                var secretValue = secretClient.GetSecret(secretKey).Value.Value;
                return Ok(secretValue);
            }
            catch (Exception ex)
            {
                // Log the exception for troubleshooting purposes
                // You can use ILogger or any other logging mechanism here

                // Return a 500 Internal Server Error response with the exception message
                return StatusCode(500, $"An error occurred while retrieving secret from Key Vault: {ex.Message}");
            }
        }

        [HttpGet("keyvault-system-identity")]
        public async Task<IActionResult> GetSecretWithSystemIdentity()
        {
            var keyVaultUri = _configuration["KeyVault:BaseUrl"];
            var secretKey = _configuration["Secrets:MySecretKey"];
            var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            //so how it worked from local ? //in azure it did not work, this is because that web app
            //did not have permission to access KV.
            var secretValue = (await secretClient.GetSecretAsync(secretKey)).Value.Value;
            return Ok(secretValue);
        }


    }
}
