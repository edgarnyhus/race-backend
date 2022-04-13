using System.Threading.Tasks;
using Domain.Models;
using Domain.Models.Helpers;

namespace Domain.Interfaces
{
    public interface ITenantRepository : IRepository<Tenant>
    {
        Task<TenantInfo> FindByIdentifier(string identifier);
        Task<bool> HasParent(string id);
    }
}