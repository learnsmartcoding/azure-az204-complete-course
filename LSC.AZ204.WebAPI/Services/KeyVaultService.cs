using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using LSC.AZ204.WebAPI.Models;

namespace LSC.AZ204.WebAPI.Services
{

    public interface IKeyVaultSecretRefreshService
    {
        void RefreshSecrets();
    }
   
    public interface IKeyVaultSecretService
    {
        void RefreshSecrets();
        List<VaultSecret> GetSecrets();
    }

    public class KeyVaultSecretService : IKeyVaultSecretService, IKeyVaultSecretRefreshService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeyVaultSecretService> logger;
        

        public KeyVaultSecretService(IConfiguration configuration, ILogger<KeyVaultSecretService> logger)
        {
            _configuration = configuration;
            this.logger = logger;
           
        }

        public void RefreshSecrets()
        {
            try
            {
                var keyVaultEndpoint = _configuration["KeyVault:BaseUrl"];
                var clientId = _configuration["AzureAd:ClientId"];
                var clientSecret = _configuration["AzureAd:ClientSecret"];
                var tenantId = _configuration["AzureAd:TenantId"];

                var secretClient = new SecretClient(new Uri(keyVaultEndpoint), new ClientSecretCredential(tenantId, clientId, clientSecret));

                if (!string.IsNullOrEmpty(keyVaultEndpoint))
                {                   

                    // Get the root configuration section
                    var rootConfiguration = (IConfigurationRoot)_configuration;

                    // Create a dictionary to hold the secrets
                    var secrets = new Dictionary<string, string>();

                    // Get the keys of all the existing secrets
                    var secretProperties = secretClient.GetPropertiesOfSecrets();
                    foreach (var secretProperty in secretProperties)
                    {
                        var secretName = secretProperty.Name;
                        var secretValue = secretClient.GetSecret(secretName).Value.Value;

                        // Add or update the secret in the configuration
                        secrets[secretName] = secretValue;

                        // Update the secret in the configuration if it already exists
                        var existingSecret = _configuration.GetSection(secretName);
                        if (existingSecret.Exists())
                        {
                            // Update the secret value
                            existingSecret.Value = secretValue;
                            logger.LogInformation($"Updated the secret '{secretName}' in the configuration.");
                        }
                        // Add the secret to the configuration if it's new
                        else
                        {
                            // Add a new configuration section with the secret
                            ((IConfigurationBuilder)rootConfiguration).AddInMemoryCollection(new Dictionary<string, string> { [secretName] = secretValue });
                            logger.LogInformation($"Added the new secret '{secretName}' to the configuration.");
                        }
                    }
                }
                else
                {
                    logger.LogWarning("The KeyVault:BaseUrl configuration setting is missing or empty. No secrets were refreshed.");
                }

                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while refreshing the secrets from Key Vault.");               
            }
        }

        public List<VaultSecret> GetSecrets()
        {
            var keyVaultSecrets = new List<VaultSecret>();
            try
            {
                
                var keyVaultEndpoint = _configuration["KeyVault:BaseUrl"];
                var clientId = _configuration["AzureAd:ClientId"];
                var clientSecret = _configuration["AzureAd:ClientSecret"];
                var tenantId = _configuration["AzureAd:TenantId"];

                var secretClient = new SecretClient(new Uri(keyVaultEndpoint), new ClientSecretCredential(tenantId, clientId, clientSecret));

                if (!string.IsNullOrEmpty(keyVaultEndpoint))
                {

                    // Get the root configuration section
                    var rootConfiguration = (IConfigurationRoot)_configuration;

                    // Create a dictionary to hold the secrets
                    var secrets = new Dictionary<string, string>();

                    // Get the keys of all the existing secrets
                    var secretProperties = secretClient.GetPropertiesOfSecrets();
                    foreach (var secretProperty in secretProperties)
                    {
                        var secretName = secretProperty.Name;
                        var secretValue = secretClient.GetSecret(secretName).Value.Value;

                        keyVaultSecrets.Add(new VaultSecret() { Name = secretName, Value = secretValue });
                        
                    }
                }
                else
                {
                    logger.LogWarning("The KeyVault:BaseUrl configuration setting is missing or empty. No secrets were refreshed.");
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while refreshing the secrets from Key Vault.");
            }
            return keyVaultSecrets;
        }
    }

}