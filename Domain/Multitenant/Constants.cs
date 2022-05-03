namespace Domain.Multitenant
{
    public class Constants
    {
        public const string TenantGlobalAdminDomain = "locusbase.no";
        public const string TenantUserRole = "user";
        public const string TenantAdminRole = "admin";
        public const string TenantGlobalAdminRole = "superadmin";
        public const string TenantDomain = "https://locusbase.no/domain";     // HARDCODED Auth0 hook (see Auth0 Dashboard -> Auth Pipeline -> Hooks)
    }
} 