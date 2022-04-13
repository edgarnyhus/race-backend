using System;
using System.Collections.Generic;
using System.Linq;
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
    public class RaceService : IRaceService
    {

        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly IRaceRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<Race> _logger;
        private readonly bool _multitenancy = false;

        public RaceService(TenantAccessService<Tenant> tenantAccessService, IRepository<Race> repository, IRepository<User> userRepository, 
            IMapper mapper, IMediator mediator, ILogger<Race> logger, IConfiguration config)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (IRaceRepository)repository;
            _userRepository = (IUserRepository) userRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<RaceDto>> GetAllRoutes(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetRacesSpecification(queryParameters));
            var response = _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDto>>(result);
            return response;
        }

        public async Task<RaceDto> GetRouteById(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.FindById(guid);
            var response = _mapper.Map<Race, RaceDto>(result);
            return response;
        }

        public async Task<RaceDto> CreateRoute(RaceContract contract)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = await UpdateProperties(contract, true);

            var result = await _repository.Add(entity);

            var response = _mapper.Map<Race, RaceDto>(result);
            return response;
        }

 
        public async Task<bool> UpdateRoute(string id, RaceContract contract)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = await UpdateProperties(contract, false);

            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Update(guid, entity);

            return result;
        }


        public async Task<bool> DeleteRoute(string id)
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Remove(guid);

            return result;
        }

        
        public async Task<Race> UpdateProperties(RaceContract contract, bool create)
        {
            var entity = _mapper.Map<RaceContract, Race>(contract);

            if (entity.TenantId == null || entity.TenantId == Guid.Empty)
            {
                var tenant = await _tenantAccessService.GetTenantAsync();
                entity.TenantId = tenant?.TenantId;
            }

            var email = await _tenantAccessService.GetUserEmailAddressAsync();
            if (string.IsNullOrEmpty(entity.CreatedBy))
                entity.CreatedBy = email;

            if (entity.OrganizationId == null)
            {
                var user = await _userRepository.FindById(email);
                entity.Organization = user?.Organization;
            }

            if (create && entity.CreatedAt == null)
                entity.CreatedAt = DateTime.UtcNow;
            else
                entity.UpdatedAt = DateTime.UtcNow;

            return entity;
        }
    }
}
