namespace CurrencyConverter.Api.Services.Interfaces
{
    public interface ICurrencyProviderFactory
    {
        IExchangeRateService GetProvider(string providerName = "frankfurter");
    }
}