namespace Customer_Mangment.Middlewares
{
    public class LoggerMiddleware(ILogger<LoggerMiddleware> logger) : IMiddleware
    {
        private readonly ILogger<LoggerMiddleware> _logger = logger;

        private const string MaskedEndpoint = "/api/Auth/token/generate";
        private const string SkipResponseEndpoint = "/api/CustomerReport/download";
        private const string SkipResponseDocEndpoint = "/openapi/v1.json";
        private const string HealthEndpoint = "/health";
        private const string scalarEndpoint = "/scalar";




        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path.Value ?? "";

            if (path.StartsWith(HealthEndpoint, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(scalarEndpoint, StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            var isMasked = path.Equals(MaskedEndpoint, StringComparison.OrdinalIgnoreCase);
            var skipResponse = path.Equals(SkipResponseEndpoint, StringComparison.OrdinalIgnoreCase);
            var skipDocResponse = path.Equals(SkipResponseDocEndpoint, StringComparison.OrdinalIgnoreCase);

            // Request Body
            context.Request.EnableBuffering();
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Response Body
            var originalStream = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await next(context);

            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();
            memStream.Position = 0;
            await memStream.CopyToAsync(originalStream);
            context.Response.Body = originalStream;

            var finalResponseBody = skipResponse || skipDocResponse
                ? "Skipped"
                : (isMasked ? "***MASKED***" : responseBody);

            _logger.LogInformation("""
            ─ Request
            Method : {Method}
            Path   : {Path}
            Body   : {RequestBody}
            ─ Response
            Status : {Status}
            Body   : {ResponseBody}
            ──
            """,
                context.Request.Method,
                path,
                isMasked ? "***MASKED***" : requestBody,
                context.Response.StatusCode,
                finalResponseBody);
        }
    }
}