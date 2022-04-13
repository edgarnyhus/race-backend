using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Api.API.Helpers;
using Application.Helpers;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;
using RestSharp;

namespace Api.API
{
    [ApiController]
    [Route("api/equipment/[controller]")]
    //[Authorize]
    public class SignsController : ControllerBase
    {
        private readonly ISignService _service;
        private readonly ILogger<SignsController> _logger;
        private AttachmentCreatedDateResolver _resolver;

        public SignsController(ISignService containerService, AttachmentCreatedDateResolver resolver, ILogger<SignsController> logger)
        {
            _service = containerService;
            _resolver = resolver;
            _logger = logger;
        }

        // GET: api/signs
        [HttpGet]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetSigns([FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetSigns(queryParameters);
            
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

        [HttpGet("count")]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetCount([FromQuery] QueryParameters queryParameters)
        {
            var result = await _service.GetCount(queryParameters);
            return Ok(result);
        }

        // GET: api/signs/<id>
        [HttpGet("{id}")]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetSignById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
            var result = await _service.GetSignById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/signs
        [HttpPost]
        [Authorize("post:sign")]
        public async Task<IActionResult> CreateSign([FromBody] SignContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
                var result = await _service.CreateSign(contract);
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
        public async Task<IActionResult> UpdateSign(string id, [FromBody] SignContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
                var result = await _service.UpdateSign(id, contract);
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
        public async Task<IActionResult> DeleteSign(string id)
        {
            try
            {
                var result = await _service.DeleteSign(id);
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
