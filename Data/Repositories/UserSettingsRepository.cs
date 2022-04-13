using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Contracts;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.Helpers;

namespace Infrastructure.Data.Repositories
{
    public class UserSettingsRepository : Repository<UserSettings>, IUserSettingsRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<UserSettingsRepository> _logger;

        public UserSettingsRepository(RaceBackendDbContext dbContext, IMapper mapper,
            ILogger<UserSettingsRepository> logger)
            : base(dbContext, mapper, logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<IEnumerable<UserSettings>> Find(ISpecification<UserSettings> specification)
        {
            var query = ApplySpecification(specification, false)
                .AsNoTracking();

            var result = SpecificationEvaluator<UserSettings>.GetQuery(query, specification, true);
            return await result.ToListAsync();
        }

        public async Task<UserSettings> FindById(string id)
        {
            var eaa = new EmailAddressAttribute();
            var findByEmail = eaa.IsValid(id);
            var idIsGuid = Guid.TryParse(id, out Guid guid);

            User user = null;
            UserSettings userSettings = null;
            
            user = await _dbContext.Set<User>()
                .Include(u => u.UserSettings)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == guid ||  u.Email.Equals(id) || u.UserId.Equals(id));
            userSettings = user?.UserSettings ?? await _dbContext.Set<UserSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(us => us.Id.Equals(guid));

            if (user != null && userSettings == null)
            {
                var newSettingsContract = new UserSettingsContract()
                {
                    UserId = user.Id.ToString()
                };
                var newSetting = _mapper.Map<UserSettingsContract, UserSettings>(newSettingsContract);
                userSettings = await Add(newSetting);
            }

            return userSettings;
        }

        public override async Task<UserSettings> Add(UserSettings entity)
        {
            await PropertyChecks.CheckProperties(_dbContext, entity, null);

            entity = await base.Add(entity);
            return entity;
        }
    }
}
