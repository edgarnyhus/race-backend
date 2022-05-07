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
    [Route("api/races")]
    public class RacesController : ControllerBase
    {
        private readonly IRaceService _service;
        private readonly AttachmentCreatedDateResolver _resolver;

        public RacesController(IRaceService service, AttachmentCreatedDateResolver resolver)
        {
            _service = service;
            _resolver = resolver;
        }

        // GET: api/races
        [HttpGet]
        [Authorize("read:races")]
        public async Task<IActionResult> GetRaces([FromQuery] QueryParameters queryParameters)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetAllRaces(queryParameters);

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

        // GET: api/races/<id>
        [HttpGet("{id}")]
        [Authorize("read:races")]
        public async Task<IActionResult> GetRaceById(string id)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetRaceById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/routes
        [HttpPost]
        [Authorize("create:races")]
        public async Task<IActionResult> CreateRace([FromBody] RaceContract contract)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.CreateRace(contract);
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

        // PUT: api/races
        [HttpPut("{id}")]
        [Authorize("update:races")]
        public async Task<IActionResult> UpdateRace(string id, RaceContract contract)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            try
            {
                var result = await _service.UpdateRace(id, contract);
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

        // DELETE: api/races
        [HttpDelete("{id}")]
        [Authorize("delete:races")]
        public async Task<IActionResult> DeleteRace(string id)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.DeleteRace(id);
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



        //
        // {race_id}/signs
        //

        // GET: api/races/{id}/waypoints
        [HttpGet("{raceId}/signs")]
        [Authorize("read:races")]
        public async Task<IActionResult> GetSignsOfRace(string raceId, [FromQuery] QueryParameters queryParameters)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            queryParameters.race_id = raceId;
            var result = await _service.GetSignsOfRace(queryParameters);
            return Ok(result);
        }

        // POST: api/races/{raceId}/waypoints
        [HttpPost("{raceId}/signs")]
        [Authorize("create:races")]
        public async Task<IActionResult> AddSignToRace([FromBody] SignContract contract)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.AddSignToRace(contract);
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

        // DELETE: api/races/waypoints<id>
        [HttpDelete("{raceId}/signs/{id}")]
        [Authorize("delete:races")]
        public async Task<IActionResult> RemoveSignFromRace(string id)
        {
            //throw new HttpResponseException((int)HttpStatusCode.NotImplemented, "Function is not yet implemented");
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.RemoveSignFromRace(id);
                return result ? (IActionResult)Ok("Sign removed") : NotFound();
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
