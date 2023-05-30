using LSC.AZ204.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LSC.AZ204.WebAPI.Controllers
{
    [ApiController]
    [Route("api/configuration")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IKeyVaultSecretService keyVaultSecretService;

        public ConfigurationController(IConfiguration configuration, IKeyVaultSecretService keyVaultSecretService)
        {
            _configuration = configuration;
            this.keyVaultSecretService = keyVaultSecretService;
        }

        [HttpGet]
        public IActionResult GetAllConfigurationItems(string keyName)
        {
            var keyValue = _configuration[keyName];
            return Ok(new { keyName, keyValue });
        }

        [HttpGet("refresh")]
        public IActionResult PostRefreshAllConfigurationItems()
        {
            var configurationItems = new List<KeyValuePair<string, string>>();

            keyVaultSecretService.RefreshSecrets();

            foreach (var configSection in _configuration.GetChildren())
            {
                foreach (var configValue in configSection.GetChildren())
                {
                    configurationItems.Add(new KeyValuePair<string, string>(configValue.Key, configValue.Value));
                }
            }

            return Ok(configurationItems);
        }
    }

}
