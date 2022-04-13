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
        Task<IEnumerable<SignGroup>> GetSignGroups(ISpecification<SignGroup> specification);
        Task<SignGroup> GetSignGroupById(Guid id);
        Task<SignGroup> CreateSignGroup(SignGroup contract);
        Task<bool> UpdateSignGroup(Guid id, SignGroup contract);
        Task<bool> DeleteSignGroup(Guid id);
    }
}
