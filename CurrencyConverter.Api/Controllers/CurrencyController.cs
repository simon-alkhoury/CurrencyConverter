using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Policy = "UserOrAdmin")]
public class CurrencyController : ControllerBase
{
    private readonly ExchangeRateService _exchangeRateService;
    private readonly ILogger<CurrencyController> _logger;

    public CurrencyController(ExchangeRateService exchangeRateService, ILogger<CurrencyController> logger)
    {
        _exchangeRateService = exchangeRateService;
        _logger = logger;
    }

    /// <summary>Get the latest exchange rates for a specific base currency.</summary>
    [HttpGet("rates/latest")]
    [ProducesResponseType(typeof(ApiResponse<ExchangeRateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExchangeRateResponse>>> GetLatestRates([FromQuery] string baseCurrency = "EUR")
    {
        var rates = await _exchangeRateService.GetLatestRatesAsync(baseCurrency,ExchangeServiceProvider.FranFurter);

        return Ok(new ApiResponse<ExchangeRateResponse>
        {
            Success = true,
            Data = rates,
            TraceId = HttpContext.Response.Headers["X-Trace-Id"].FirstOrDefault()
        });
    }

    [HttpPost("convert")]
    [ProducesResponseType(typeof(ApiResponse<ConversionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ConversionResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ConversionResponse>>> ConvertCurrency([FromBody] ConversionRequest request)
    {
        if (CurrencyGuards.InvolvesExcluded(request.From, request.To))
            throw new ArgumentException("Currency conversion not supported for excluded currencies (TRY, PLN, THB, MXN).");

        var result = await _exchangeRateService.ConvertCurrencyAsync(request, ExchangeServiceProvider.FranFurter);

        if (result == null)
        {
            return BadRequest(new ApiResponse<ConversionResponse>
            {
                Success = false,
                Error = "Conversion not possible with provided currencies",
                TraceId = HttpContext.Response.Headers["X-Trace-Id"].FirstOrDefault(),
                  
            });
        }

        return Ok(new ApiResponse<ConversionResponse>
        {
            Success = true,
            Data = result,
            TraceId = HttpContext.Response.Headers["X-Trace-Id"].FirstOrDefault()
        });
    }

    [HttpGet("rates/historical")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>>>>> GetHistoricalRates(
        [FromQuery] string @base = "EUR",
        [FromQuery] DateTime startDate = default,
        [FromQuery] DateTime endDate = default,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (startDate == default || endDate == default || endDate < startDate)
            return BadRequest(new ApiResponse<object> { Success = false, Error = "Invalid date range." });

        var req = new HistoricalRatesRequest
        {
            Base = @base,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var paged = await _exchangeRateService.GetHistoricalRatesAsync(req, ExchangeServiceProvider.FranFurter);

        return Ok(new ApiResponse<PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>>>
        {
            Success = true,
            Data = paged,
            TraceId = HttpContext.Response.Headers["X-Trace-Id"].FirstOrDefault()
        });
    }
}

internal static class CurrencyGuards
{
    private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
            { "TRY", "PLN", "THB", "MXN" };

    public static bool InvolvesExcluded(string from, string to) =>
        Excluded.Contains(from) || Excluded.Contains(to);
}