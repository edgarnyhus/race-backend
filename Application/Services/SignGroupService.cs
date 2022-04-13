using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Interfaces;
using Domain.Models;
using Domain.Multitenant;
using Domain.Queries.Helpers;
using Domain.Specifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Application.Services;

public class SignGroupService : ISignGroupService
{
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly ISignGroupRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<SignGroupService> _logger;
        private readonly bool _multitenancy = false;


        public SignGroupService(TenantAccessService<Tenant> tenantAccessService, IRepository<SignGroup> repository,
            IConfiguration config, IMapper mapper, ILogger<SignGroupService> logger)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (ISignGroupRepository) repository;
            _mapper = mapper;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<SignGroupDto>> GetSignGroups(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetSighGroupsSpecification(queryParameters));
            _logger.LogTrace($"GetEquipment: Making query {result}");

            var response = _mapper.Map<IEnumerable<SignGroup>, IEnumerable<SignGroupDto>>(result);
            return response;
        }

        public async Task<SignGroupDto> GetSignGroupById(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.FindById(guid);

            var response = _mapper.Map<SignGroup, SignGroupDto>(result);
            return response;
        }

        public async Task<SignGroupDto> CreateSignGroup(SignGroupContract contract)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException(
                    "Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<SignGroupContract, SignGroup>(contract);
            entity = await _repository.Add(entity);

            var result = _mapper.Map<SignGroup, SignGroupDto>(entity);
            return result;
        }

        public async Task<bool> UpdateSignGroup(string id, SignGroupContract contract)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException(
                    "Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<SignGroupContract, SignGroup>(contract);
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Update(guid, entity);
            return result;
        }

        public async Task<bool> DeleteSignGroup(string id)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException(
                    "Unauthorized. You are missing the necessary permissions to issue this request.");

            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Remove(guid);

            return result;
        }
}