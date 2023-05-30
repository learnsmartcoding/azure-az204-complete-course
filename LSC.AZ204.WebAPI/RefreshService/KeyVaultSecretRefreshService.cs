using LSC.AZ204.WebAPI.Services;

namespace LSC.AZ204.WebAPI.RefreshService
{
    public class KeyVaultSecretRefreshService : BackgroundService
    {
        private readonly IKeyVaultSecretService _keyVaultSecretService;
        private readonly TimeSpan _refreshInterval;

        public KeyVaultSecretRefreshService(IKeyVaultSecretService keyVaultSecretService)
        {
            _keyVaultSecretService = keyVaultSecretService;
            _refreshInterval = TimeSpan.FromSeconds(10);//only for demo purpose, ideally this frequently the data wont change.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Call the RefreshSecrets method
                _keyVaultSecretService.RefreshSecrets();

                // Wait for the specified interval before executing again
                await Task.Delay(_refreshInterval, stoppingToken);
            }
        }
    }

}
