using System;
using System.Linq.Expressions;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Specifications;

public class GetSignTypesSpecification : BaseSpecification<SignType>
{
    public GetSignTypesSpecification(Expression<Func<SignType, bool>> criteria) : base(criteria)
    {
    }

    public GetSignTypesSpecification(IQueryParameters parameters) : base(parameters)
    {
        if (parameters.multitenancy)
        {
            Guid.TryParse(parameters.tenant_id, out Guid tenantId);
            AddCriteria(c => c.TenantId == tenantId);
        }

        if (parameters.page_size > 0)
        {
            var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
            ApplyPaging(page * parameters.page_size, parameters.page_size);
        }
    }

}