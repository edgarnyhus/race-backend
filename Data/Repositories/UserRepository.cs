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
using System.Text.RegularExpressions;

namespace Infrastructure.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public class Auth0MgtToken
        {

            public string grant_type { get; set; }
            public string client_id { get; set; }
            public string client_secret { get; set; }
            public string audience { get; set; }
        }

        public class Token
        {
            public string access_token { get; set; }
        }

        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRepository> _logger;
        private readonly string _audience;
        private readonly string _api_identifier;
        private string _access_token;

        public UserRepository(LocusBaseDbContext dbContext, IConfiguration configuration, IMapper mapper, ILogger<UserRepository> logger)
            : base(dbContext, mapper, logger)
        {
            _configuration = configuration;
            _logger = logger;
            _audience = _configuration["Auth0_mgt:audience"];
            _api_identifier = _configuration["Auth0_mgt:api_identifier"];
            _access_token = null;
        }

        private async Task<Token> GetAccessToken()
        {
            var clientId = _configuration["Auth0_mgt:clientId"];
            var clientSecret = _configuration["Auth0_mgt:clientSecret"];
            var audience = _configuration["Auth0_mgt:audience"];
            var endpoint = _configuration["Auth0_mgt:endpoint"];
            var client = new RestClient(endpoint);
            var request = new RestRequest(Method.POST);

            var data = new Auth0MgtToken()
            {
                grant_type = "client_credentials",
                client_id = clientId,
                client_secret = clientSecret,
                audience = audience
            };
            var json = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,       // Exclude properties set to null from the payload
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
            request.AddJsonBody(json);
            request.AddHeader("content-type", "application/json");


            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");
            var result = JsonConvert.DeserializeObject<Token>(response.Content);
            _access_token = result.access_token;
            return result;
        }

        public override async Task<IEnumerable<User>> Find(ISpecification<User> specification)
        {
            var accessToken = await GetAccessToken();

            var mgtClient = new RestClient($"{_audience}users");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

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
                if (string.IsNullOrEmpty(user.UserId))
                    user.UserId = user.Identities[0].UserId;
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
            var mgtClient = new RestClient(endpoint);
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");
            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

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

            var usr = await GetUserFromDb(user.UserId);
            user.Id = usr?.Id;
            user.PhoneNumber = usr?.PhoneNumber;
            user.OrganizationId = usr?.OrganizationId;
            user.TenantId = usr?.TenantId;
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

            //Set defualt values
            user.Connection = "Username-Password-Authentication";
            if (user.Name == null)
                user.Name = GetFullNameFromEmail(user.Email);
            if (user.Nickname == null)
                user.Nickname = GetFirstName(user.Name);
            user.Password = user.Nickname + "|1234";

            var appMetadata = new AppMetadata()
            {
                TenantId = tenantId?.ToString().ToLower(),
                OrganizationId = organizationId?.ToString().ToLower(),
            };
            user.AppMetadata = appMetadata;
            
            var entity = new User()
            {
                Id = user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                UserId = user.UserId,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TenantId = tenantId,
                OrganizationId = organizationId,
                //AppMetadata = appMetadata
            };

            // These properties will cause a Payload error if set
            // Setting them to null will exclude them from the Payload (request body)
            user.Id = null;
            user.UserId = null;
            user.PhoneNumber = null;
            user.EmailVerified = null;
            user.OrganizationId = null;
            user.TenantId = null;

            var accessToken = await GetAccessToken();
            var mgtClient = new RestClient($"{_audience}users");
            var request = new RestRequest(Method.POST);

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

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

            var entry = JsonConvert.DeserializeObject<User>(response.Content);

            // Set dafault user role
            string[] roles = { _configuration["Auth0_mgt:defaultUserRole"] };
            appMetadata = new AppMetadata()
            {
                Roles = roles
            };
            await SetUserRoles(entry.UserId, appMetadata);

            // Now add the local User data
            entity.UserId = entry?.UserId;
            entity = await AddUserToDb(entity);

            entry.Id = entity.Id;
            entry.OrganizationId = entity.OrganizationId;
            entry.TenantId = entity.TenantId;
            entry.PhoneNumber = entry.PhoneNumber;

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

            var appMetadata = new AppMetadata()
            {
                TenantId = tenantId?.ToString(),
                OrganizationId = organizationId?.ToString()
            };
            user.AppMetadata = appMetadata;

            var entity = new User()
            {
                Id = usr != null ? usr.Id : user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                UserId = user.UserId,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TenantId = tenantId,
                OrganizationId = organizationId,
                //AppMetadata = appMetadata
            };
            
            // These properties will cause a Payload error if set
            // Setting them to null will exclude them from the Payload (request body)
            user.Id = null;
            user.UserId = null;
            user.PhoneNumber = null;
            user.EmailVerified = null;
            user.OrganizationId = null;
            user.TenantId = null;
            
            var accessToken = await GetAccessToken();
            var mgtClient = new RestClient($"{_audience}users/{id}");
            var request = new RestRequest(Method.PATCH);
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

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

            // Now update the local User data
            await AddUserToDb(entity);

            return response.IsSuccessful;
        }

        public async Task<bool> Remove(string id)
        {
            var usr = await GetUserFromDb(id);
            if (usr != null)
                id = usr.UserId;

            var accessToken = await GetAccessToken();
            var mgtClient = new RestClient($"{_audience}users/{id}");
            var request = new RestRequest(Method.DELETE);

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

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

            // Remove user object from our database
            if (usr != null)
                await base.Remove(usr);

            JsonConvert.DeserializeObject<User>(response.Content);
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        /// 
        /// Misc
        ///

        public string GetFullNameFromEmail(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string n = name.Split('@').First();

            //string[] substrings = Regex.Matches(n, "\\w+('(s|d|t|ve|m))?")
            //    .Cast<Match>().Select(x => x.Value).ToArray();
            var substrings = Regex.Split(n, @"[^a-zA-Z0-9']").Where(s => s != String.Empty).ToArray();

            string s = "";
            foreach (string str in substrings)
            {
                if (!string.IsNullOrEmpty(s))
                    s += " ";
                s += str;
            }
            return s;
        }

        public string GetFirstName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string n = name.Split('@').First();

            //string[] substrings = Regex.Matches(n, "\\w+('(s|d|t|ve|m))?")
            //    .Cast<Match>().Select(x => x.Value).ToArray();
            var substrings = Regex.Split(n, @"[^a-zA-Z0-9']").Where(s => s != String.Empty).ToArray();
            return substrings[0];
        }



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
                string id = user.Id != null ? user.Id.ToString() : user.UserId;
                var usr = await GetUserFromDb(id);
                //if (usr != null)
                //    id = usr.UserId;

                Guid? tenantId = usr != null ? usr.TenantId : user.TenantId;
                Guid? organizationId = usr != null ? usr.OrganizationId : user.OrganizationId;
                if (tenantId == null && Guid.TryParse(user.AppMetadata?.TenantId, out Guid tid))
                    tenantId = tid;
                if (organizationId == null && Guid.TryParse(user.AppMetadata?.OrganizationId, out Guid oid))
                    organizationId = oid;

                // Make a user object in our database - not all properties are mapped
                var entity = new User()
                {
                    Id = usr != null ? usr.Id : user.Id,
                    Name = user.Name,
                    Nickname = user.Nickname,
                    UserId = user.UserId,
                    Email = user.Email,
                    PhoneNumber = usr != null ? usr.PhoneNumber : user.PhoneNumber,
                    TenantId = tenantId,
                    OrganizationId = organizationId,
                };
                await PropertyChecks.CheckPrincipleAndDependant(_dbContext, entity, entity.Organization);

                if (usr == null)
                    entity = await base.Add(entity);
                else
                    _dbContext.Entry(entity).CurrentValues.SetValues(usr);
                await _dbContext.SaveChangesAsync();
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
            var mgtClient = new RestClient($"{_audience}roles");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

            var result = JsonConvert.DeserializeObject<List<Role>>(response.Content); ;
            return result;
        }

        public async Task<IEnumerable<Role>> GetUserRoles(string id)
        {
            var accessToken = await GetAccessToken();
            var mgtClient = new RestClient($"{_audience}users/{id}/roles");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

            var result = JsonConvert.DeserializeObject<List<Role>>(response.Content);
            return result;
        }

        public async Task<bool> SetUserRoles(string id, AppMetadata role)
        {
            var accessToken = _access_token;
            if (accessToken == null)
            {
                var token = await GetAccessToken();
                accessToken = token.access_token;
            }
            var mgtClient = new RestClient($"{_audience}users/{id}/roles");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", $"Bearer {accessToken}");

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

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> DeleteUserRoles(string id, AppMetadata role)
        {
            var accessToken = await GetAccessToken();
            var mgtClient = new RestClient($"{_audience}users/{id}/roles");
            var request = new RestRequest(Method.DELETE);
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

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

            var result = JsonConvert.DeserializeObject<Role>(response.Content);
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<string> UnblockUserById(string id)
        {
            var accessToken = await GetAccessToken();
            //var mgtClient = new RestClient($"{_audience}user-blocks?identifier={id}");
            var mgtClient = new RestClient($"{_audience}user-blocks/{id}");
            var request = new RestRequest(Method.DELETE);
            request.AddHeader("authorization", $"Bearer {accessToken.access_token}");

            var response = await mgtClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new UsersException(response.Content ?? "Unspecified error");

            return response.Content;
        }
    }
}