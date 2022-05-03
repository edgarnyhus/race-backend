using System;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Specifications
{
    public sealed class GetTenantsSpecification : BaseSpecification<Tenant>
    {
        public GetTenantsSpecification(Guid id) : base(x => x.Id.Equals(id))
        {
        }

        public GetTenantsSpecification(IQueryParameters parameters) : base(parameters)
        {
            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}