using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Helpers;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Interfaces;
using Domain.Models;
using Domain.Multitenant;
using Domain.Queries.Helpers;
using Domain.Specifications;

namespace Application.Services
{
    public class DriverService : IDriverService
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly IDriverRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<DriverService> _logger;
        private readonly bool _multitenancy = false;

        public DriverService(TenantAccessService<Tenant> tenantAccessService, IRepository<Driver> repository, IMapper mapper,
            ILogger<DriverService> logger, IConfiguration config)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (IDriverRepository)repository;
            _mapper = mapper;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<DriverDto>> GetDrivers(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetDriversSpecification(queryParameters));
            var response = _mapper.Map<IEnumerable<Driver>, IEnumerable<DriverDto>>(result);
            return response;
        }

        public async Task<DriverDto> GetDriverById(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.FindById(guid);
            var response = _mapper.Map<Driver, DriverDto>(result);
            return response;
        }

        public async Task<DriverDto> CreateDriver(DriverContract contract)
        {
            var entity = await UpdateProperties(contract);
            
            var result = await _repository.Add(entity);

            var response = _mapper.Map<Driver, DriverDto>(result);
            return response;
        }

        public async Task<bool> UpdateDriver(string id, DriverContract contract)
        {
            var entity = await UpdateProperties(contract);

            Guid.TryParse(id, out Guid guid);
            var result  = await _repository.Update(guid,entity);
            return result;
        }

        public async Task<bool> DeleteDriver(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Remove(guid);
            return result;
        }


        private async Task<Driver> UpdateProperties(DriverContract contract)
        {
            var entity = _mapper.Map<DriverContract, Driver>(contract);

            Guid? tenantId = entity.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
            {
                var tenant = await _tenantAccessService.GetTenantAsync();
                tenantId = tenant?.TenantId;
                entity.TenantId = tenantId;
            }
            entity.Id = GuidExtensions.CheckGuid(entity.Id);
            return entity;
        }
    }
}