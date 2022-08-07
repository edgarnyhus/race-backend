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
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System;

namespace Application.Services
{
    public class UserService : IUserService
    {

        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<UserService> _logger;
        private readonly bool _multitenancy = false;
        private readonly IConfiguration _config;


        public UserService(TenantAccessService<Tenant> tenantAccessService, IRepository<User> repository, IMapper mapper,
            IMediator mediator, ILogger<UserService> logger, IConfiguration config)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (IUserRepository)repository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            _config = config;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }


        public async Task<IEnumerable<UserDto>> GetAllUsers(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetUsersSpecification(queryParameters));
            var response = _mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(result);
            return response;
        }

        public async Task<UserDto> GetUserById(string id)
        {
            var result = await _repository.FindById(id);
            var response = _mapper.Map<User, UserDto>(result);
            return response;
        }

        public async Task<UserDto> CreateUser(UserContract contract)
        {
            var entity = await UpdateProperties(contract);
            var ma = new MailAddress(entity.Email);
            if (ma.Host == "gmail.com" || ma.Host == "hotmail.com" || ma.Host == "outlook.com")
                throw new ArgumentException("Invalid email address. Choose an email address with a doamin name that matches a domain in the user's Organization.Identifier list.");

            var result = await _repository.Add(entity);
            var response = _mapper.Map<User, UserDto>(result);
            return response;
        }

        public async Task<bool> UpdateUser(string id, UserContract contract)
        {
            var entity = await UpdateProperties(contract);
            var result = await _repository.Update(id, entity);
            return result;
        }

        public async Task<bool> DeleteUser(string id)
        {
            var result = await _repository.Remove(id);
            return result;
        }


        //Misc


        public async Task<User> UpdateProperties(UserContract contract)
        {
            var entity = _mapper.Map<UserContract, User>(contract);
            if (entity.AppMetadata == null)
                entity.AppMetadata = new AppMetadata();
            if (entity.TenantId != null)
                entity.AppMetadata.TenantId = entity.TenantId.ToString();
            if (entity.OrganizationId != null)
                entity.AppMetadata.OrganizationId = entity.OrganizationId.ToString();
            if (string.IsNullOrEmpty(entity.AppMetadata.TenantId))
            {
                var tenant = await _tenantAccessService.GetTenantAsync();
                entity.AppMetadata.TenantId = tenant?.TenantId?.ToString();
                if (string.IsNullOrEmpty(entity.AppMetadata.OrganizationId))
                    entity.AppMetadata.OrganizationId = tenant?.OrganizationId?.ToString();
            }
            entity.AppMetadata.TenantId?.ToUpper();
            entity.AppMetadata.OrganizationId?.ToUpper();
            return entity;
        }


        //Roles


        public async Task<IEnumerable<RoleDto>> GetAllRoles()
        {
            var isGlobalAdmin = await _tenantAccessService.IsGlobalAdministrator();
            var result = await _repository.GetAllRoles();
            if (!isGlobalAdmin)
               result = result.Where(c => c.Name != Constants.TenantGlobalAdminRole).ToList();
            var response = _mapper.Map<IEnumerable<Role>, IEnumerable<RoleDto>>(result);
            return response;
        }

        public async Task<IEnumerable<UserDto>> GetUsersOfRole(string id)
        {
            var result = await _repository.GetUsersOfRole(id);
            var response = _mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(result);
            return response;
        }

        public async Task<bool> AssignUsersToRole(string roleId, UserList userList)
        {
            var result = await _repository.AssignUsersToRole(roleId, userList);
            return result;
        }



        public async Task<IEnumerable<RoleDto>> GetUserRoles(string id)
        {
            var result = await _repository.GetUserRoles(id);
            var response = _mapper.Map<IEnumerable<Role>, IEnumerable<RoleDto>>(result);
            return response;
        }
       
        public async Task<bool> SetUserRoles(string id, AppMetadataDto metadata)
        {
            await CheckRoles(metadata);

            var data = _mapper.Map<AppMetadataDto, AppMetadata> (metadata);
            var result = await _repository.SetUserRoles(id, data);
            return result;

        }

        public async Task<bool> DeleteUserRoles(string id, AppMetadataDto metadata)
        {
            await CheckRoles(metadata);

            var data = _mapper.Map<AppMetadataDto, AppMetadata>(metadata);
            var result = await _repository.DeleteUserRoles(id, data);
            return result;
        }


        private async Task CheckRoles(AppMetadataDto metadata) {
            if (!await _tenantAccessService.IsGlobalAdministrator())
            {
                // if role = admin, throw exception
                var roleAdmin = _config["Auth0_mgt:RoleAdminName"];
                var roles = await _repository.GetAllRoles();
                foreach (var role in roles)
                {
                    if (role.Name.Equals(roleAdmin, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (metadata.Roles.FirstOrDefault(x => x.Equals(role.Id, StringComparison.InvariantCultureIgnoreCase)) != null)
                            throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");
                    }
                }
            }
        }
    }
}