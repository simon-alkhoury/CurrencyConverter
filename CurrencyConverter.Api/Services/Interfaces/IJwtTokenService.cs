namespace CurrencyConverter.Api.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(string username, string role);
        bool ValidateCredentials(string username, string password);
    }
}