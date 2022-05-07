using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Dtos;
using Domain.Models;
using Domain.Queries.Helpers;
using Domain.Specifications;

namespace Domain.Interfaces
{
    public interface IRaceRepository : IRepository<Race>
    {
        Task<IEnumerable<Sign>> GetSignsOfRace(GetSignsSpecification specification);
        Task<bool> AddSignToRace(Sign entity);
        Task<bool> UpdateSign(string id, Sign entity);
        Task<bool> RemoveSignFromRace(string id);
    }
}