namespace CurrencyConverter.Api.Middleware
{
    using CurrencyConverter.Api.Models;
    using System.Net;
    using System.Text.Json;

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var apiResponse = new ApiResponse<object>
            {
                Success = false,
                TraceId = context.Response.Headers["X-Trace-Id"].FirstOrDefault(),
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    apiResponse.Error = exception.Message;
                    break;
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    apiResponse.Error = "Unauthorized access";
                    break;
                case HttpRequestException:
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    apiResponse.Error = "External service unavailable";
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    apiResponse.Error = "An internal server error occurred";
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(apiResponse);
            await response.WriteAsync(jsonResponse);
        }
    }
}
