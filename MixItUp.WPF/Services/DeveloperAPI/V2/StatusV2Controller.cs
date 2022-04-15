using System.Reflection;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    [RoutePrefix("api/v2/status")]
    public class StatusV2Controller : ApiController
    {
        [Route("version")]
        [HttpGet]
        public IHttpActionResult GetVersion()
        {
            return Ok(Assembly.GetEntryAssembly().GetName().Version.ToString());
        }
    }
}