using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
    public interface ITenantService
    {
        public Task<IEnumerable<TenantDto>> GetTenants(QueryParameters queryParameters);
        public Task<TenantDto> GetTenantById(string id);
        public Task<TenantDto> CreateTenant(TenantContract contract);
        public Task<bool> UpdateTenant(string id, TenantContract contract);
        public Task<bool> DeleteTenant(string id);

    }
}