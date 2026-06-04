
namespace Marketplace.Web.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
            correlationId = Guid.NewGuid().ToString("N");

        context.Response.Headers[CorrelationIdHeader] = correlationId;
        await _next(context);
    }
}
