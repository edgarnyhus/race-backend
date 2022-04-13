using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Contracts;
using Domain.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Domain.Models;
using Domain.Queries.Helpers;
using Infrastructure.Data.Repositories.Helpers;
using Infrastructure.Data.Context;

namespace Infrastructure.Data.Repositories
{
    public class SignRepository : Repository<Sign>, ISignRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<SignRepository> _logger;

        public SignRepository(RaceBackendDbContext dbContext, IMapper mapper, ILogger<SignRepository> logger)
            : base(dbContext, mapper, logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<IEnumerable<Sign>> Find(ISpecification<Sign> specification)
        {
            var query = _dbContext.Set<Sign>()
                .AsNoTracking();

            var result = SpecificationEvaluator<Sign>.GetQuery(query, specification, true);
            return await result.ToListAsync();
        }

        public override async Task<Sign> Add(Sign entity)
        {
            await PropertyChecks.CheckProperties(_dbContext, entity, null);

            entity = await base.Add(entity);
            return entity;
        }

        public override async Task<bool> Update(Guid id, Sign entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Sign, Sign>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
            var mapper = configuration.CreateMapper();

            var existingEntity = await _dbContext.Set<Sign>()
                .Where(e => id == e.Id)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;

                entity = await Add(entity);
                return entity != null;
            }

            await PropertyChecks.CheckProperties(_dbContext, entity, existingEntity);
            mapper.Map(entity, existingEntity);

            if (IsDevelopment)
                foreach (var entry in _dbContext.ChangeTracker.Entries())
                    Console.WriteLine($"{entry.Metadata.Name}, {entry.State}");

            await _dbContext.SaveChangesAsync();
            return true;
        }


        /// 
        /// SignGroups
        /// 
        public async Task<IEnumerable<SignGroup>> GetSignGroups(ISpecification<SignGroup> specification)
        {
            var query = _dbContext.Set<SignGroup>()
                .AsNoTracking();

            var result = SpecificationEvaluator<SignGroup>.GetQuery(query, specification, true);
            return await result.ToListAsync();
        }

        public async Task<SignGroup> GetSignGroupById(Guid id)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<SignGroup, SignGroup>());

            var result = await _dbContext.Set<SignGroup>()
                .Where(e => id == e.Id)
                .ProjectTo<SignGroup>(configuration)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<SignGroup> CreateSignGroup(SignGroup entity)
        {
            var result = await _dbContext.Set<SignGroup>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<bool> UpdateSignGroup(Guid id, SignGroup entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<SignGroup, SignGroup>());

            var existingEntity = await _dbContext.Set<SignGroup>()
                .ProjectTo<SignGroup>(configuration)
                .FirstOrDefaultAsync(e => id == e.Id);

            if (existingEntity == null)
            {
                var result = await _dbContext.Set<SignGroup>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return true;
            }

            _mapper.Map(entity, existingEntity);
            _dbContext.Entry(existingEntity).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSignGroup(Guid id)
        {
            var entity = await _dbContext.Set<SignGroup>()
                .Where(e => id == e.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (entity == null)
                return false;

            _dbContext.Set<SignGroup>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}