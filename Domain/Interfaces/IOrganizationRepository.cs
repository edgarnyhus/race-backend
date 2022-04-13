using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Domain.Models;


namespace Domain.Interfaces
{
    public interface IOrganizationRepository : IRepository<Organization>
    {
        Task<Organization> FindById(string id);
        Task<bool> Update(string id, Organization entity);
        Task<bool> Remove(string id);
    }
}