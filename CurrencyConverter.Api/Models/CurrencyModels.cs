namespace CurrencyConverter.Api.Models
{
    public class ExchangeRateResponse
    {
        public decimal Amount { get; set; }
        public string Base { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }

    public class ConversionRequest
    {
        public decimal Amount { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }

    public class ConversionResponse
    {
        public decimal Amount { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;

        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal Result { get; set; }
        public DateTime Date { get; set; }
        public decimal Rate { get; set; }
    }

    public class HistoricalRatesRequest
    {
        public string Base { get; set; } = "EUR";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalItems { get; set; }                                
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;
    }

    public class CurrencySettings
    {
        public List<string> ExcludedCurrencies { get; set; } = new() { "TRY", "PLN", "THB", "MXN" };
        public int CacheDurationMinutes { get; set; } = 60;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public enum ExchangeServiceProvider
    {
        FranFurter,
        Fixer
    }
}
