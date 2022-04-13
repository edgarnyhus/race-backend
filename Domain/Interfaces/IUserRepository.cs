using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> FindById(string id);
        Task<bool> Update(string id, User user);
        Task<bool> Remove(string id);
        Task<IEnumerable<Role>> GetAllRoles();
        Task<IEnumerable<Role>> GetUserRoles(string? id);
        Task<bool> SetUserRoles(string id, AppMetadata role);
        Task<bool> DeleteUserRoles(string id, AppMetadata role);
        Task<User> GetUserFromDb(string id);
    }
}