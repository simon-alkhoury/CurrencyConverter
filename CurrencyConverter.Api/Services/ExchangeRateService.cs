using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services.Interfaces;
using CurrencyConverter.Api.Utilities;
namespace CurrencyConverter.Api.Services
{
    public class ExchangeRateService
    {
        private readonly ICurrencyProviderFactory _providerFactory;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(ICurrencyProviderFactory providerFactory, ILogger<ExchangeRateService> logger)
        {
            _providerFactory = providerFactory;
            _logger = logger;
        }

        public async Task<ExchangeRateResponse?> GetLatestRatesAsync(string baseCurrency, ExchangeServiceProvider provider)
        {
            var service = _providerFactory.GetProvider(provider);
            return await service.GetLatestRatesAsync(baseCurrency);
        }

        public async Task<ConversionResponse?> ConvertCurrencyAsync(ConversionRequest request, ExchangeServiceProvider provider)
        {
            var service = _providerFactory.GetProvider(provider);
            return await service.ConvertCurrencyAsync(request);
        }

        public async Task<PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>>> GetHistoricalRatesAsync(HistoricalRatesRequest request, ExchangeServiceProvider provider)
        {
            var service = _providerFactory.GetProvider(provider);
            return await service.GetHistoricalRatesAsync(request);
        }
    }
}