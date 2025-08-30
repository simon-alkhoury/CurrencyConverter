using CurrencyConverter.Api.Models;

namespace CurrencyConverter.Api.Services.Interfaces
{
    public interface ICurrencyService
    {
        Task<ExchangeRateResponse?> GetLatestRatesAsync(string baseCurrency);
        Task<ConversionResponse?> ConvertCurrencyAsync(ConversionRequest request);
        Task<PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>>> GetHistoricalRatesAsync(HistoricalRatesRequest request);
    }
}
