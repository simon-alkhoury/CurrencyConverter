namespace CurrencyConverter.Api.Middleware
{
    using System.Diagnostics;
    using System.Security.Claims;

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var clientId = context.User.FindFirst("clientId")?.Value ?? "Anonymous";
            var method = context.Request.Method;
            var endpoint = context.Request.Path;
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

            context.Response.Headers.Add("X-Trace-Id", traceId);

            _logger.LogInformation("Request started: {Method} {Endpoint} from {ClientIp} (Client: {ClientId}, Trace: {TraceId})",
                method, endpoint, clientIp, clientId, traceId);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var responseCode = context.Response.StatusCode;
                var responseTime = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "Request completed: {Method} {Endpoint} responded {StatusCode} in {ResponseTime}ms " +
                    "(Client: {ClientId}, IP: {ClientIp}, Trace: {TraceId})",
                    method, endpoint, responseCode, responseTime, clientId, clientIp, traceId);
            }
        }
    }
}
