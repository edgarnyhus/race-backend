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
    [Route("api/races/")]
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

        // GET: api/races/{id}/waypoints
        [HttpGet("{raceId}/waypoints")]
        [Authorize("read:races")]
        public async Task<IActionResult> GetWaypoints(string raceId, [FromQuery] QueryParameters queryParameters)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            queryParameters.race_id = raceId;
            var result = await _service.GetWaypoints(queryParameters);
            return Ok(result);
        }

        // GET: api/races/waypoints/<id>
        [HttpGet("{raceId}/waypoints/{id}")]
        [Authorize("read:races")]
        public async Task<IActionResult> GetWaypointById(string id)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
        
            var result = await _service.GetWaypointById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/races/{raceId}/waypoints
        [HttpPost("{raceId}/waypoints")]
        [Authorize("create:races")]
        public async Task<IActionResult> CreateWaypoint([FromBody] WaypointContract contract)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
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

        // PUT: api/races/waypoints<id>
        [HttpPut("{raceId}/waypoints/{id}")]
        [Authorize("update:races")]
        public async Task<IActionResult> UpdateWaypoint(string id, WaypointContract contract)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
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

        // DELETE: api/races/waypoints<id>
        [HttpDelete("{raceId}/waypoints/{id}")]
        [Authorize("delete:races")]
        public async Task<IActionResult> DeleterWaypoint(string id)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
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
