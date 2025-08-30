using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services.Interfaces;
using CurrencyConverter.Api.Utilities;
namespace CurrencyConverter.Api.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyProviderFactory _providerFactory;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(ICurrencyProviderFactory providerFactory, ILogger<CurrencyService> logger)
        {
            _providerFactory = providerFactory;
            _logger = logger;
        }

        public async Task<ExchangeRateResponse?> GetLatestRatesAsync(string baseCurrency)
        {
            var provider = _providerFactory.GetProvider();
            return await provider.GetLatestRatesAsync(baseCurrency);
        }

        public async Task<ConversionResponse?> ConvertCurrencyAsync(ConversionRequest request)
        {
            var provider = _providerFactory.GetProvider();
            return await provider.ConvertCurrencyAsync(request);
        }

        public async Task<PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>>> GetHistoricalRatesAsync(HistoricalRatesRequest request)
        {
            var provider = _providerFactory.GetProvider();
            return await provider.GetHistoricalRatesAsync(request);
        }
    }
}