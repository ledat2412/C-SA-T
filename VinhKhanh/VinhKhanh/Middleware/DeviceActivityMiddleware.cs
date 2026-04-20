using Microsoft.Extensions.Caching.Memory;
using VinhKhanh.Services;

namespace VinhKhanh.Middleware
{
    public class DeviceActivityMiddleware
    {
        private const string DeviceHeaderName = "X-Device-Id";
        private static readonly TimeSpan DedupWindow = TimeSpan.FromSeconds(10);

        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeviceActivityMiddleware> _logger;

        public DeviceActivityMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            IServiceScopeFactory scopeFactory,
            ILogger<DeviceActivityMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode >= 400)
                return;

            if (!context.Request.Headers.TryGetValue(DeviceHeaderName, out var headerValue))
                return;

            var maThietBi = headerValue.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(maThietBi) || maThietBi.Length > 100)
                return;

            var cacheKey = "device_touch::" + maThietBi;
            if (_cache.TryGetValue(cacheKey, out _))
                return;

            _cache.Set(cacheKey, true, DedupWindow);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var deviceService = scope.ServiceProvider.GetRequiredService<DeviceService>();
                    await deviceService.TouchByCodeAsync(maThietBi);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Passive touch failed for device {MaThietBi}", maThietBi);
                }
            });
        }
    }
}
