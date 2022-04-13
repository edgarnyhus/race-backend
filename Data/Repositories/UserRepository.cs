using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.Helpers;
using RestSharp;

namespace Infrastructure.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public class Token
        {
            public string access_token { get; set; }
        }

        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRepository> _logger;
        private readonly string _audience;

        public UserRepository(RaceBackendDbContext dbContext, IConfiguration configuration, IMapper mapper, ILogger<UserRepository> logger)
            : base(dbContext, mapper, logger)
        {
            _configuration = configuration;
            _logger = logger;
            _audience = _configuration["Auth0_mgt:audience"];
        }

        private async Task<Token> GetAccessToken()
        {
            var url = _configuration["Auth0_mgt:url"];
            var clientId = _configuration["Auth0_mgt:clientId"];
            var clientSecret = _configuration["Auth0_mgt:clientSecret"];
            var audience = _configuration["Auth0_mgt:audience"];
            var client = new RestClient($"{url}");
            var request = new RestRequest
            {
                Method = Method.Post
            };
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter($"application/x-www-form-urlencoded", $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}&audience={audience}", ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);
            
            var result = JsonConvert.DeserializeObject<Token>(response.Content);
            return result;
        }


        public override async Task<IEnumerable<User>> Find(ISpecification<User> specification)
        {
            //RestRequest request;
            //IRestResponse response;
            var accessToken = await GetAccessToken();

            var mgtClient = new RestClient($"{_audience}users");
            var request = new RestRequest
            {
                Method = Method.Get
            };
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content);

            var users = JsonConvert.DeserializeObject<List<User>>(response.Content,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            foreach (var user in users)
            {
                if (user.AppMetadata != null)
                {
                    if (!string.IsNullOrEmpty(user.AppMetadata.TenantId))
                        if (Guid.TryParse(user.AppMetadata.TenantId, out Guid tid))
                            user.TenantId = tid;
                    if (!string.IsNullOrEmpty(user.AppMetadata.OrganizationId))
                        if (Guid.TryParse(user.AppMetadata.OrganizationId, out Guid oid))
                            user.OrganizationId = oid;
                }
                var usr = await AddUserToDb(user);
                user.Id = usr?.Id;
                user.TenantId = usr?.TenantId;
                user.OrganizationId = usr?.OrganizationId;
                user.PhoneNumber = usr?.PhoneNumber;
            }   

            var query = users.AsQueryable();
            var result = SpecificationEvaluator<User>.GetQuery(query, specification, true);
            return result;
        }

        public async Task<User> FindById(string id)
        {
            var eaa = new EmailAddressAttribute();
            var findByEmail = eaa.IsValid(id);

            string endpoint;
            if (findByEmail)
                endpoint = $"{_audience}users-by-email?email={id.ToLower()}";
            else
                endpoint = $"{_audience}users/{id}";

            //var usr = await GetUserFromDb(id);
            //if (usr != null)
            //    id = usr.UserId;

            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            var mgtClient = new RestClient(endpoint);
            request = new RestRequest
            {
                Method = Method.Get
            };
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            User user;
            if (findByEmail)
            {
                var users = JsonConvert.DeserializeObject<List<User>>(response.Content);
                if (users.Count == 0)
                    return null;
                user = users.FirstOrDefault();
            }
            else
                user = JsonConvert.DeserializeObject<User>(response.Content);

            var usr = await AddUserToDb(user);
            user.Id = usr?.Id;
            user.PhoneNumber = usr?.PhoneNumber;
            user.Organization = usr?.Organization;
            return user;
        }

        public override async Task<User> Add(User user)
        {
            // Make a user object in our database - not all properties are mapped
            Guid? tenantId = user.TenantId;
            Guid? organizationId = user.OrganizationId;
            if (tenantId == null && Guid.TryParse(user.AppMetadata?.TenantId, out Guid tid))
                tenantId = tid;
            if (organizationId == null && Guid.TryParse(user.AppMetadata?.OrganizationId, out Guid oid))
                organizationId = oid;

            var appMetadata = new AppMetadata()
            {
                TenantId = tenantId?.ToString(),
                OrganizationId = organizationId?.ToString()
            };
            
            var entity = new User()
            {
                Id = user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                UserId = user.UserId,
                Email = user.Email,
                TenantId =tenantId,
                AppMetadata = appMetadata
            };

            // These properties will cause a Payload error if set
            // Setting them to null will exclude them from the Payload (request body)
            user.Id = null;
            user.UserId = null;
            user.PhoneNumber = null;
            user.EmailVerified = null;

            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            var mgtClient = new RestClient($"{_audience}users");
            request = new RestRequest
            {
                Method = Method.Post
            };

            var json = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,       // Exclude properties set to null from the payload
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
            request.AddJsonBody(json);

            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            var entry = JsonConvert.DeserializeObject<User>(response.Content);

            // Now add the local User data
            await PropertyChecks.CheckPrincipleAndDependant(_dbContext, entity, entity.Organization);
            entity.UserId = entry?.UserId;
            entity.TenantId = Guid.Parse(entry.AppMetadata.TenantId);
            entity = await base.Add(entity);
            
            entry.Id = entity.Id;
            entry.Organization = entity.Organization;

            //UpdateItxEntity(entity);
            return entry;
        }

        public async Task<bool> Update(string id, User user)
        {
            var usr = await GetUserFromDb(id);
            if (usr != null)
                id = usr.UserId;

            Guid? tenantId = user.TenantId;
            Guid? organizationId = user.OrganizationId;
            if (tenantId == null && Guid.TryParse(user.AppMetadata?.TenantId, out Guid tid))
                tenantId = tid;
            if (organizationId == null && Guid.TryParse(user.AppMetadata?.OrganizationId, out Guid oid))
                organizationId = oid;

            // Make a user object in our database - not all properties are mapped
            var appMetadata = new AppMetadata()
            {
                TenantId = tenantId?.ToString(),
                OrganizationId = organizationId?.ToString()
            };
            var entity = new User()
            {
                Id = usr != null ? usr.Id : user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                UserId = id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TenantId = tenantId,
                OrganizationId = organizationId,
                AppMetadata = appMetadata
            };
            
            // These properties will cause a Payload error if set
            // Setting them to null will exclude them from the Payload (request body)
            user.Id = null;
            user.UserId = null;
            user.PhoneNumber = null;
            user.EmailVerified = null;
            user.OrganizationId = null;
            
            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            var mgtClient = new RestClient($"{_audience}users/{id}");
            request = new RestRequest
            {
                Method = Method.Patch
            };
            var json = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,       // Exclude properties set to null from the payload
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
            request.AddJsonBody(json);
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            // Now update the local User data
            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();

            return response.IsSuccessful;
        }

        public async Task<bool> Remove(string id)
        {
            var usr = await GetUserFromDb(id);
            if (usr != null)
                id = usr.UserId;

            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            var mgtClient = new RestClient($"{_audience}users/{id}");
            request = new RestRequest
            {
                Method = Method.Delete
            };
            var json = JsonConvert.SerializeObject(Formatting.None, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
            request.AddJsonBody(json);
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            // Remove user object from our database
            if (usr != null)
                await base.Remove(usr);

            JsonConvert.DeserializeObject<User>(response.Content);
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        /// 
        /// Misc
        /// 

        public async Task<User> GetUserFromDb(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            Guid guid;
            bool isGuid = Guid.TryParse(id, out guid);
            var eat = new EmailAddressAttribute();
            var isEmail = eat.IsValid(id);

            User usr = null;
            var query = _dbContext.Users
                .Include(c => c.Organization)
                .AsNoTracking();

            if (isGuid)
                usr = await query.FirstOrDefaultAsync(x => x.Id == guid);
            else if (isEmail)
                usr = await query.FirstOrDefaultAsync(x => x.Email == id);
            else
                usr = await query.FirstOrDefaultAsync(x => x.UserId == id);

            return usr;
        }


        private async Task<User> AddUserToDb(User user)
        {
            try
            {
                //var configuration = new MapperConfiguration(cfg =>
                //    cfg.CreateMap<User, User>()
                //    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
                //var mapper = configuration.CreateMapper();

                //var entity = await _dbContext.Set<User>()
                //    .Include(i => i.Organization)
                //    .AsNoTracking()
                //    .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                //Guid? tenantId = user.TenantId;
                //Guid? organizationId = user.OrganizationId;
                //if (tenantId == null && Guid.TryParse(user.AppMetadata?.TenantId, out Guid tid))
                //    tenantId = tid;
                //if (tenantId == null && entity != null)
                //    tenantId = entity.TenantId;
                //if (organizationId == null && Guid.TryParse(user.AppMetadata?.OrganizationId, out Guid oid))
                //    organizationId = oid;
                //if (organizationId == null && entity != null)
                //    organizationId = entity.OrganizationId;

                //var usr = new User()
                //{
                //    Id = entity?.Id,
                //    Name = user.Name,
                //    Nickname = user.Nickname,
                //    Username = user.Username,
                //    UserId = user.UserId,
                //    Email = user.Email,
                //    PhoneNumber = user.PhoneNumber,
                //    TenantId = tenantId,
                //    OrganizationId = organizationId
                //};

                //if (entity == null)
                //{
                //    //entity = await base.Add(usr);
                //    await PropertyChecks.CheckProperties(_dbContext, usr, null);
                //    var result = await _dbContext.AddAsync(usr);
                //    await _dbContext.SaveChangesAsync();
                //    entity = result.Entity;
                //}
                //else
                //{
                //    if (usr.Nickname == "en")
                //        Console.WriteLine("debug");
                //    await PropertyChecks.CheckProperties(_dbContext, usr, entity);
                //    mapper.Map(usr, entity);
                //    //_dbContext.Update(entity);
                //    _dbContext.Entry(entity).CurrentValues.SetValues(usr);
                //    await _dbContext.SaveChangesAsync();
                //}

                //return entity;

                string id = user.Id != null ? user.Id.ToString() : user.UserId.ToString();
                var usr = await GetUserFromDb(id);
                if (usr != null)
                    id = usr.UserId;

                Guid? tenantId = usr != null ? usr.TenantId : user.TenantId;
                Guid? organizationId = usr != null ? usr.OrganizationId : user.OrganizationId;
                if (tenantId == null && Guid.TryParse(user.AppMetadata?.TenantId, out Guid tid))
                    tenantId = tid;
                if (organizationId == null && Guid.TryParse(user.AppMetadata?.OrganizationId, out Guid oid))
                    organizationId = oid;

                // Make a user object in our database - not all properties are mapped
                var appMetadata = new AppMetadata()
                {
                    TenantId = tenantId?.ToString(),
                    OrganizationId = organizationId?.ToString()
                };
                var entity = new User()
                {
                    Id = usr != null ? usr.Id : user.Id,
                    Name = user.Name,
                    Nickname = user.Nickname,
                    UserId = id,
                    Email = user.Email,
                    PhoneNumber = usr != null ? usr.PhoneNumber : user.PhoneNumber,
                    TenantId = tenantId,
                    OrganizationId = organizationId,
                    AppMetadata = appMetadata
                };
                await PropertyChecks.CheckPrincipleAndDependant(_dbContext, entity, entity.Organization);

                if (usr == null)
                {
                    entity = await base.Add(entity);
                }
                //else
                //{
                //    _dbContext.Entry(entity).CurrentValues.SetValues(usr);
                //    await _dbContext.SaveChangesAsync();
                //}
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddUserToDb Exception: {ex.Message}");
            }
            return null;
        }


        ///
        /// Roles
        /// 

        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            var mgtClient = new RestClient($"{_audience}roles");
            request = new RestRequest
            {
                Method = Method.Get
            };
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            var result = JsonConvert.DeserializeObject<List<Role>>(response.Content); ;
            return result;
        }
        public async Task<IEnumerable<Role>> GetUserRoles(string id)
        {
            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            var mgtClient = new RestClient($"{_audience}users/{id}/roles");
            request = new RestRequest
            {
                Method = Method.Get
            };
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            var result = JsonConvert.DeserializeObject<List<Role>>(response.Content);
            return result;
        }
        public async Task<bool> SetUserRoles(string id, AppMetadata role)
        {
            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            var mgtClient = new RestClient($"{_audience}users/{id}/roles");
            request = new RestRequest
            {
                Method = Method.Post
            };
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            var json = JsonConvert.SerializeObject(role, Formatting.None, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
            request.AddJsonBody(json);
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> DeleteUserRoles(string id, AppMetadata role)
        {
            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            var mgtClient = new RestClient($"{_audience}users/{id}/roles");
            request = new RestRequest
            {
                Method = Method.Delete
            };
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            var json = JsonConvert.SerializeObject(role, Formatting.None, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
            request.AddJsonBody(json);
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            var result = JsonConvert.DeserializeObject<Role>(response.Content);
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<string> UnblockUserById(string id)
        {
            var accessToken = await GetAccessToken();
            RestRequest request;
            RestResponse response;
            //var mgtClient = new RestClient($"{_audience}user-blocks?identifier={id}");
            var mgtClient = new RestClient($"{_audience}user-blocks/{id}");
            request = new RestRequest
            {
                Method = Method.Delete
            };
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                if (response.Content != null)
                    throw new UsersException(response.Content);

            return response.Content;
        }
    }
}