using Microsoft.Extensions.DependencyInjection;
using Domain.Models;

namespace Domain.Multitenant
{
    /// Nice method to create the tenant builder
    public static class ServiceCollectionExtensions
    {
        /// Add the services (application specific tenant class)
        public static TenantBuilder<T> AddMultiTenancy<T>(this IServiceCollection services) where T : Tenant
            => new TenantBuilder<T>(services);

        /// Add the services (default tenant class)
        public static TenantBuilder<Tenant> AddMultiTenancy(this IServiceCollection services)
            => new TenantBuilder<Tenant>(services);
    }
}
