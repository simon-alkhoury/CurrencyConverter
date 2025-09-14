using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services.Interfaces;

namespace CurrencyConverter.Api.Services
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<ExchangeServiceProvider, IExchangeRateService> _providers;

        //public CurrencyProviderFactory(IServiceProvider serviceProvider)
        //{
        //    _serviceProvider = serviceProvider;

        //    _exchangeServiceProvider = new Dictionary<ExchangeServiceProvider, Type>
        //{
        //    { ExchangeServiceProvider.FranFurter, typeof(FrankfurterService) },
        //    { ExchangeServiceProvider.Fixer, typeof(FixerService) },

        //};
        //}
        public CurrencyProviderFactory(IEnumerable<IExchangeRateService> providers)
        {
            _providers = providers.ToDictionary(p => p.ProviderType, p => p);
        }

        public IExchangeRateService GetProvider(ExchangeServiceProvider exchangeServiceProvider)
        {
            if (_providers.TryGetValue(exchangeServiceProvider, out var provider))
                return provider;

            return _providers[ExchangeServiceProvider.FranFurter]; // fallback
        }

        //public IExchangeRateService GetProvider(ExchangeServiceProvider exchangeServiceProvider)
        //{
        //    // 4. LOOKUP: Find the Type for this customer type
        //    if (_exchangeServiceProvider.TryGetValue(exchangeServiceProvider, out var provider))
        //    {
        //        // 5. CREATE: Ask DI container to create instance with all dependencies
        //        return (IExchangeRateService)_serviceProvider.GetService(provider);
        //    }

        //    // 6. FALLBACK: Default to regular discount if type not found
        //    return (IExchangeRateService)_serviceProvider.GetService(typeof(FrankfurterService));
    }
    
}
