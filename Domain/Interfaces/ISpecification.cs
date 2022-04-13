using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Domain.Interfaces
{
    //public interface ISpecification<T>
    //{
    //    Expression<Func<T, bool>> Criteria { get; }
    //    List<Expression<Func<T, object>>> Includes { get; }
    //    List<string> IncludeStrings { get; }
    //}

    public interface ISpecification<T>
    {
        IEnumerable<T>? Query { get; set; }
        List<Expression<Func<T, bool>>> Criteria { get; }
        string? Sql { get; set; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        List<Expression<Func<T, object>>> IncludeFilters { get; set;  }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }
        Expression<Func<T, object>>? GroupBy { get; }

        IQueryParameters? Parameters { get; }

        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }

        void AddQuery(IEnumerable<T> query);

    }
}
