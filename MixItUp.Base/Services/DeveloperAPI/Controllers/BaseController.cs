using System.Net;
using Microsoft.AspNetCore.Mvc;
using StreamingClient.Base.Util;

namespace MixItUp.Base.Services.DeveloperAPI.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult GetErrorResponse<T>(HttpStatusCode status, T content)
        {
            return StatusCode((int)status, JSONSerializerHelper.SerializeToString(content));
        }
    }
}
