using System;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Specifications
{
    public class GetUserSettingsSpecification : BaseSpecification<UserSettings>
    {
        public GetUserSettingsSpecification(string id) : base(x => x.Id == new Guid(id))
        {

        }

        public GetUserSettingsSpecification(IQueryParameters parameters) : base(parameters)
        {
            if (!string.IsNullOrEmpty(parameters.user_id))
            {
                Guid.TryParse(parameters.user_id, out Guid userId);
                AddCriteria(us => us.UserId == userId);
            }

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}
