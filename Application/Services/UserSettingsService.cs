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
    public class UserSettingsService : IUserSettingsService
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly IUserSettingsRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<UserSettingsService> _logger;
        private readonly bool _multitenancy = false;

        public UserSettingsService(TenantAccessService<Tenant> tenantAccessService,
            IRepository<UserSettings> repository, IRepository<User> userRepository, IConfiguration config, IMapper mapper, IMediator mediator,
            ILogger<UserSettingsService> logger)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (IUserSettingsRepository) repository;
            _userRepository = (IUserRepository) userRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<UserSettingsDto>> GetUserSettings(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetUserSettingsSpecification(queryParameters));
            _logger.LogTrace($"GetUserSettings: Making query {result}");
            
            var response = _mapper.Map<IEnumerable<UserSettings>, IEnumerable<UserSettingsDto>>(result);
            return response;
        }

        public async Task<UserSettingsDto> GetUserSettingsById(string id)
        {
            var result = await _repository.FindById(id);
            _logger.LogTrace($"GetUserSettings: Making query {result}");
            
            var response = _mapper.Map<UserSettings, UserSettingsDto>(result);
            
            return response;
        }


        public async Task<UserSettingsDto> CreateUserSettings(UserSettingsContract contract)
        {
            var entity = _mapper.Map<UserSettingsContract, UserSettings>(contract);
            entity = UpdateProperties(entity);

            entity = await _repository.Add(entity);

            var result = _mapper.Map<UserSettings, UserSettingsDto>(entity);
            return result;
        }

        public async Task<UserSettingsDto> CreateDefaultUserSettings(string userId)
        {
            var user = await _userRepository.FindById(userId);
            if (user?.Id != null || user.UserSettings != null)
                return null;

            var contract = new UserSettingsContract()
            {
                UserId = user.Id.ToString()
            };

            var result = await CreateUserSettings(contract);
            return result;
        }

        public async Task<bool> UpdateUserSettings(string id, UserSettingsContract contract)
        {
            try
            {
                var entity = _mapper.Map<UserSettingsContract, UserSettings>(contract);

                var existingEntity = await _repository.FindById(id);
                if (existingEntity.Id.Equals(id))
                {
                    Guid.TryParse(id, out Guid guid);
                    return await _repository.Update(guid, entity);
                }

                return false;
            }

            catch (Exception ex)
            {
                _logger.LogError($"UpdateUserSettings Exception: {ex.Message}");
                throw;
            }
            
        }

        public async void AdminCheck()
        {
            var isAdmin = await _tenantAccessService.IsAdministrator();
            if (_multitenancy && !isAdmin)
                throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");
        }

        public UserSettings UpdateProperties(UserSettings entity)
        {
            entity.Id = GuidExtensions.CheckGuid(entity.Id);
            return entity;
        }

    }
}
