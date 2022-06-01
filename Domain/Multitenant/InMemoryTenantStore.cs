using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Models.Helpers;

namespace Domain.Multitenant
{
    /// In memory store for testing
    public class InMemoryTenantStore : ITenantStore<Tenant>
    {
        List<Tenant> tenants = new List<Tenant>()
        {
            new Tenant()
            {
                Id = Guid.NewGuid(),
                Identifier = "get.com"
            },
        };

        /// Get a tenant for a given identifier
        public async Task<TenantInfo?> GetTenantAsync(string? identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            TenantInfo tenantInfo = new TenantInfo();
            var tenant = await Task.Run(() => tenants.FirstOrDefault(t => t.Identifier.Contains(identifier)) );
            tenantInfo.TenantId = tenant?.Id;
            return tenantInfo;
        }

        public async Task<Tenant?> GetTenantByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var tenant = tenants.FirstOrDefault(t => t.Name == name);
            return await Task.FromResult(tenant);
        }

        public async Task<bool> HasParent(string id)
        {
            return await Task.Run(() => false );
        }

    }
}
