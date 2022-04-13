using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Domain.Multitenant
{
    public class DomainResolutionStrategy : ITenantResolutionStrategy
    {
        private readonly IConfiguration _config;
        private readonly AuthenticationApiClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DomainResolutionStrategy(IConfiguration config, AuthenticationApiClient client,
            IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> GetTenantIdentifierAsync()
        {
            return await Task.Run(() =>
            {
                var value = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type.EndsWith("email"))
                    ?.Value;
                
                if (string.IsNullOrEmpty(value))
                {
                    // Retrieve domainname from claims - domain added to Auth0 Application Client Metadata
                    var v = _httpContextAccessor.HttpContext?.User.Claims
                        .FirstOrDefault(c => c.Type == Constants.TenantDomain)?.Value;
                    if (!string.IsNullOrEmpty(v))
                        return v;

                    // Retrieve domainname from claims - domain added as Auth0 permission
                    v = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "scope")?.Value;
                    if (!string.IsNullOrEmpty(v))
                    {
                        var list = v.Split(' ');
                        value = list?.FirstOrDefault(c => c.StartsWith("domain:"));
                    }
                }

                return string.IsNullOrEmpty(value) ? null : value.Split('@')[1];
            });
        }

        public async Task<string?> GetUserEmailAddressAsync()
        {
            var value = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type.EndsWith("email"))
                ?.Value;
            if (string.IsNullOrEmpty(value))
            {
                value = await GetTenantIdentifierAsync();
                if (!string.IsNullOrEmpty(value))
                    value = $"post@{value}";
            }

            return value;
        }

        public async Task<string?> GetUserRolesAsync()
        {
            var roles = await Task.Run(() =>
                _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type.EndsWith("roles"))?.Value
            );
            return roles;
        }

        public string? GetRequestPath()
        {
            var value = _httpContextAccessor.HttpContext?.Request.Path.Value;
            return value;
        }
    }
}