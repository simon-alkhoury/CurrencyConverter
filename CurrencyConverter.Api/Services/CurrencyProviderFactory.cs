using CurrencyConverter.Api.Services.Interfaces;

namespace CurrencyConverter.Api.Services
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IExchangeRateService GetProvider(string providerName = "frankfurter")
        {
            return providerName.ToLower() switch
            {
                "frankfurter" => _serviceProvider.GetRequiredService<IExchangeRateService>(),
                _ => _serviceProvider.GetRequiredService<IExchangeRateService>()
            };
        }
    }
}
