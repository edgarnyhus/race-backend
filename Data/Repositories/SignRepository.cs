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
using System.Collections.ObjectModel;


namespace Infrastructure.Data.Repositories
{
    public class SignRepository : Repository<Sign>, ISignRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<SignRepository> _logger;

        public SignRepository(LocusBaseDbContext dbContext, IMapper mapper, ILogger<SignRepository> logger)
            : base(dbContext, mapper, logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<IEnumerable<Sign>> Find(ISpecification<Sign> specification)
        {
            var count = _dbContext.Set<Sign>().Count();
            var r = await _dbContext.Set<Sign>()
                .Include(i => i.SignType)
                .AsNoTracking()
                .ToListAsync();

            var query = _dbContext.Set<Sign>()
                .Include(i => i.SignType)
                .AsNoTracking();

            var result = SpecificationEvaluator<Sign>.GetQuery(query, specification, true);
            return await result.ToListAsync();
        }

        public async Task<Sign> FindById(string id)
        {
            var query = _dbContext.Set<Sign>()
                .Include(i => i.SignType)
                .AsNoTracking();

            Sign result = null;
            if (Guid.TryParse(id, out Guid gid))
                result = await query
                    .Where(x => x.Id == gid)
                    .FirstOrDefaultAsync();
            else
                result = await query
                    .Where(x => x.QrCode == id)
                    .FirstOrDefaultAsync();

            return result;
        }

        public override async Task<Sign> Add(Sign entity)
        {
            await PropertyChecks.CheckProperties(_dbContext, entity, null);

            //if (IsDevelopment)
            //    foreach (var ee in _dbContext.ChangeTracker.Entries())
            //        Console.WriteLine($"{ee.Metadata.Name}, {ee.State}");

            entity = await base.Add(entity);
            return entity;
        }

        public async Task<bool> Update(string id, Sign entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Sign, Sign>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
            var mapper = configuration.CreateMapper();

            var query = _dbContext.Set<Sign>()
                .Include(u => u.SignType)
                .Include(x => x.Location);

            Sign existingEntity = null;
            if (Guid.TryParse(id, out Guid guid))
                existingEntity = await query
                    .Where(x => x.Id == guid)
                    .FirstOrDefaultAsync();
            else
                existingEntity = await query
                    .Where(x => x.QrCode == id || (x.Name == id && x.OrganizationId == entity.OrganizationId))
                    .FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;

                entity = await Add(entity);
                return entity != null;
            }


            await PropertyChecks.CheckProperties(_dbContext, entity, existingEntity);
            mapper.Map(entity, existingEntity);

            try
            {
                if (IsDevelopment)
                    foreach (var entry in _dbContext.ChangeTracker.Entries())
                        Console.WriteLine($"{entry.Metadata.Name}, {entry.State}");

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                if (error.Contains("IX_Equipment_QrCode"))
                    Console.WriteLine("IX_Equipment_QrCode");
                else
                    Console.WriteLine($"{error}");
                throw;
            }
        }

        public async Task<bool> Remove(string id)
        {
            var entity = await FindById(id);
            if (entity == null)
                return false;
            var result = _dbContext.Set<Sign>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return result != null;
        }


        public List<KeyValuePair<int, string>> GetSignStates()
        {
            var result = Enum.GetValues(typeof(SignState))
                .Cast<int>()
                .ToDictionary(ee => (int)ee, ee => Enum.GetName(typeof(SignState), ee)).ToList();

            return result;
        }
    }
}