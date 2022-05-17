using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
    public interface IUserSettingsService
    {
        public Task<IEnumerable<UserSettingsDto>> GetUserSettings(QueryParameters queryParameters);
        public Task<UserSettingsDto> GetUserSettingsById(string id);
        public Task<UserSettingsDto> CreateUserSettings(UserSettingsContract contract);
        public Task<UserSettingsDto> CreateDefaultUserSettings(string userId);
        public Task<bool> UpdateUserSettings(string id, UserSettingsContract contract);
    }
}
