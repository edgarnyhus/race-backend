using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Domain.Models;
using Location = Domain.Models.Location;

namespace Domain.Interfaces
{
    public interface IRepository<T> where T : EntityBase
    {
        Task<IEnumerable<T>> Find(ISpecification<T> specification);
        Task<T> FindById(Guid id);
        Task<T> FindById(ISpecification<T> specification);
        Task<T> Add(T entity);
        Task<bool> Update(Guid id, T entity);
        Task<bool> Remove(Guid id);
        Task<bool> Remove(T entity);
        bool Contains(ISpecification<T> specification);
        bool Contains(Expression<Func<T, bool>> predicate);
        int Count(ISpecification<T> specification);
        int Count(Expression<Func<T, bool>> predicate);
    }

    public abstract class EntityBase
    {
        public Guid? Id { get; /*protected internal*/ set; }
    }
}
