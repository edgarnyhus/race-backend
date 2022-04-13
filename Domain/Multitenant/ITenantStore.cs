using System.Threading.Tasks;
using Domain.Models;
using Domain.Models.Helpers;

namespace Domain.Multitenant
{
    public interface ITenantStore<T> where T : Tenant
    {
        Task<TenantInfo?> GetTenantAsync(string? identifier);
        Task<bool> HasParent(string id);
    }
}
