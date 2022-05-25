using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Api.API.Helpers;
using Application.Helpers;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Api.API
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SignTypesController : ControllerBase
    {
        private readonly ISignTypeService _service;
        private readonly ILogger<SignTypesController> _logger;
        private AttachmentCreatedDateResolver _resolver;

        public SignTypesController(ISignTypeService service, AttachmentCreatedDateResolver resolver, ILogger<SignTypesController> logger)
        {
            _service = service;
            _resolver = resolver;
            _logger = logger;
        }

        // GET: api/signs
        [HttpGet]
        [Authorize("read:signs")]
        public async Task<IActionResult> GetSignTypes([FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetSignTypes(queryParameters);

            int count = ((IList)result).Count;
            var metadata = new
            {
                count,
                queryParameters.page,
                queryParameters.page_size
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok((IEnumerable<SignTypeDto>)result);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("count")]
        [Authorize("read:signs")]
        public async Task<IActionResult> GetCount([FromQuery] QueryParameters queryParameters)
        {
            var result = await _service.GetCount(queryParameters);
            return Ok(result);
        }

        // GET: api/signs/<id>
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        [Authorize("read:signs")]
        public async Task<IActionResult> GetSignGroupById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetSignTypeById(id);
            return result != null ? (IActionResult)Ok(result) : NotFound();
        }

        // POST: api/signs
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Authorize("create:signs")]
        public async Task<IActionResult> CreateSignType([Microsoft.AspNetCore.Mvc.FromBody] SignTypeContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.CreateSignType(contract);
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
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // PUT: api/signs/<id>
        [Microsoft.AspNetCore.Mvc.HttpPut("{id}")]
        [Authorize("update:signs")]
        public async Task<IActionResult> UpdateSignType(string id, [Microsoft.AspNetCore.Mvc.FromBody] SignTypeContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.UpdateSignType(id, contract);
                return result ? (IActionResult)Ok("Sign Type updated") : NotFound();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // DELETE: api/signs/<id>
        [Microsoft.AspNetCore.Mvc.HttpDelete("{id}")]
        [Authorize("delete:signs")]
        public async Task<IActionResult> DeleteSignGroup(string id)
        {
            try
            {
                var result = await _service.DeleteSignType(id);
                return result ? (IActionResult)Ok("Sign Type deleted") : NotFound();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

    }
}