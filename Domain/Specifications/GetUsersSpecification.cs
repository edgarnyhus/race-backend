using System;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Specifications
{
    public sealed class GetUsersSpecification : BaseSpecification<User>
    {
        public GetUsersSpecification(string id) : base(x => x.UserId.Equals(id, StringComparison.OrdinalIgnoreCase))
        {

        }

        public GetUsersSpecification(IQueryParameters parameters) : base(parameters)
        {
            if (!string.IsNullOrEmpty(parameters.phone_number))
            {
                AddCriteria(c => c.PhoneNumber == parameters.phone_number);
            }
            else if (!string.IsNullOrEmpty(parameters.email))
            {
                AddCriteria(c => parameters.email.Equals(c.Email, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                if (parameters.multitenancy)
                {
                    if (Guid.TryParse(parameters.tenant_id, out Guid tenantId))
                        AddCriteria(c => c.TenantId == tenantId);
                }

                if (Guid.TryParse(parameters.organization_id, out Guid id))
                    AddCriteria(c => c.OrganizationId == id);
            }

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}