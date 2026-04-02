namespace Customer_Mangment.MultiTenancy;

public interface ITenantContext
{
    string TenantId { get; }
    string ConnectionString { get; }
    bool IsResolved { get; }
}
