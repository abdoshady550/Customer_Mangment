using System.Globalization;

namespace Customer_Mangment.IdentityServer.Middlewares
{

    public sealed class RequestCultureMiddleware(RequestDelegate next, ILogger<RequestCultureMiddleware> logger)
    {
        private static readonly HashSet<string> _supported = new(StringComparer.OrdinalIgnoreCase)
        {
            "en", "en-US", "ar", "ar-EG"
        };

        public async Task InvokeAsync(HttpContext context)
        {
            var culture = ResolveFromQuery(context)
                          ?? ResolveFromHeader(context)
                          ?? "en";

            if (!_supported.Contains(culture))
            {
                logger.LogDebug("Culture '{Culture}' not supported", culture);
                culture = "en";
            }

            var cultureInfo = new CultureInfo(culture);

            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            context.Items["RequestCulture"] = culture;

            logger.LogDebug("Request culture set to '{Culture}'", culture);
            logger.LogWarning("Culture: {Culture}", CultureInfo.CurrentUICulture);
            await next(context);
        }


        private static string? ResolveFromQuery(HttpContext context)
        {
            var lang = context.Request.Query["lang"].FirstOrDefault();
            return string.IsNullOrWhiteSpace(lang) ? null : lang.Trim();
        }

        private static string? ResolveFromHeader(HttpContext context)
        {
            var header = context.Request.Headers.AcceptLanguage.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(header)) return null;

            var primary = header.Split(',')[0].Trim().Split(';')[0].Trim();
            return string.IsNullOrWhiteSpace(primary) ? null : primary;
        }
    }
}