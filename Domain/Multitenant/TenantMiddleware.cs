using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Domain.Models;

namespace Domain.Multitenant
{
    internal class TenantMiddleware<T> where T : Tenant
    {
        private readonly RequestDelegate next;

        public TenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Items.ContainsKey(Constants.TenantGlobalAdminDomain))
            {
                var tenantService = context.RequestServices.GetService(typeof(TenantAccessService<T>)) as TenantAccessService<T>;
                context.Items.Add(Constants.TenantGlobalAdminDomain, await tenantService?.GetTenantAsync());
            }

            //Continue processing
            if (next != null)
                await next(context);
        }
    }
}