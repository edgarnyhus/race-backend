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
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _service.GetRaceById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        // POST: api/routes
        [HttpPost]
        [Authorize("create:races")]
        public async Task<IActionResult> CreateRace([FromBody] RaceContract contract)
        {
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
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // PUT: api/races
        [HttpPut("{id}")]
        [Authorize("update:races")]
        public async Task<IActionResult> UpdateRace(string id, RaceContract contract)
        {
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
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // DELETE: api/races
        [HttpDelete("{id}")]
        [Authorize("delete:races")]
        public async Task<IActionResult> DeleteRace(string id)
        {
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
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }



        //
        // {race_id}/signs
        //

        // GET: api/races/{id}/signs
        [HttpGet("{raceId}/signs")]
        [Authorize("read:races")]
        public async Task<IActionResult> GetSignsOfRace(string raceId, [FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            try
            {
                queryParameters.race_id = raceId;
                var result = await _service.GetSignsOfRace(queryParameters);

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
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // POST: api/races/{raceId}/signs
        [HttpPost("{raceId}/signs")]
        [Authorize("create:races")]
        public async Task<IActionResult> AddSignToRace([FromBody] SignContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.AddSignToRace(contract);
                return Ok("Sign added to race");
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // PUT: api/races/{raceId}/signs
        [HttpPut("{raceId}/signs/{id}")]
        [Authorize("create:races")]
        public async Task<IActionResult> UpdateSign(string id, [FromBody] SignContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.UpdateSignInRace(id, contract);
                return Ok("Sign updated");
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.BadRequest, error);
            }
        }

        // DELETE: api/races/signs<id>
        [HttpDelete("{raceId}/signs/{id}")]
        [Authorize("delete:races")]
        public async Task<IActionResult> RemoveSignFromRace(string id)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _service.RemoveSignFromRace(id);
                return result ? (IActionResult)Ok("Sign removed from race") : NotFound();
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
