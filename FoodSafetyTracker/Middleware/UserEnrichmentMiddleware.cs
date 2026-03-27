using Serilog.Context;

namespace FoodSafetyTracker.Middleware;

public class UserEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public UserEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Push the current username into every log event for this request
        var userName = context.User?.Identity?.Name ?? "anonymous";
        using (LogContext.PushProperty("UserName", userName))
        {
            await _next(context);
        }
    }
}
