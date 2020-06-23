using Mixer.Base.Model.User;
using MixItUp.API.Models;
using MixItUp.Base;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI
{
    [RoutePrefix("api/mixer/users")]
    public class MixerUserController : ApiController
    {
        [Route("{userID:int:min(0)}")]
        public UserModel Get(uint userID)
        {
            return null;
        }

        [Route("{username}")]
        [HttpGet]
        public UserModel Get(string username)
        {
            return null;
        }
    }
}
