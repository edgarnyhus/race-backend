namespace Domain.Multitenant
{
    public class Constants
    {
        public const string TenantGlobalAdminDomain = "vink-kort.no";
        public const string TenantUserRole = "user";
        public const string TenantAdminRole = "admin";
        public const string TenantGlobalAdminRole = "superadmin";
        public const string TenantDomain = "https://vink-kort.no/domain";     // HARDCODED Auth0 hook (see Auth0 Dashboard -> Auth Pipeline -> Hooks)
    }
} 