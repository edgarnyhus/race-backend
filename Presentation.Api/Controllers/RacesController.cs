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
    public class RacesController : ControllerBase
    {
        private readonly IRaceService _service;
        private readonly AttachmentCreatedDateResolver _resolver;

        public RacesController(IRaceService service, AttachmentCreatedDateResolver resolver)
        {
            _service = service;
            _resolver = resolver;
        }

        // GET: api/rputes
        [HttpGet]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetRoutes([FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetAllRoutes(queryParameters);

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

        // GET: api/routes/<id>
        [HttpGet("{id}")]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetRouteById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetRouteById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/routes
        [HttpPost]
        [Authorize("post:sign")]
        public async Task<IActionResult> CreateRoute([FromBody] RaceContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.CreateRoute(contract);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }

        // PUT: api/routes
        [HttpPut("{id}")]
        [Authorize("put:sign")]
        public async Task<IActionResult> UpdateRoute(string id, RaceContract contract)
        {
            try
            {
                var result = await _service.UpdateRoute(id, contract);
                if (!result)
                    return NotFound();
                return Ok("Race updated");
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }

        // DELETE: api/routes
        [HttpDelete("{id}")]
        [Authorize("delete:sign")]
        public async Task<IActionResult> DeleteRoute(string id)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.DeleteRoute(id);
                return result ? (IActionResult) Ok("Race deleted.") : NotFound();
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
