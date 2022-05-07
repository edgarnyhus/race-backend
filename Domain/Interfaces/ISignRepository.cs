    using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Models;
using Domain.Queries.Helpers;

namespace Domain.Interfaces
{
    public interface ISignRepository : IRepository<Sign>
    {
        Task<Sign> FindById(string id);
        Task<bool> Update(string id, Sign entity);
        Task<bool> Remove(string id);
        List<KeyValuePair<int, string>> GetSignStates();
    }
}
