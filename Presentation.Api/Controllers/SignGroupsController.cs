using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Api.API;
using Api.API.Helpers;
using Application.Helpers;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignGroupsController : ControllerBase
    {
        private readonly ISignGroupService _service;
        private readonly ILogger<SignGroupsController> _logger;
        private AttachmentCreatedDateResolver _resolver;

        public SignGroupsController(ISignGroupService signGroupService, AttachmentCreatedDateResolver resolver, ILogger<SignGroupsController> logger)
        {
            _service = signGroupService;
            _resolver = resolver;
            _logger = logger;
        }

        // GET: api/signs
        [HttpGet]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetContainers([FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetSignGroups(queryParameters);
            
            int count = ((IList)result).Count;
            var metadata = new
            {
                count,
                queryParameters.page,
                queryParameters.page_size
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok((IEnumerable<SignDto>)result);
        }

        // GET: api/signs/<id>
        [HttpGet("{id}")]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetSignGroupById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
            var result = await _service.GetSignGroupById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/signs
        [HttpPost]
        [Authorize("post:sign")]
        public async Task<IActionResult> CreateSignGroup([FromBody] SignGroupContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
                var result = await _service.CreateSignGroup(contract);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // return Problem(
                //     detail: ex.StackTrace,
                //     title: ex.Message);
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }

        // PUT: api/signs/<id>
        [HttpPut("{id}")]
        [Authorize("put:sign")]
        public async Task<IActionResult> UpdateSign(string id, [FromBody] SignGroupContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
                var result = await _service.UpdateSignGroup(id, contract);
                return result ? (IActionResult) Ok("Container updated") : NotFound();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }

        // DELETE: api/signs/<id>
        [HttpDelete("{id}")]
        [Authorize("delete:sign")]
        public async Task<IActionResult> DeleteContainer(string id)
        {
            try
            {
                var result = await _service.DeleteSignGroup(id);
                return result ? (IActionResult) Ok("Container deleted") : NotFound();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }
    }
}
