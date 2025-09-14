using CurrencyConverter.Api.Models;

namespace CurrencyConverter.Api.Services.Interfaces
{
    public interface ICurrencyProviderFactory
    {
        IExchangeRateService GetProvider(ExchangeServiceProvider exchangeServiceProvider);
    }
}