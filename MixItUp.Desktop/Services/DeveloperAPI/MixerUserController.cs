using Mixer.Base.Model.User;
using MixItUp.Base;
using System.Net;
using System.Web.Http;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    [RoutePrefix("api/mixer/users")]
    public class MixerUserController : ApiController
    {
        [Route("{userID:int:min(0)}")]
        public UserModel Get(uint userID)
        {
            UserModel user = ChannelSession.Connection.GetUser(userID).Result;
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return user;
        }

        [Route("{username}")]
        [HttpGet]
        public UserModel Get(string username)
        {
            UserModel user = ChannelSession.Connection.GetUser(username).Result;
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return user;
        }
    }
}
