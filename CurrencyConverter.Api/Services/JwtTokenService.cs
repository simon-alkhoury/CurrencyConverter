using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CurrencyConverter.Api.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        private readonly Dictionary<string, (string Password, string Role)> _users =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["admin"] = ("admin123", "Admin"),
                ["user"] = ("user123", "User")
            };

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool ValidateCredentials(string username, string password)
        {
            return _users.TryGetValue(username, out var creds) && creds.Password == password;
        }

        public string GenerateToken(string username, string role)
        {
            var secret = _configuration["JwtSettings:Secret"]
                         ?? throw new InvalidOperationException("JwtSettings:Secret is not configured."); 
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expiryHours = double.Parse(_configuration["JwtSettings:ExpiryHours"] ?? "1");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("clientId", username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
