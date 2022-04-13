using System;
using System.Linq.Expressions;
using Domain.Interfaces;
using Domain.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace Domain.Specifications
{
    public class GetSignpostsSpecification : BaseSpecification<Signpost>
    {
        public GetSignpostsSpecification(Expression<Func<Signpost, bool>> criteria) : base(criteria)
        {
        }

        public GetSignpostsSpecification(IQueryParameters parameters) : base(parameters)
        {
            var signType = parameters.sign_type;

            if (!string.IsNullOrEmpty(parameters.within_radius))
            {
                var wd = JsonConvert.DeserializeObject<WithinDistance>(parameters.within_radius);
                if (!string.IsNullOrEmpty(wd.sign_type))
                    signType = wd.sign_type;

                var location = new Point(wd.longitude, wd.latitude) { SRID = 4326 };

                AddCriteria(c => c.Location != null && c.GeoLocation != null &&
                                 (c.GeoLocation.Distance(location) < wd.radius));
            }
            else if (!string.IsNullOrEmpty(parameters.within_square))
            {
                var we = JsonConvert.DeserializeObject<WithinEnvelope>(parameters.within_square);
                if (!string.IsNullOrEmpty(we.sign_type))
                    signType = we.sign_type;

                AddCriteria(c => c.Location != null && c.GeoLocation != null);
            }

            if (!string.IsNullOrEmpty(parameters.race_id))
            {
                if (Guid.TryParse(parameters.race_id, out Guid id))
                    AddCriteria(c => c.RaceId == id);
            }

            if (!string.IsNullOrEmpty(parameters.state))
            {
                Int32.TryParse(parameters.state, out int st);
                SignState signState = (SignState)st;
                AddCriteria(c => c.State == signState);
            }

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}