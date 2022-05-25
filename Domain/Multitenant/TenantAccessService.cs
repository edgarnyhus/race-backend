using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Domain.Models;
using Domain.Models.Helpers;
using System;

namespace Domain.Multitenant
{
    /// Tenant access service
    public class TenantAccessService<T> where T : Tenant
    {
        private readonly ITenantResolutionStrategy _tenantResolutionStrategy;
        private readonly ITenantStore<T> _tenantStore;
        private readonly IMediator _mediator;

        public TenantAccessService(ITenantResolutionStrategy tenantResolutionStrategy, ITenantStore<T> tenantStore, IMediator mediator)
        {
            _tenantResolutionStrategy = tenantResolutionStrategy;
            _tenantStore = tenantStore;
            _mediator = mediator;
        }

        /// Get the current tenant
        public async Task<TenantInfo?> GetTenantAsync()
        {
            var tenantIdentifier = await _tenantResolutionStrategy.GetTenantIdentifierAsync();
            return await _tenantStore.GetTenantAsync(tenantIdentifier);
        }
        
        public async Task<TenantInfo?> GetTenantAsync(string identifier)
        {
           var tenant = await _tenantStore.GetTenantAsync(identifier);
            if (tenant == null)
                tenant = await GetTenantAsync();
            
            return tenant;
        }

        public async Task<string?> GetTenantIdentifierAsync()
        {
            string? tenantIdentifier = await _tenantResolutionStrategy.GetTenantIdentifierAsync();
            return tenantIdentifier;
        }

        public async Task<string?> GetUserRolesAsync()
        {
            var roles = await _tenantResolutionStrategy.GetUserRolesAsync();
            return roles;
        }

        public async Task<string?> GetUserEmailAddressAsync()
        {
            var email = await _tenantResolutionStrategy.GetUserEmailAddressAsync();
            return email;
        }

        public string? GetRequestPath()
        {
            var path = _tenantResolutionStrategy.GetRequestPath();
            return path;
        }

        public string GetRaceIdFromRequestPath()
        {
            try
            {
                // Get RaceId from request path - api/races/{race_id}/signs

                var path = _tenantResolutionStrategy.GetRequestPath();
                var arr = path.Split('/');
                var raceId = arr.First<string>(x => IsValidGuid(x));
                return raceId;
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid race_id in request path.");
            }
        }

        public bool IsValidGuid(string str)
        {
            var isValid = Guid.TryParse(str, out _);
            return isValid;
        }

        public async Task<bool> IsAdministrator()
        {
            if (await IsGlobalAdministrator())
                return true;
            
            var roles = await _tenantResolutionStrategy.GetUserRolesAsync();
            if (string.IsNullOrEmpty(roles))
                return false;
            var roleList = roles.Split(',');
            var result = roleList.SingleOrDefault(c => c == Constants.TenantAdminRole);
            return (!string.IsNullOrEmpty(result));
        }

        public async Task<bool> IsGlobalAdministrator()
        {
            // All users with role 'superadmin' or with domain:vink.kort.no
            var roles = await _tenantResolutionStrategy.GetUserRolesAsync();
            if (string.IsNullOrEmpty(roles))
            {
                var identifier = await _tenantResolutionStrategy.GetTenantIdentifierAsync();
                if (string.IsNullOrEmpty(identifier))
                    return false;

                //if (Constants.TenantGlobalAdminDomain.Equals(identifier, System.StringComparison.OrdinalIgnoreCase))
                //    return true;
                var identifierList = identifier.Split(',');
                var res = identifierList.SingleOrDefault(c => c == Constants.TenantGlobalAdminDomain);
                return (!string.IsNullOrEmpty(res));
            }
            var roleList = roles.Split(',');
            var result = roleList.SingleOrDefault(c => c == Constants.TenantGlobalAdminRole);
            return (!string.IsNullOrEmpty(result));
        }

        public async Task<bool> HasParent(string id)
        {
            var result = await _tenantStore.HasParent(id);
            return result;
        }
    }
}
