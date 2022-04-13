using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.Helpers;


namespace Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : EntityBase
    {
        private protected readonly RaceBackendDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<Repository<T>> _logger;
        public bool IsDevelopment { get; set; }

        public Repository(RaceBackendDbContext dbContext, IMapper mapper, ILogger<Repository<T>> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            IsDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        }

        public virtual async Task<IEnumerable<T>> Find(ISpecification<T> specification)
        {
            var result = ApplySpecification(specification, true);
            return await result.ToListAsync();
        }

        public virtual async Task<T> FindById(Guid id)
        {
            try
            {
                var configuration = new MapperConfiguration(cfg =>
                    cfg.CreateMap<T, T>());

                var result = await _dbContext.Set<T>()
                    .Where(e => id == e.Id)
                    .ProjectTo<T>(configuration)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"Find Exception {e}", e.ToString());
                throw;
            }
        }

        public virtual async Task<T> FindById(ISpecification<T> specification)
        {
            try
            {
                var result = ApplySpecification(specification, true);
                return await result
                    .FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"FindById Exception {e}", e.ToString());
                throw;
            }
        }

        public virtual async Task<T> Add(T entity)
        {
            try
            {
                var result = await _dbContext.Set<T>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return result.Entity;
            }
            catch (DbUpdateException e)
            {
                _logger.LogError($"(Add) DbUpdate Exception {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"(Add) Exception {e.Message}");
                throw;
            }
        }

        public virtual async Task<bool> Update(Guid id, T entity)
        {
            // Update entity
            // 1. Create instance for DbContext class
            // 2. Retrieve entity by key
            // 3. Make changes on entity's properties
            // 4. Save changes

            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<T, T>());

            var existingEntity = await _dbContext.Set<T>()
                .ProjectTo<T>(configuration)
                .FirstOrDefaultAsync(e => id == e.Id);

            if (existingEntity == null)
            {
                var result = await _dbContext.Set<T>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return true;
            }

            try
            {
                _mapper.Map(entity, existingEntity);
                _dbContext.Entry(existingEntity).State = EntityState.Modified;

                if (IsDevelopment)
                    foreach (var entry in _dbContext.ChangeTracker.Entries())
                        Console.WriteLine($"{entry.Metadata.Name}, {entry.State}");
                
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError($"(Update) DbUpdate Exception {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"(Update) Exception {e.Message}");
                throw;
            }
            return true;
        }

        public virtual async Task<bool> Remove(Guid id)
        {
            var entity = await FindById(id);
            if (entity == null)
                return false;
            var result = _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return result != null;
        }

        public virtual async Task<bool> Remove(T entity)
        {
            _dbContext.Attach(entity);
            var result = _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return result != null;
        }

        public virtual bool Contains(ISpecification<T> specification = null)
        {
            var result = Count(specification);
            return result > 0;
        }

        public bool Contains(Expression<Func<T, bool>> predicate)
        {
            var result = Count(predicate);
            return result > 0;
        }

        public int Count(ISpecification<T> specification = null)
        {
            var result = ApplySpecification(specification, true);
            return result.Count();
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            var result = _dbContext.Set<T>().Where(predicate);
            return result.Count();
        }

        public IQueryable<T> ApplySpecification(ISpecification<T> spec, bool applyPaging)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<T, T>());

            var query = _dbContext.Set<T>()
                .ProjectTo<T>(configuration)
                .AsNoTracking();

            return SpecificationEvaluator<T>.GetQuery(query, spec, applyPaging);
        }
    }
}