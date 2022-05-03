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
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<OrganizationsController> _logger;
        private readonly AttachmentCreatedDateResolver _resolver;

        public OrganizationsController(ILogger<OrganizationsController> logger, IOrganizationService organizationService, AttachmentCreatedDateResolver resolver)
        {
            _logger = logger;
            _resolver = resolver;
            _organizationService = organizationService;
        }

        // GET: api/organizations
        [HttpGet]
        [Authorize("read:organizations")]
        public async Task<IActionResult> GetOrganizations([FromQuery] QueryParameters queryParameters)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
            var result = await _organizationService.GetOrganizations(queryParameters);

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

        // GET api/organizations/<id>
        [HttpGet("{id}")]
        [Authorize("read:organizations")]
        public async Task<IActionResult> GetOrganizationById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
            var result = await _organizationService.GetOrganizationById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }

        //POST api/organizations/
        [HttpPost]
        [Authorize("create:organizations")]
        public async Task<IActionResult> CreateOrganization([FromBody] OrganizationContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            
                var result = await _organizationService.CreateOrganization(contract);
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

        // PUT: api/organizations/<id>
        [HttpPut("{id}")]
        [Authorize("update:organizations")]
        public async Task<IActionResult> UpdateOrganization(string id, [FromBody] OrganizationContract contract)
        {
            try
            {
                var result = await _organizationService.UpdateOrganization(id, contract);
                if (!result)
                    return NotFound();
                return Ok("Organization updated");
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }

        // DELETE: api/organizations/<id>
        [HttpDelete("{id}")]
        [Authorize("delete:organizations")]
        public async Task<IActionResult> DeleteOrganization(string id)
        {
            try
            {
                var result = await _organizationService.DeleteOrganization(id);
                return result ? (IActionResult) Ok("Organization deleted.") : NotFound();
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