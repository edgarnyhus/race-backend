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
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class RaceService : IRaceService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly IRaceRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<Race> _logger;
        private readonly bool _multitenancy = false;

        public RaceService(IHttpContextAccessor httpContextAccessor, TenantAccessService<Tenant> tenantAccessService,
            IRepository<Race> repository, IRepository<User> userRepository, 
            IMapper mapper, IMediator mediator, ILogger<Race> logger, IConfiguration config)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantAccessService = tenantAccessService;
            _repository = (IRaceRepository)repository;
            _userRepository = (IUserRepository) userRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<RaceDto>> GetAllRaces(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetRacesSpecification(queryParameters));
            var response = _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDto>>(result);

            return response;
        }

        public async Task<RaceDto> GetRaceById(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.FindById(guid);
            var response = _mapper.Map<Race, RaceDto>(result);

            return response;
        }

        public async Task<RaceDto> CreateRace(RaceContract contract)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = await UpdateProperties(contract);
            if (entity.CreatedAt == null)
                entity.CreatedAt = DateTime.UtcNow;
            else
                entity.UpdatedAt = DateTime.UtcNow;


            var result = await _repository.Add(entity);
            var response = _mapper.Map<Race, RaceDto>(result);

            return response;
        }

 
        public async Task<bool> UpdateRace(string id, RaceContract contract)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = await UpdateProperties(contract);

            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Update(guid, entity);

            return result;
        }


        public async Task<bool> DeleteRace(string id)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Remove(guid);

            return result;
        }


        private async Task<Race> UpdateProperties(RaceContract contract)
        {
            QueryParameters parameters = new QueryParameters();
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(parameters);

            var entity = _mapper.Map<RaceContract, Race>(contract);

            if (entity.TenantId == null)
            {
                if (Guid.TryParse(parameters.tenant_id, out Guid tid))
                    entity.TenantId = tid;
            }
            if (entity.OrganizationId == null)
            {
                if (Guid.TryParse(parameters.organization_id, out Guid oid))
                    entity.OrganizationId = oid;
            }

            if (string.IsNullOrEmpty(entity.CreatedBy))
                entity.CreatedBy = await _tenantAccessService.GetUserEmailAddressAsync();

            return entity;
        }


        //
        // Race Signs
        //

        public async Task<IEnumerable<SignDto>> GetSignsOfRace(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            // Get RaceId from request path
            var path = _httpContextAccessor.HttpContext.Request.Path.Value;
            var arr = path.Split('/');
            queryParameters.race_id = arr[3];

            var result = await _repository.GetSignsOfRace(new GetSignsSpecification(queryParameters));
            var response = _mapper.Map<IEnumerable<Sign>, IEnumerable<SignDto>>(result);

            return response;
        }

        public async Task<bool> AddSignToRace(SignContract contract)
        {
            var entity = _mapper.Map<SignContract, Sign>(contract);
            entity.Location.Timestamp = DateTime.UtcNow;
            if (entity.RaceId == null)
            {
                // Get RaceId from request path - api/races/{race_id}/waypoints
                var path = _httpContextAccessor.HttpContext.Request.Path.Value;
                var arr = path.Split('/');
                entity.RaceId = new Guid(arr[3]);
            }

            var result = await _repository.AddSignToRace(entity);
            return result;
        }

        public async Task<bool> UpdateSign(string id, SignContract contract)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException(
            //        "Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<SignContract, Sign>(contract);

            if (entity.RaceId == null)
            {
                // Get RaceId from request path - api/races/{race_id}/waypoints
                var path = _httpContextAccessor.HttpContext.Request.Path.Value;
                var arr = path.Split('/');
                entity.RaceId = new Guid(arr[3]);
            }

            var result = await _repository.UpdateSign(id, entity);
            return result;
        }

        public async Task<bool> RemoveSignFromRace(string id)
        {
            var result = await _repository.RemoveSignFromRace(id);
            return result;
        }
    }
}
