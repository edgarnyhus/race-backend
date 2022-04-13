using Microsoft.AspNetCore.Builder;
using Domain.Models;

namespace Domain.Multitenant
{
    /// Nice method to register our middleware
    public static class IApplicationBuilderExtensions
    {
        /// Use the Teanant Middleware to process the request
        public static IApplicationBuilder UseMultiTenancy<T>(this IApplicationBuilder builder) where T : Tenant 
            => builder.UseMiddleware<TenantMiddleware<T>>();


        /// Use the Teanant Middleware to process the request
        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder builder) 
            => builder.UseMiddleware<TenantMiddleware<Tenant>>();
    }
}