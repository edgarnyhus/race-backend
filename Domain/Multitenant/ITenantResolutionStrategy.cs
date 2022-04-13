using System.Threading.Tasks;

namespace Domain.Multitenant
{
    public interface ITenantResolutionStrategy
    {
        Task<string?> GetTenantIdentifierAsync();
        Task<string?> GetUserRolesAsync();
        Task<string?> GetUserEmailAddressAsync();
        string? GetRequestPath();
    }
}
