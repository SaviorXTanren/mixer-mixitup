using Microsoft.AspNetCore.Mvc;
using Mixer.Base.Model.User;
using MixItUp.API.Models;
using System.Net;
using System.Net.Http;

namespace MixItUp.Base.Services.DeveloperAPI.Controllers
{
    [Route("api/mixer/users")]
    public class MixerUserController : BaseController
    {
        [Route("{userID:int:min(0)}")]
        public IActionResult Get(uint userID)
        {
            UserModel user = ChannelSession.MixerUserConnection.GetUser(userID).Result;
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {userID.ToString()}." });
            }

            return Ok(user);
        }

        [Route("{username}")]
        [HttpGet]
        public IActionResult Get(string username)
        {
            UserModel user = ChannelSession.MixerUserConnection.GetUser(username).Result;
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {username}." });
            }

            return Ok(user);
        }
    }
}
