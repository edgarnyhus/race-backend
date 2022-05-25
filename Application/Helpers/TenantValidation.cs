using Domain.Interfaces;
using Domain.Models;
using Domain.Multitenant;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public class TenantValidation
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly bool _multitenancy = false;

        public TenantValidation(TenantAccessService<Tenant> tenantAccessService, bool multitenancy)
        {
            _tenantAccessService = tenantAccessService;
            _multitenancy = multitenancy;
        }

        /// <summary>
        /// Get tenant_id from user's email address.
        /// If user's organization is not top level in an organization tree (parent_id is not set),
        /// remove organization_id as filter - meaning that the top level organization will retrieve
        /// data includinbg all sub-organizations.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task Validate(IQueryParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.tenant_id))
            {
                var tenant = await _tenantAccessService.GetTenantAsync();
                if (tenant != null)
                {
                    parameters.tenant_id = tenant.TenantId?.ToString() ?? null;
                    if (string.IsNullOrEmpty(parameters.organization_id))
                        parameters.organization_id = tenant.OrganizationId?.ToString() ?? null;
                }
                if (!string.IsNullOrEmpty(parameters.organization_id))
                {
                    var has_parent = await _tenantAccessService.HasParent(parameters.organization_id);
                    if (!has_parent)
                        parameters.organization_id = null;
                }

                parameters.multitenancy = !await _tenantAccessService.IsGlobalAdministrator() && _multitenancy;
            }
            else
                parameters.multitenancy = _multitenancy;
        }
    }
}
