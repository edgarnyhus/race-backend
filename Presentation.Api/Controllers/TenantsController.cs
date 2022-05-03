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
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantsController> _logger;
        private readonly AttachmentCreatedDateResolver _resolver;

        public TenantsController(ILogger<TenantsController> logger, ITenantService tenantService,
            AttachmentCreatedDateResolver resolver)
        {
            _logger = logger;
            _tenantService = tenantService;
            _resolver = resolver;
        }

        // GET: api/tenant
        [HttpGet]
        [Authorize("read:tenants")]
        public async Task<IActionResult> GetTenants([FromQuery] QueryParameters queryParameters)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _tenantService.GetTenants(queryParameters);

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
                throw new HttpResponseException((int)HttpStatusCode.Forbidden, error);
            }
        }

        // GET api/tenant/<id>
        [HttpGet("{id}")]
        [Authorize("read:tenants")]
        public async Task<IActionResult> GetTenantById(string id)
        {
            _resolver.SetTimeZone(Request.Headers["TimeZone"]);
            var result = await _tenantService.GetTenantById(id);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }


        // POST: api/tenant/
        [HttpPost]
        [Authorize("create:tenants")]
        public async Task<IActionResult> CreateTenant([FromBody] TenantContract contract)
        {
            try
            {
                _resolver.SetTimeZone(Request.Headers["TimeZone"]);

                var result = await _tenantService.CreateTenant(contract);
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

        // PUT: api/tenant/<id>
        [HttpPut("{id}")]
        [Authorize("update:tenants")]
        public async Task<IActionResult> UpdateTenant(string id, [FromBody] TenantContract contract)
        {
            try
            {
                var result = await _tenantService.UpdateTenant(id, contract);
                if (!result)
                    return NotFound();
                return Ok("Tenant updated");
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                throw new HttpResponseException((int)HttpStatusCode.Conflict, error);
            }
        }

        // DELETE: api/tenant/<id>
        [HttpDelete("{id}")]
        [Authorize("delete:tenants")]
        public async Task<IActionResult> DeleteTenant(string id)
        {
            try
            {
                var result = await _tenantService.DeleteTenant(id);
                return result ? (IActionResult) Ok("Tenant deleted.") : NotFound();
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