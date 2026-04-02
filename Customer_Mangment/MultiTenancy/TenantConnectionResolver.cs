namespace Customer_Mangment.MultiTenancy;

public sealed class TenantConnectionResolver
{
    private readonly IConfiguration _configuration;
    private readonly string _defaultConnectionString;

    public TenantConnectionResolver(IConfiguration configuration)
    {
        _configuration = configuration;

        _defaultConnectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is required and was not found in configuration.");
    }


    public string? Resolve(string tenantId)
    {

        var aspireConnection = _configuration.GetConnectionString(tenantId);
        if (!string.IsNullOrWhiteSpace(aspireConnection))
            return aspireConnection;

        var explicitConnection = _configuration[$"Tenants:{tenantId}:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(explicitConnection))
            return explicitConnection;

        if (_configuration.GetSection($"Tenants:{tenantId}").Exists())
            return _defaultConnectionString;

        return null;
    }

    public IEnumerable<string> RegisteredTenants =>
        _configuration.GetSection("Tenants")
                       .GetChildren()
                       .Select(s => s.Key);
}