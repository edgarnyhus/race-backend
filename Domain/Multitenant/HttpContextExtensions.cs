using Microsoft.AspNetCore.Http;
using Domain.Models;

namespace Domain.Multitenant
{
    /// Extensions to HttpContext to make multi-tenancy easier to use
    public static class HttpContextExtensions
    {
        /// Returns the current tenant
        public static T GetTenant<T>(this HttpContext context) where T : Tenant
        {
            if (!context.Items.ContainsKey(Constants.TenantGlobalAdminDomain))
                return null;
            return context.Items[Constants.TenantGlobalAdminDomain] as T;
        }
    
        /// Returns the current Tenant
        public static Tenant GetTenant(this HttpContext context)
        {
            return context.GetTenant<Tenant>();
        }
    }}