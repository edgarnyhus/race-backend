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
    public class WaypointsController : ControllerBase
    {
        private readonly IWaypointService _service;
        private readonly AttachmentCreatedDateResolver _resolver;

        public WaypointsController(IWaypointService service, AttachmentCreatedDateResolver resolver)
        {
            _service = service;
            _resolver = resolver;
        }

        // GET: api/routes/{id}/waypoints
        [HttpGet("{raceId}/waypoints")]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetWaypoints(string raceId, [FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            queryParameters.race_id = raceId;
            var result = await _service.GetWaypoints(queryParameters);
            return Ok(result);
        }

        // GET: api/routes/routes/waypoints/<id>
        [HttpGet("waypoints/{id}")]
        [Authorize("get:sign")]
        public async Task<IActionResult> GetWaypointById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
        
            var result = await _service.GetWaypointById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/routes/{raceId}/waypoints
        [HttpPost("{raceId}/waypoints")]
        [Authorize("post:sign")]
        public async Task<IActionResult> CreateWaypoint(string raceId, [FromBody] WaypointContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.CreateWaypoint(contract);
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

        // PUT: api/routes/waypoints/<id>
        [HttpPut("waypoints/{id}")]
        [Authorize("put:sign")]
        public async Task<IActionResult> UpdateWaypoint(string id, WaypointContract contract)
        {
            try
            {
                var result = await _service.UpdateWaypoint(id, contract);
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

        // DELETE: api/routes/waypoints/<id>
        [HttpDelete("waypoints/{id}")]
        [Authorize("delete:sign")]
        public async Task<IActionResult> DeleterWaypoint(string id)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.DeleteWaypoint(id);
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
    }
}
