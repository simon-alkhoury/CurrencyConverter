using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace CurrencyConverter.Tests
{
    public class unitTest
    {
        private FrankfurterService CreateService(IMemoryCache cache)
        {
            var httpClient = new HttpClient(new MockHttpMessageHandler());
            var settings = Options.Create(new CurrencySettings { CacheDurationMinutes = 10 });
            var logger = new LoggerFactory().CreateLogger<FrankfurterService>();
            return new FrankfurterService(httpClient, cache, settings, logger);
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_ReturnsCorrectPage_FromCache()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(cache);

            var dict = new Dictionary<DateTime, ExchangeRateResponse>
            {
                { new DateTime(2024, 1, 1), new ExchangeRateResponse { Date = new DateTime(2024, 1, 1), Base = "EUR", Amount = 1, Rates = new() } },
                { new DateTime(2024, 1, 2), new ExchangeRateResponse { Date = new DateTime(2024, 1, 2), Base = "EUR", Amount = 1, Rates = new() } },
                { new DateTime(2024, 1, 3), new ExchangeRateResponse { Date = new DateTime(2024, 1, 3), Base = "EUR", Amount = 1, Rates = new() } }
            };
            var allItems = dict.OrderBy(kv => kv.Key).ToList();
            cache.Set("hist-EUR-20240101-20240103", allItems, TimeSpan.FromMinutes(10));

            var request = new HistoricalRatesRequest
            {
                Base = "EUR",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 3),
                Page = 2,
                PageSize = 1
            };

            // Act
            var result = await service.GetHistoricalRatesAsync(request);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(new DateTime(2024, 1, 2), result.Items.First().Key);
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_ReturnsEmpty_WhenCacheIsEmpty()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(cache);

            var request = new HistoricalRatesRequest
            {
                Base = "EUR",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 3),
                Page = 1,
                PageSize = 1
            };

            // Act
            var result = await service.GetHistoricalRatesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        // Mock handler to avoid real HTTP calls
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var json = "{\"rates\":{}}";
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(json)
                });
            }
        }
    }
}
