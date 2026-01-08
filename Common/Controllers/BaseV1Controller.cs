using Microsoft.AspNetCore.Mvc;
using System.Net;
using BE.Common.Models;
using Asp.Versioning;

namespace BE.Common.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BaseV1Controller<T> : ControllerBase
        where T : class
    {

        protected readonly T _service;
        

        public BaseV1Controller(IServiceProvider services)
        {
            _service = services.GetRequiredService<T>();
            
        }

        protected IActionResult Success(object result)
        {
            return Ok(ResponseModel.Success(result));
        }

        protected IActionResult CreatedSuccess(object result)
        {
            return Created(Request.Path, ResponseModel.Success(result, HttpStatusCode.Created));
        }
    }
}
