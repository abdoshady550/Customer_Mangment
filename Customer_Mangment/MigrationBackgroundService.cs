using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Services.Background;
using Polly.CircuitBreaker;
using Quartz;

namespace Customer_Mangment
{
    [DisallowConcurrentExecution]
    public sealed class MigrationJob(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<MigrationJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var flags = configuration
                .GetSection(FeatureFlags.SectionName)
                .Get<FeatureFlags>() ?? new FeatureFlags();

            if (!flags.RunMigrationOnStartup)
            {
                logger.LogInformation("MigrationJob: skipped (RunMigrationOnStartup = false)");
                return;
            }

            var policy = ResiliencePolicies.GetCombinedPolicy(logger);

            try
            {
                await policy.ExecuteAsync(async () =>
                {

                    using var scope = scopeFactory.CreateScope();
                    var migrationService = scope.ServiceProvider
                        .GetRequiredService<IMigrationService>();

                    var direction = flags.UseMongoDb ? "SQL → MongoDB" : "MongoDB → SQL";
                    logger.LogInformation("MigrationJob: starting ({Direction})", direction);

                    var result = flags.UseMongoDb
                        ? await migrationService.MigrateSqlToMongoAsync(context.CancellationToken)
                        : await migrationService.MigrateMongoToSqlAsync(context.CancellationToken);

                    if (!result.Success)

                        throw new InvalidOperationException(
                            $"Migration failed: {string.Join(" | ", result.Errors)}");

                    logger.LogInformation(
                        "Migration done. Direction={D} Customers={C} Addresses={A} Tokens={T} Users={U}",
                        result.Direction, result.CustomersProcessed,
                        result.AddressesProcessed, result.RefreshTokensProcessed,
                        result.UsersProcessed);
                });
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError("MigrationJob: Circuit is OPEN, skipping — {Error}", ex.Message);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("MigrationJob: cancelled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MigrationJob: all retries exhausted.");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}