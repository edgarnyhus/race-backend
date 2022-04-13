using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Models;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsers(QueryParameters queryParameters);
        Task<UserDto> GetUserById(string id);
        Task<UserDto> CreateUser(UserContract contract);
        Task<bool> UpdateUser(string id, UserContract contract);
        Task<bool> DeleteUser(string id);

        // Roles : 

        Task<IEnumerable<RoleDto>> GetAllRoles();
        Task<IEnumerable<RoleDto>> GetUserRoles(string userId);
        Task<bool> SetUserRoles(string id, AppMetadataDto roles);
        Task<bool> DeleteUserRoles(string id, AppMetadataDto roles);
    }
}