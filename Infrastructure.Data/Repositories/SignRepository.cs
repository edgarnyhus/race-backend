using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Repositories.Helpers;
using Infrastructure.Data.Context;


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
            var query = _dbContext.Set<Sign>()
                .Include(i => i.Location)
                .Include(i => i.SignType)
                .AsNoTracking();

            var result = SpecificationEvaluator<Sign>.GetQuery(query, specification, true);
            return await result.ToListAsync();
        }

        public async Task<Sign> FindById(string id)
        {
            var query = _dbContext.Set<Sign>()
                .Include(i => i.Location)
                .Include(i => i.SignType)
                .AsNoTracking();

            Sign result = null;
            if (Guid.TryParse(id, out Guid gid))
                result = await query
                    .SingleOrDefaultAsync(x => x.Id == gid);
            else
            {
                // QR Code may be on the format '1|UPZYUM70U6VT' where suffix separated by '|' is race day
                var raceDay = 1;
                var qrCode = id;
                var arr = id.Split('|');
                if (arr.Count() > 1)
                {
                    try { raceDay = int.Parse(arr[0]); } catch (Exception) { };
                    qrCode = arr[1];
                }
                result = await query
                    .SingleOrDefaultAsync(x => x.QrCode == qrCode && x.RaceDay == raceDay);
            }

            return result;
        }

        public override async Task<Sign> Add(Sign entity)
        {
            await PropertyChecks.CheckProperties(_dbContext, entity, null);

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
                    .SingleOrDefaultAsync(x => x.Id == guid);
            else
            {
                // QR Code may be on the format '1|UPZYUM70U6VT' where suffix separated by '|' is race day
                var raceDay = entity.RaceDay;
                var qrCode = id;
                var arr = id.Split('|');
                if (arr.Count() > 1)
                {
                    try { raceDay = int.Parse(arr[0]); } catch (Exception) { };
                    if (raceDay != entity.RaceDay)
                        throw new ArgumentException("race_day mismatch between sign.race_day and id (<race_day>|<qr_code>).");
                    qrCode = arr[1];
                }
                existingEntity = await query
                    .SingleOrDefaultAsync(x => x.QrCode == qrCode && x.RaceDay == entity.RaceDay);
            }

            if (existingEntity == null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;

                entity = await Add(entity);
                return entity != null;
            }

            if (existingEntity.RaceDay != entity.RaceDay)
                throw new ArgumentException("Invalid property (race_day).");

            await PropertyChecks.CheckProperties(_dbContext, entity, existingEntity);
            mapper.Map(entity, existingEntity);

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Remove(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return false;

            var entity = await _dbContext.Set<Sign>()
                .Include(i => i.Location)
                .SingleOrDefaultAsync(x => x.Id == guid);

            if (entity == null)
                return false;

            _dbContext.Remove(entity);
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