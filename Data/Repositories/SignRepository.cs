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

        public SignRepository(RaceBackendDbContext dbContext, IMapper mapper, ILogger<SignRepository> logger)
            : base(dbContext, mapper, logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<IEnumerable<Sign>> Find(ISpecification<Sign> specification)
        {
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
                result = await query.FirstOrDefaultAsync(x => x.Id == gid);
            else
                result = await query.FirstOrDefaultAsync(x => x.QrCode == id || x.Name == id);

            return result;
        }

        public override async Task<Sign> Add(Sign entity)
        {
            //await PropertyChecks.CheckProperties(_dbContext, entity, null);

            var signType = await _dbContext.Set<SignType>()
                .Include(i => i.Signs)
                .FirstOrDefaultAsync(x => x.Id == entity.SignType.Id);

            if (signType == null)
                throw new ArgumentException("Property 'sign_type.id' must be set to an existing sign_type");

            entity.SignType = null;
            entity = await base.Add(entity);

            if (signType.Signs == null)
                signType.Signs = new Collection<Sign>();
            signType.Signs.Add(entity);

            await _dbContext.SaveChangesAsync();
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
                .Include(u => u.SignType)
                .Include(x => x.Location)
                .FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;

                entity = await Add(entity);
                return entity != null;
            }

            await PropertyChecks.CheckProperties(_dbContext, entity, existingEntity);

            var signType = await _dbContext.Set<SignType>()
                .Include(i => i.Signs)
                .FirstOrDefaultAsync(x => x.Id == existingEntity.SignType.Id);

            if (entity.SignType.Id != existingEntity.SignType.Id)
                signType.Signs.Remove(existingEntity);

            mapper.Map(entity, existingEntity);
            entity.SignType = null;

            if (IsDevelopment)
                foreach (var entry in _dbContext.ChangeTracker.Entries())
                    Console.WriteLine($"{entry.Metadata.Name}, {entry.State}");

            //_dbContext.Set<Sign>().Update(entity);

            if (signType.Signs == null)
                signType.Signs = new Collection<Sign>();
            signType.Signs.Add(entity);

            await _dbContext.SaveChangesAsync();
            return true;
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