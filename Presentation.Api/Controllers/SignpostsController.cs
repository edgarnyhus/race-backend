using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Api.API.Helpers;
using Application.Helpers;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Queries.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignpostsController : ControllerBase
    {
        private readonly ISignpostService _service;
        private readonly AttachmentCreatedDateResolver _resolver;

        public SignpostsController(ISignpostService service, AttachmentCreatedDateResolver resolver)
        {
            _service = service;
            _resolver = resolver;
        }

        // GET: api/routes/{id}/signposts
        [HttpGet("{raceId}/signposts")]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetSignposts(string raceId, [FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            queryParameters.race_id = raceId;
            var result = await _service.GetSignposts(queryParameters);
            return Ok(result);
        }

        // GET: api/routes/routes/signposts/<id>
        [HttpGet("signposts/{id}")]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetWaypointById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
        
            var result = await _service.GetSignpostById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/routes/{raceId}/signposts
        [HttpPost("{raceId}/signposts")]
        [Authorize("post:sign")]
        public async Task<IActionResult> CreateWaypoint(string raceId, [FromBody] SignpostContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.CreateSignpost(contract);
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

        // PUT: api/routes/signposts/<id>
        [HttpPut("signposts/{id}")]
        [Authorize("put:sign")]
        public async Task<IActionResult> UpdateWaypoint(string id, SignpostContract contract)
        {
            try
            {
                var result = await _service.UpdateSignpost(id, contract);
                if (!result)
                    return NotFound();
                return Ok("waypoint updated");
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }

        // DELETE: api/routes/signposts/<id>
        [HttpDelete("signposts/{id}")]
        [Authorize("delete:sign")]
        public async Task<IActionResult> DeleterWaypoint(string id)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.DeleteSignpost(id);
                return result ? (IActionResult) Ok("waypoint deleted.") : NotFound();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }


        // GET: api/routes/signposts/states
        [HttpGet("signposts/states")]
        [Authorize("get:sign")]
        public IActionResult GetSignStates()
        {
            var result = _service.GetSignpostStates();
            return (IActionResult)Ok(result);
        }
    }
}
