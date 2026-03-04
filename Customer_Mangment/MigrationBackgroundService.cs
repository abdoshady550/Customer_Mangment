using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
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

            var direction = flags.UseMongoDb ? "SQL To MongoDB" : "MongoDB To SQL";
            logger.LogInformation("MigrationJob: starting ({Direction})", direction);

            using var scope = scopeFactory.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();

            try
            {
                MigrationResult result = flags.UseMongoDb
                    ? await migrationService.MigrateSqlToMongoAsync(context.CancellationToken)
                    : await migrationService.MigrateMongoToSqlAsync(context.CancellationToken);

                if (result.Success)
                {
                    logger.LogInformation(
                        "Migration completed. Direction={Direction} Customers={C} Addresses={A} Tokens={T} Users={U}",
                        result.Direction,
                        result.CustomersProcessed,
                        result.AddressesProcessed,
                        result.RefreshTokensProcessed,
                        result.UsersProcessed);
                }
                else
                {
                    logger.LogWarning(
                        "Migration finished with {ErrorCount} error(s): {Errors}",
                        result.Errors.Count,
                        string.Join(" | ", result.Errors));
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("MigrationJob: cancelled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MigrationJob: fatal error.");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}