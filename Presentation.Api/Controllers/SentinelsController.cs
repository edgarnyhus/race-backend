using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Api.API.Helpers;
using Application.Helpers;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Queries.Helpers;

namespace Api.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class SentinelsController : ControllerBase
    {
        private readonly ISentinelService _service;
        private readonly AttachmentCreatedDateResolver _resolver;

        public SentinelsController(ISentinelService service, AttachmentCreatedDateResolver resolver)
        {
            _service = service;
            _resolver = resolver;
        }


        // GET: api/drivers
        [HttpGet]
        [Authorize("read:drivers")]
        public async Task<IActionResult> GetSentinels([FromQuery] QueryParameters queryParameters)
        {
            throw new NotImplementedException("Function is not yet implemented");

            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetSentinels(queryParameters);

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


        // GET: api/drivers<id>
        [HttpGet("{id}")]
        [Authorize("read:drivers")]
        public async Task<IActionResult> GetSentinelById(string id)
        {
            throw new NotImplementedException("Function is not yet implemented");

            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetSentinelById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();

        }


        // POST: api/drivers
        [HttpPost]
        [Authorize("create:drivers")]
        public async Task<IActionResult> CreateSentinel([FromBody] SentinelContract contract)
        {
            throw new NotImplementedException("Function is not yet implemented");

            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.CreateSentinel(contract);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }


        // PUT: api/drivers
        [HttpPut("{id}")]
        [Authorize("update:drivers")]
        public async Task<IActionResult> UpdateSentinel(string id, SentinelContract contract)
        {
            throw new NotImplementedException("Function is not yet implemented");

            try
            {
                var result = await _service.UpdateSentinel(id, contract);
                if (!result)
                    return NotFound();
                return Ok("Sentinel updated");
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }


        // DELETE: api/drivers
        [HttpDelete("{id}")]
        [Authorize("delete:drivers")]
        public async Task<IActionResult> DeleteSentinel(string id)
        {
            throw new NotImplementedException("Function is not yet implemented");

            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.DeleteSentinel(id);
                return result ? (IActionResult) Ok("Sentinel deleted.") : NotFound();
            }
            catch (Exception ex)
            {∫
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }
    }
}
