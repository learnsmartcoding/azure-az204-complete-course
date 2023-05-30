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

        [HttpGet]
        public IActionResult GetAllConfigurationItems()
        {
            var response = keyVaultSecretService.GetSecrets();
            return Ok(response);
        }
    }
}
