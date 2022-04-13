using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IUserSettingsRepository : IRepository<UserSettings>
    {
        new Task<IEnumerable<UserSettings>> Find(ISpecification<UserSettings> specification);
        Task<UserSettings> FindById(string id);
    }

 }
