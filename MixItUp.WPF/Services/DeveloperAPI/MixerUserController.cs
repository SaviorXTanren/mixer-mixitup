using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI
{
    [RoutePrefix("api/mixer/users")]
    public class MixerUserController : ApiController
    {
        [Route("{userID:int:min(0)}")]
        public object Get(uint userID)
        {
            return null;
        }

        [Route("{username}")]
        [HttpGet]
        public object Get(string username)
        {
            return null;
        }
    }
}
