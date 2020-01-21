using Mixer.Base.Model.User;
using MixItUp.API.Models;
using MixItUp.Base;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    [RoutePrefix("api/mixer/users")]
    public class MixerUserController : ApiController
    {
        [Route("{userID:int:min(0)}")]
        public UserModel Get(uint userID)
        {
            UserModel user = ChannelSession.MixerUserConnection.GetUser(userID).Result;
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {userID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "User ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return user;
        }

        [Route("{username}")]
        [HttpGet]
        public UserModel Get(string username)
        {
            UserModel user = ChannelSession.MixerUserConnection.GetUser(username).Result;
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {username}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Username not found"
                };
                throw new HttpResponseException(resp);
            }

            return user;
        }
    }
}
