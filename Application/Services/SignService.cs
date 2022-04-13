using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Domain.Dtos;
using Domain.Queries.Helpers;
using Application.Helpers;
using Domain.Contracts;
using Domain.Interfaces;
using Domain.Models;
using Domain.Multitenant;
using Domain.Specifications;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class SignService : ISignService
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly ISignRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<SignService> _logger;
        private readonly bool _multitenancy = false;


        public SignService(TenantAccessService<Tenant> tenantAccessService, IRepository<Sign> repository,
            IConfiguration config, IMapper mapper, ILogger<SignService> logger)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (ISignRepository) repository;
            _mapper = mapper;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<SignDto>> GetSigns(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetSignsSpecification(queryParameters));
            _logger.LogTrace($"GetEquipment: Making query {result}");

            var response = _mapper.Map<IEnumerable<Sign>, IEnumerable<SignDto>>(result);
            return response;
        }

        public async Task<SignDto> GetSignById(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.FindById(guid);

            var response = _mapper.Map<Sign, SignDto>(result);
            return response;
        }

        public Task<string> GetSignWithQrCode(string qrCode)
        {
            throw new NotImplementedException();
        }

        public async Task<SignDto> CreateSign(SignContract contract)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException(
                    "Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<SignContract, Sign>(contract);
            entity = await _repository.Add(entity);

            var result = _mapper.Map<Sign, SignDto>(entity);
            return result;
        }

        public async Task<bool> UpdateSign(string id, SignContract contract)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException(
                    "Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<SignContract, Sign>(contract);
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Update(guid, entity);
            return result;
        }

        public async Task<bool> DeleteSign(string id)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException(
                    "Unauthorized. You are missing the necessary permissions to issue this request.");

            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Remove(guid);

            return result;
        }

        public async Task<int> GetCount(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);
            queryParameters.page_size = 0;

            var result = _repository.Count(new GetSignsSpecification(queryParameters));
            return result;
        }
    }
}