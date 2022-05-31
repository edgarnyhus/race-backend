using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Api.API.Helpers;
using Application.Helpers;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Exceptions;
using Domain.Models;
using Domain.Queries.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;

namespace Api.API
{

    [ApiController]
    [EnableCors("SiteCorsPolicy")]
    //[Route("api/[controller]")]

    public class UsersController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _service;
        private AttachmentCreatedDateResolver _resolver;

        public UsersController(IHttpContextAccessor httpContextAccessor, IUserService service, AttachmentCreatedDateResolver resolver)
        {
            _httpContextAccessor = httpContextAccessor;
            _service = service;
            _resolver = resolver;
        }

        [HttpGet("api/users")]
        [Authorize("read:users")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers([FromQuery] QueryParameters queryParameters)
        {
            try
            {
                var result = await _service.GetAllUsers(queryParameters);
                int count = ((IList)result).Count;
                var metadata = new
                {
                    count,
                    queryParameters.page,
                    queryParameters.page_size
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

                return Ok(result);
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }

        [HttpGet("api/users/{id}")]
        [Authorize("read:users")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            try
            {
                var result = await _service.GetUserById(id);
                return Ok(result);
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }

        [HttpPost("api/users")]
        [Authorize("create:users")]
        public async Task<ActionResult<UserContract>> CreateUser(UserContract contract)
        {
            try
            {
                var result = await _service.CreateUser(contract);
                return Ok(result);
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error); //?
            }
        }

        [HttpPut("api/users/{id}")]
        [Authorize("update:users")]
        public async Task<ActionResult<UserContract>> UpdateUser(string id, UserContract contract)
        {
            try
            {
                var result = await _service.UpdateUser(id, contract);
                return Ok("User Updated");
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error); //?
            }
        }

        [HttpDelete("api/users/{id}")]
        [Authorize("delete:users")]
        public async Task<ActionResult<bool>> DeleteUser(string id)
        {
            try
            {
                var result = await _service.DeleteUser(id);
                return Ok("User Deleted");
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception e)
            {
                var error = e.Message;
                if (e.InnerException != null)
                    error = e.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        //
        // User's Roles
        //

        [HttpGet("api/users/{id}/roles")]
        [Authorize("read:users")]
        public async Task<ActionResult<List<Role>>> GetUserRoles(string id)
        {
            try
            {
                var result = await _service.GetUserRoles(id);
                return Ok(result);
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }


        [HttpPut("api/users/{id}/roles")]
        [Authorize("update:users")]
        public async Task<ActionResult<bool>> SetUserRoles(string id, AppMetadataDto metadata)
        {
            try
            {
                var result = await _service.SetUserRoles(id, metadata);
                return Ok("User role assigned");
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }

        [HttpDelete("api/users/{id}/roles")]
        [Authorize("delete:users")]
        public async Task<ActionResult<bool>> DeleteUserRoles(string id, AppMetadataDto metadata)
        {
            try
            {
                var result = await _service.DeleteUserRoles(id, metadata);
                return Ok("Role deleted from user");
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }

        //
        // Role's Users
        //

        [HttpGet("api/roles")]
        [Authorize("read:users")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
                var result = await _service.GetAllRoles();
                return Ok(result);
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }

        [HttpGet("api/roles/{id}/users")]
        [Authorize("read:users")]
        public async Task<IActionResult> GetUsersOfRole(string id)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
                var result = await _service.GetUsersOfRole(id);
                return Ok(result);
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }

        [HttpPost("api/roles/{id}/users")]
        [Authorize("update:users")]
        public async Task<IActionResult> AssignUsersToRole(string id, UserList userList)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
                var request = _httpContextAccessor.HttpContext.Request;
                var result = await _service.AssignUsersToRole(id, userList);
                return Ok("Users assigned to role");
            }
            catch (UsersException ex)
            {
                var error = JsonConvert.DeserializeObject(ex.Message);
                return BadRequest(error);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }
    }
}
