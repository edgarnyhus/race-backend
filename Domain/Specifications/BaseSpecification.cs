using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite;
using Domain.Interfaces;
using Domain.Models;
using Domain.Queries.Helpers;
using Coordinate = NetTopologySuite.Geometries.Coordinate;

namespace Domain.Specifications
{
    public class WithinDistance
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public double radius { get; set; }
        public string? signtype { get; set; }
    }

    public class WithinEnvelope
    {
        public double longitude1 { get; set; }
        public double latitude1 { get; set; }
        public double longitude2 { get; set; }
        public double latitude2 { get; set; }
        public string? signtype { get; set; }
    }

    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            AddCriteria(criteria);
        }
        protected BaseSpecification(IQueryParameters parameters)
        {
            Parameters = parameters;
        }

        public IEnumerable<T>? Query { get; set; }
        public string? Sql { get; set; }
        public List<Expression<Func<T, bool>>> Criteria { get; set; } = new List<Expression<Func<T, bool>>>();
        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
        public List<string> IncludeStrings { get; } = new List<string>();
        public List<Expression<Func<T, object>>> IncludeFilters { get; set; } = new List<Expression<Func<T, object>>>();
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public Expression<Func<T, object>>? GroupBy { get; private set; }

        public virtual IQueryParameters? Parameters { get; set;  }

        public int Take { get; set; }
        public int Skip { get; set; }
        public bool IsPagingEnabled { get; set; } = false;

        public virtual void AddQuery(IEnumerable<T> query)
        {
            Query = query;
        }

        protected virtual void AddSql(string sqlExpression)
        {
            Sql = sqlExpression;
        }

        protected virtual void AddCriteria(Expression<Func<T, bool>> criteriaExpression)
        {
            Criteria.Add(criteriaExpression);
        }

        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected virtual void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        protected virtual void AddIncludeFilter(Expression<Func<T, object>> includeExpression)
        {
            IncludeFilters.Add(includeExpression);
        }

        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }

        protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
        }
    }
}
