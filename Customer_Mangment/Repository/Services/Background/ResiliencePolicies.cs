using Polly;
using Polly.CircuitBreaker;

namespace Customer_Mangment.Repository.Services.Background
{
    public static class ResiliencePolicies
    {

        public static IAsyncPolicy GetRetryPolicy(ILogger logger) =>
            Policy
                .Handle<Exception>(ex => ex is not BrokenCircuitException)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, attempt, _) =>
                    {
                        logger.LogWarning(
                            "Retry {Attempt}/3 after {Delay}s — {Error}",
                            attempt, timeSpan.TotalSeconds, exception.Message);
                    });

        public static IAsyncPolicy GetCircuitBreakerPolicy(ILogger logger) =>
            Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, duration) =>
                    {
                        logger.LogError(
                            "Circuit OPEN for {Duration}s — {Error}",
                            duration.TotalSeconds, exception.Message);
                    },
                    onReset: () => logger.LogInformation("Circuit CLOSED — back to normal"),
                    onHalfOpen: () => logger.LogInformation("Circuit HALF-OPEN — testing..."));

        public static IAsyncPolicy GetCombinedPolicy(ILogger logger) =>
            Policy.WrapAsync(
                GetRetryPolicy(logger),
                GetCircuitBreakerPolicy(logger));
    }
}
