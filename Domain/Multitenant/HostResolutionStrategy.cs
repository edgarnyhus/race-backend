using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Domain.Multitenant
{
    /// Resolve the host to a tenant identifier
    public class HostResolutionStrategy : ITenantResolutionStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HostResolutionStrategy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// Get the tenant identifier
        public async Task<string?> GetTenantIdentifierAsync()
        {
            return await Task.FromResult(_httpContextAccessor?.HttpContext?.Request.Host.Host);
        }

        public Task<string?> GetUserRolesAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<string?> GetUserEmailAddressAsync()
        {
            throw new System.NotImplementedException();
        }

        public string? GetRequestPath()
        {
            throw new System.NotImplementedException();
        }
    }
}
