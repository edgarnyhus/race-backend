using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
    public interface IDriverService
    {
        Task<IEnumerable<DriverDto>> GetDrivers(QueryParameters queryParameters);
        Task<DriverDto> GetDriverById(string id);
        Task<DriverDto> CreateDriver(DriverContract contract);
        Task<bool> UpdateDriver(string id, DriverContract contract);
        Task<bool> DeleteDriver(string id);
    }
}