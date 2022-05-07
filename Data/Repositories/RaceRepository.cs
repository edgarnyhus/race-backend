using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.Helpers;
using Domain.Specifications;
using System.Collections.ObjectModel;
using Domain.Contracts;

namespace Infrastructure.Data.Repositories
{
    public class RaceRepository : Repository<Race>, IRaceRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<RaceRepository> _logger;

        public RaceRepository(LocusBaseDbContext dbContext, IMapper mapper, ILogger<RaceRepository> logger) : base(dbContext, mapper, logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<Race> Add(Race entity)
        {
            try
            {
                //await PropertyChecks.CheckPrincipleAndDependant(_dbContext, entity, entity.Organization);
                var result = await _dbContext.Set<Race>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception e)
            {
                var error = e.Message;
                if (e.InnerException != null)
                    error = e.InnerException.Message;
                _logger.LogError($"Add Race Exception: {error}");
                throw;
            }
        }

        public override async Task<bool> Update(Guid id, Race entity)
        {
            var existingEntity = await _dbContext.Set<Race>()
                .Where(e => id == e.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                var assignment = await Add(entity);
                return assignment != null;
            }
            
            //await PropertyChecks.CheckPrincipleAndDependant(_dbContext, entity, entity.Organization);
            _mapper.Map(entity, existingEntity);

            if (IsDevelopment)
                foreach (var entry in _dbContext.ChangeTracker.Entries())
                    Console.WriteLine($"{entry.Metadata.Name}, {entry.State}");

            _dbContext.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public override async Task<bool> Remove(Guid id)
        {
            try
            {
                var result = await Task.Run(() => base.Remove(id));
                return result;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                _logger.LogError($"Remove Exception: {error}");
                throw new Exception(error);
            }
        }

        //
        // Signs of Race
        // 

        public async Task<IEnumerable<Sign>> GetSignsOfRace(GetSignsSpecification specification)
        {
            Guid.TryParse(specification.Parameters.race_id, out Guid guid);
            var result = _dbContext.Set<Sign>()
                .Where(x => x.RaceId == guid)
                .Include(i => i.SignType)
                .Include(i => i.Location)
                .AsNoTracking()
                .AsEnumerable();

            return await Task.Run(() => result.ToList());
        }

        public async Task<bool> AddSignToRace(Sign entity)
        {
            var sign = await _dbContext.Signs
                .Where(x => x.Id == entity.Id)
                .FirstOrDefaultAsync();

            if (sign == null)
                throw new ArgumentException("Invalid request. A sign with the given Id does not exists!");
            if (entity.RaceId == null)
                throw new ArgumentException("Invalid request. A race withh the specified race_id does not exists!");

            await PropertyChecks.CheckProperties(_dbContext, entity, sign);
            sign.RaceId = entity.RaceId;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSign(string id, Sign entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Sign, Sign>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
            var mapper = configuration.CreateMapper();

            Guid.TryParse(id, out Guid guid);
            var existingEntity = await _dbContext.Set<Sign>()
                .Include(x => x.Location)
                .Where(x => x.Id == guid)
                .FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;

                var sign = await AddSignToRace(entity);
                return sign != null;
            }

            await PropertyChecks.CheckProperties(_dbContext, entity, existingEntity);
            mapper.Map(entity, existingEntity);

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSignFromRace(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var sign = await _dbContext.Signs
                .Where(x => x.Id == guid)
                .FirstOrDefaultAsync();

            sign.RaceId = null;

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
