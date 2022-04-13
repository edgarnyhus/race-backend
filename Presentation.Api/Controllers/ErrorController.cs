using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Api.API
{

    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        [Route("/error-local-development")]
        private IActionResult ErrorLocalDevelopment(
            [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.EnvironmentName != "Development")
            {
                throw new InvalidOperationException(
                    "This shouldn't be invoked in non-development environments.");
            }

            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            var error = context.Error.Message;
            if (context.Error.InnerException != null)
                error = context.Error.InnerException.Message;
            return Problem(
                detail: context.Error.StackTrace,
                title: error);
        }

        [HttpGet]
        [Route("/error")]
        private IActionResult Error() => Problem();
    }
}