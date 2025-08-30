using System.Diagnostics;

namespace CurrencyConverter.Api.Http
{
    public class CorrelationHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

            if (!request.Headers.Contains("X-Trace-Id"))
                request.Headers.Add("X-Trace-Id", traceId);

            Activity.Current ??= new Activity("OutgoingHttp").Start();
            using var _ = Activity.Current;

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
