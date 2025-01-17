﻿using System;
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

namespace Api.API
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SignsController : ControllerBase
    {
        private readonly ISignService _service;
        private readonly ILogger<SignsController> _logger;
        private AttachmentCreatedDateResolver _resolver;

        public SignsController(ISignService service, AttachmentCreatedDateResolver resolver, ILogger<SignsController> logger)
        {
            _service = service;
            _resolver = resolver;
            _logger = logger;
        }

        // GET: api/signs
        [HttpGet]
        [Authorize("read:signs")]
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
        [Authorize("read:signs")]
        public async Task<IActionResult> GetCount([FromQuery] QueryParameters queryParameters)
        {
            var result = await _service.GetCount(queryParameters);
            return Ok(result);
        }

        // GET: api/signs/<id>
        [HttpGet("{id}")]
        [Authorize("read:signs")]
        public async Task<IActionResult> GetSignById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
            var result = await _service.GetSignById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/signs
        [HttpPost]
        [Authorize("create:signs")]
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
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // PUT: api/signs/<id>
        [HttpPut("{id}")]
        [Authorize("update:signs")]
        public async Task<IActionResult> UpdateSign(string id, [FromBody] SignContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
                var result = await _service.UpdateSign(id, contract);
                return result ? (IActionResult) Ok("Sign updated") : NotFound();
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
        [HttpDelete("{id}")]
        [Authorize("delete:signs")]
        public async Task<IActionResult> DeleteSign(string id)
        {
            try
            {
                var result = await _service.DeleteSign(id);
                return result ? (IActionResult) Ok("Sign deleted") : NotFound();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // GET: api/routes/signs/states
        [HttpGet("states")]
        [Authorize("read:signs")]
        public IActionResult GetSignStates()
        {
            var result = _service.GetSignStates();
            return (IActionResult)Ok(result);
        }
    }
}
