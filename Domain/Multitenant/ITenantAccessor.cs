using Domain.Models;

namespace Domain.Multitenant
{
    public interface ITenantAccessor<T> where T : Tenant
    {
        T Tenant { get; }
    }
}