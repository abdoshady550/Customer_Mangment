using Customer_Mangment.Model.Results;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface IMigrationService
    {
        Task<MigrationResult> MigrateSqlToMongoAsync(CancellationToken ct = default);
        Task<MigrationResult> MigrateMongoToSqlAsync(CancellationToken ct = default);
    }
}
