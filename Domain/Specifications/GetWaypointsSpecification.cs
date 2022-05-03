using System;
using System.Linq.Expressions;
using Domain.Interfaces;
using Domain.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace Domain.Specifications
{
    public class GetWaypointsSpecification : BaseSpecification<Waypoint>
    {
        public GetWaypointsSpecification(Expression<Func<Waypoint, bool>> criteria) : base(criteria)
        {
        }

        public GetWaypointsSpecification(IQueryParameters parameters) : base(parameters)
        {
            Guid.TryParse(parameters.race_id, out Guid id);
            AddCriteria(c => c.RaceId == id);

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}