using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;
using Infrastructure.Data.Context;
using Z.EntityFramework.Plus;

namespace Infrastructure.Data.Repositories.Helpers
{
    public class SpecificationEvaluator<T> where T : EntityBase
    {
        protected RaceBackendDbContext _dbContext { get; set; }

        public SpecificationEvaluator(RaceBackendDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification, bool applyPaging)
        {
            var query = inputQuery;

            if (specification == null)
                return query;

            // modify the IQueryable using the specification's criteria expression
            query = specification.Criteria.Aggregate(query,
                (current, criteria) => current.Where(criteria));

            // Includes all expression-based includes
            query = specification.Includes.Aggregate(query,
                (current, include) => current.Include(include));

            // Include any string-based include statements
            query = specification.IncludeStrings.Aggregate(query,
                (current, include) => current.Include(include));

            query = specification.IncludeFilters.Aggregate(query,
                (current, include) => current.IncludeFilter(include));

            // Apply ordering if expressions are set
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            if (specification.GroupBy != null)
            {
                query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
            }

            if (applyPaging && specification.IsPagingEnabled)
            {
                query = query
                    .Skip(specification.Skip)
                    .Take(specification.Take);
            }

            return query;
        }

        public static IEnumerable<T> ApplyPaging(IEnumerable<T> inputQuery, ISpecification<T> specification)
        {
            var query = inputQuery;


            //Apply paging if enabled
            if (specification.IsPagingEnabled)
            {
                query = query
                    .Skip(specification.Skip)
                    .Take(specification.Take);
            }
            
            return query;
        }
    }
}

