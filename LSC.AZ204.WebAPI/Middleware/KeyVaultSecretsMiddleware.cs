using LSC.AZ204.WebAPI.Services;

namespace LSC.AZ204.WebAPI.Middleware
{
    public class KeyVaultSecretsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IKeyVaultSecretService _keyVaultSecretService;

        public KeyVaultSecretsMiddleware(RequestDelegate next, IKeyVaultSecretService keyVaultSecretService)
        {
            _next = next;
            _keyVaultSecretService = keyVaultSecretService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Call the RefreshSecrets method during application startup
            _keyVaultSecretService.RefreshSecrets();

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

}
