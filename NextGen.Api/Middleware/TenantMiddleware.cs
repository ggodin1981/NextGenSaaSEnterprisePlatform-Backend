namespace NextGen.Api.Middleware;

public static class TenantExtensions
{
    public static Guid? GetTenantId(this HttpContext context)
    {
        if (context.Items.TryGetValue("TenantId", out var value) && value is Guid g)
            return g;

        var claim = context.User.FindFirst("tenant_id");
        if (claim != null && Guid.TryParse(claim.Value, out var claimGuid))
            return claimGuid;

        return null;
    }
}

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
        {
            if (Guid.TryParse(tenantHeader!, out var tenantId))
            {
                context.Items["TenantId"] = tenantId;
                _logger.LogDebug("Tenant ID resolved from header: {TenantId}", tenantId);
            }
        }

        await _next(context);
    }
}
