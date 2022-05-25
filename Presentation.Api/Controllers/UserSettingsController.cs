using System;
using System.Collections;
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
using Domain.Queries.Helpers;

namespace Api.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSettingsController : ControllerBase
    {
        private readonly IUserSettingsService _userSettingsService;
        private readonly ILogger<UserSettingsController> _logger;
        private AttachmentCreatedDateResolver _resolver;

        public UserSettingsController(IUserSettingsService userSettingsService, AttachmentCreatedDateResolver resolver,
            ILogger<UserSettingsController> logger)
        {
            _userSettingsService = userSettingsService;
            _resolver = resolver;
            _logger = logger;
        }

        // GET. api/user_settings
        [HttpGet]
        [Authorize("read:users")]
        public async Task<IActionResult> GetUserSettings([FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _userSettingsService.GetUserSettings(queryParameters);

            int count = ((IList) result).Count;
            var metadata = new
            {
                count,
                queryParameters.page,
                queryParameters.page_size
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(result);
        }

        // GET. api/user_settings/[email]
        // GET. api/user_settings/[user id]
        // GET. api/user_settings/[userSetting id]
        [HttpGet("{id}")]
        [Authorize("read:users")]
        public async Task<IActionResult> GetUserSettingsById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);

            var result = await _userSettingsService.GetUserSettingsById(id);

            return Ok(result);
        }



        // POST. api/user_settings
        [HttpPost]
        [Authorize("update:users")]
        public async Task<IActionResult> CreateUserSettings([FromBody] UserSettingsContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
                var result = await _userSettingsService.CreateUserSettings(contract);
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

        // POST api/user_settings/[user id]
        [HttpPost("{id}")]
        [Authorize("create:users")]
        public async Task<IActionResult> CreateDefaultUserSettings(string id)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
                var result = await _userSettingsService.CreateDefaultUserSettings(id);
                if (result == null)
                    throw new HttpResponseException((int)HttpStatusCode.Forbidden, "User does not exist or User already has UserSettings");
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

        [HttpPut("{id}")]
        [Authorize("update:users")]
        public async Task<IActionResult> UpdateUserSettings(string id, [FromBody] UserSettingsContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _userSettingsService.UpdateUserSettings(id, contract);
                return result ? (IActionResult)Ok("User settings Updated") : NotFound();
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
