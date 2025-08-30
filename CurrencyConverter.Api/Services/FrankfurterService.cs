using System.Text.Json;
using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CurrencyConverter.Api.Utilities;
using Microsoft.AspNetCore.Mvc;
namespace CurrencyConverter.Api.Services
{
    public class FrankfurterService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly CurrencySettings _settings;
        private readonly ILogger<FrankfurterService> _logger;

        private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
            { "TRY", "PLN", "THB", "MXN" };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FrankfurterService(HttpClient httpClient, IMemoryCache cache, IOptions<CurrencySettings> settings, ILogger<FrankfurterService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _settings = settings.Value;
            _logger = logger;

            _logger.LogInformation("FrankfurterService initialized with BaseAddress: {BaseAddress}",
                _httpClient.BaseAddress?.ToString() ?? "NULL");
        }

        public async Task<ExchangeRateResponse?> GetLatestRatesAsync(string baseCurrency)
        {
            string cacheKey = $"latest-{baseCurrency.ToUpper()}";
            if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse? cached))
            {
                _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                return cached;
            }

            try
            {
                _logger.LogInformation("Fetching latest rates for {BaseCurrency}", baseCurrency);
                var response = await _httpClient.GetAsync($"latest?from={baseCurrency}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ExchangeRateResponse>(json, JsonOptions);

                if (result != null)
                {
                    // Filter out excluded currencies
                    var filteredRates = result.Rates
                        .Where(r => !Excluded.Contains(r.Key))
                        .ToDictionary(r => r.Key, r => r.Value);

                    result.Rates = filteredRates;

                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
                    _logger.LogInformation("Cached result for {CacheKey}", cacheKey);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest rates for {BaseCurrency}", baseCurrency);
                throw;
            }
        }

        public async Task<ConversionResponse?> ConvertCurrencyAsync(ConversionRequest request)
        {
            if (Excluded.Contains(request.From) || Excluded.Contains(request.To))
                throw new ArgumentException("Conversion not supported for excluded currencies.");

            try
            {
                _logger.LogInformation("Converting {Amount} from {From} to {To}", request.Amount, request.From, request.To);

                var response = await _httpClient.GetAsync($"latest?amount={request.Amount}&from={request.From}&to={request.To}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ExchangeRateResponse>(json, JsonOptions);

                if (result?.Rates != null && result.Rates.TryGetValue(request.To, out var convertedAmount))
                {
                    return new ConversionResponse
                    {
                        From = request.From,
                        To = request.To,
                        OriginalAmount = request.Amount,
                        ConvertedAmount = convertedAmount,
                        Rate = convertedAmount / request.Amount,
                        Date = result.Date
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting {Amount} from {From} to {To}", request.Amount, request.From, request.To);
                throw;
            }
        }

        public async Task<PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>>> GetHistoricalRatesAsync(HistoricalRatesRequest request)
        {
            string cacheKey = $"hist-{request.Base}-{request.StartDate:yyyyMMdd}-{request.EndDate:yyyyMMdd}";
            if (_cache.TryGetValue(cacheKey, out List<KeyValuePair<DateTime, ExchangeRateResponse>>? cached) && cached !=null)
            {
                _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                return Utilities.Utilities.Paginate(cached , request);
            }

            try
            {
                _logger.LogInformation("Fetching historical rates for {Base} from {StartDate} to {EndDate}",
                    request.Base, request.StartDate, request.EndDate);

                var response = await _httpClient.GetAsync($"{request.StartDate:yyyy-MM-dd}..{request.EndDate:yyyy-MM-dd}?from={request.Base}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                var dict = new Dictionary<DateTime, ExchangeRateResponse>();

                if (doc.RootElement.TryGetProperty("rates", out var ratesElement))
                {
                    foreach (var day in ratesElement.EnumerateObject())
                    {
                        var rates = new Dictionary<string, decimal>();
                        foreach (var rate in day.Value.EnumerateObject())
                        {
                            if (!Excluded.Contains(rate.Name) && rate.Value.TryGetDecimal(out var value))
                            {
                                rates[rate.Name] = value;
                            }
                        }

                        if (DateTime.TryParse(day.Name, out var date))
                        {
                            dict[date] = new ExchangeRateResponse
                            {
                                Base = request.Base,
                                Date = date,
                                Amount = 1,
                                Rates = rates
                            };
                        }
                    }
                }

                var allItems = dict.OrderBy(kv => kv.Key).ToList();

                var result =  Utilities.Utilities.Paginate(allItems, request);


                _cache.Set(cacheKey, allItems, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
                _logger.LogInformation("Cached historical data for {CacheKey}", cacheKey);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching historical rates for {Base}", request.Base);
                throw;
            }
        }
    }
}