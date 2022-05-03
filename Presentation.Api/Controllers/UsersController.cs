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

namespace Api.API
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private AttachmentCreatedDateResolver _resolver;

        public UsersController(IUserService service, AttachmentCreatedDateResolver resolver)
        {
            _service = service;
            _resolver = resolver;
        }

        [HttpGet]
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

        [HttpGet("{id}")]
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

        [HttpPost]
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
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error); //?
            }
        }

        [HttpPut("{id}")]
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
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error); //?
            }
        }

        [HttpDelete("{id}")]
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
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }



        [HttpGet("{id}/roles")]
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


        [HttpPut("{id}/roles")]
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

        [HttpDelete("{id}/roles")]
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

        [HttpGet("roles")]
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
    }
}
