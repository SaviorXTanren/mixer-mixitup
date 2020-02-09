using Microsoft.AspNetCore.Mvc;
using MixItUp.API.Models;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.DeveloperAPI.Controllers
{
    [Route("api/mixplay")]
    public class MixPlayController : BaseController
    {
        [Route("status")]
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new MixPlayStatus
            {
                IsConnected = ChannelSession.Services.MixPlay.IsConnected,
                GameName = ChannelSession.Services.MixPlay.SelectedGame?.name ?? string.Empty
            });
        }

        [Route("users")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            var mixplayUsers = ChannelSession.Services.User.GetAllWorkableUsers();
            return Ok(mixplayUsers.Where(x => x.IsInteractiveParticipant).Select(x => new MixPlayUser()
            {
                ID = x.MixerID,
                UserName = x.Username,
                ParticipantIDs = x.InteractiveIDs.Keys.ToList(),
            }));
        }

        [Route("user/{userID}")]
        [HttpGet]
        public IActionResult GetUser(uint userID)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByMixerID(userID);
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {userID.ToString()}." });
            }

            return Ok(new MixPlayUser()
            {
                ID = user.MixerID,
                UserName = user.Username,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            });
        }

        [Route("user/search/username/{userName}")]
        [HttpGet]
        public IActionResult GetUserByUserName(string userName)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByUsername(userName);

            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {userName}." });
            }

            return Ok(new MixPlayUser()
            {
                ID = user.MixerID,
                UserName = user.Username,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            });
        }

        [Route("user/search/participant/{participantID}")]
        [HttpGet]
        public IActionResult GetUserByParticipantID(string participantID)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByMixPlayID(participantID);

            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {participantID}." });
            }

            return Ok(new MixPlayUser()
            {
                ID = user.MixerID,
                UserName = user.Username,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            });
        }

        [Route("broadcast")]
        [HttpPost]
        public async Task<IActionResult> Broadcast([FromBody] MixPlayTargetBroadcast broadcast)
        {
            if (broadcast == null || broadcast.Data == null || broadcast.Targets == null)
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = $"Unable to parse broadcast from POST body." });
            }

            if (!ChannelSession.Services.MixPlay.IsConnected)
            {
                return GetErrorResponse(HttpStatusCode.ServiceUnavailable, new Error { Message = $"Unable to broadcast because to MixPlay is not connected." });
            }

            await ChannelSession.Services.MixPlay.BroadcastEvent(broadcast.Targets.Select(x => x.ScopeString()).ToList(), broadcast.Data);
            return Ok();
        }

        [Route("broadcast/users")]
        [HttpPost]
        public async Task<IActionResult> Broadcast([FromBody] MixPlayUserBroadcast broadcast)
        {
            if (broadcast == null || broadcast.Data == null || broadcast.Users == null)
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = $"Unable to parse broadcast from POST body." });
            }

            if (!ChannelSession.Services.MixPlay.IsConnected)
            {
                return GetErrorResponse(HttpStatusCode.ServiceUnavailable, new Error { Message = $"Unable to broadcast because to MixPlay is not connected." });
            }

            MixPlayBroadcastTargetBase[] targets;

            var mixplayUsers = ChannelSession.Services.User.GetUsersByMixerID(broadcast.Users.Select(x => x.UserID).ToArray());

            targets = mixplayUsers.Where(x => x.IsInteractiveParticipant).SelectMany(x => x.InteractiveIDs.Keys).Select(x => new MixPlayBroadcastParticipant(x)).ToArray();

            if (targets == null || targets.Count() == 0)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"No Matching Users Found For The Provided IDs." });
            }

            await Broadcast(new MixPlayTargetBroadcast() { Data = broadcast.Data, Targets = targets });
            return Ok();
        }
    }
}