namespace Customer_Mangment.MultiTenancy
{
    internal sealed class TenantContextAccessor : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string TenantId => _httpContextAccessor.HttpContext?.Items["TenantId"] as string
            ?? throw new InvalidOperationException("TenantId not resolved.");

        public string ConnectionString => _httpContextAccessor.HttpContext?.Items["TenantConnectionString"] as string
            ?? throw new InvalidOperationException("ConnectionString not resolved.");

        public bool IsResolved { get; set; }
    }
}
