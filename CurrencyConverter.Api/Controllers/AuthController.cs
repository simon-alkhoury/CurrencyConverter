using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwt;

        public AuthController(IJwtTokenService jwt) => _jwt = jwt;

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<LoginResponse>> Login([FromBody] Api.Models.LoginRequest request)
        {
            if (!_jwt.ValidateCredentials(request.Username, request.Password))
                return Unauthorized(new ApiResponse<LoginResponse> { Success = false, Error = "Invalid credentials" });

            var role = request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";
            var token = _jwt.GenerateToken(request.Username, role);

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Data = new LoginResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(double.Parse(
                        HttpContext.RequestServices.GetRequiredService<IConfiguration>()["JwtSettings:ExpiryHours"] ?? "1")),
                    Role = role
                }
            });
        }
    }
}
