using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace LSC.AZ204.WebAPI.Services
{
    public static class KeyVaultConfiguration
    {
        public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder configBuilder, IConfiguration configuration)
        {
            var keyVaultEndpoint = configuration["KeyVault:BaseUrl"];

            if (!string.IsNullOrEmpty(keyVaultEndpoint))
            {
                var clientId = configuration["AzureAd:ClientId"];
                var clientSecret = configuration["AzureAd:ClientSecret"];
                var tenantId = configuration["AzureAd:TenantId"];
                var secretClient = new SecretClient(new Uri(keyVaultEndpoint), new ClientSecretCredential(tenantId, clientId, clientSecret));
                Dictionary<string, string> _secrets = new Dictionary<string, string>();

                // Read all secrets from the key vault and store them in memory
                foreach (var secret in secretClient.GetPropertiesOfSecrets())
                {
                    var secretName = secret.Name;
                    var secretValue = secretClient.GetSecret(secretName).Value.Value;

                    _secrets[secretName] = secretValue;
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string> { [secretName] = secretValue });
                    //configBuilder.AddInMemoryCollection(new Dictionary<string, string> { [secretName.Replace('-', ':')] = secretValue });
                }
            }

            return configBuilder;
        }
    }
}
