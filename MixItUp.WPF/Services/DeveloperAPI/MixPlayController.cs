using MixItUp.API.Models;
using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI
{
    [RoutePrefix("api/mixplay")]
    public class MixPlayController : ApiController
    {
        [Route("status")]
        [HttpGet]
        public MixPlayStatus GetStatus()
        {
            return new MixPlayStatus
            {
                IsConnected = ChannelSession.Services.MixPlay.IsConnected,
                GameName = ChannelSession.Services.MixPlay.SelectedGame?.name ?? string.Empty
            };
        }

        [Route("users")]
        [HttpGet]
        public Task<IEnumerable<MixPlayUser>> GetUsers()
        {
            var mixplayUsers = ChannelSession.Services.User.GetAllWorkableUsers();
            return Task.FromResult<IEnumerable<MixPlayUser>>(mixplayUsers.Where(x => x.IsMixerMixPlayParticipant).Select(x => new MixPlayUser()
            {
                ID = x.MixerID,
                UserName = x.Username,
                ParticipantIDs = x.InteractiveIDs.Keys.ToList(),
            }));
        }

        [Route("user/{userID}")]
        [HttpGet]
        public Task<MixPlayUser> GetUser(uint userID)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByMixerID(userID);
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user by ID: {userID}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "User not found"
                };
                throw new HttpResponseException(resp);
            }

            return Task.FromResult(new MixPlayUser()
            {
                ID = user.MixerID,
                UserName = user.Username,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            });
        }

        [Route("user/search/username/{userName}")]
        [HttpGet]
        public Task<MixPlayUser> GetUserByUserName(string userName)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByUsername(userName);

            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user by name: {userName}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "User not found"
                };
                throw new HttpResponseException(resp);
            }

            return Task.FromResult(new MixPlayUser()
            {
                ID = user.MixerID,
                UserName = user.Username,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            });
        }

        [Route("user/search/participant/{participantID}")]
        [HttpGet]
        public Task<MixPlayUser> GetUserByParticipantID(string participantID)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByMixPlayID(participantID);

            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find participant by ID: {participantID}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Participant not found"
                };
                throw new HttpResponseException(resp);
            }

            return Task.FromResult(new MixPlayUser()
            {
                ID = user.MixerID,
                UserName = user.Username,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            });
        }

        [Route("broadcast")]
        [HttpPost]
        public async Task Broadcast([FromBody] MixPlayTargetBroadcast broadcast)
        {
            if (broadcast == null || broadcast.Data == null || broadcast.Scopes == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to parse broadcast from POST body." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            if (!ChannelSession.Services.MixPlay.IsConnected)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to broadcast because to MixPlay is not connected" }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "MixPlay Service Not Connected"
                };
                throw new HttpResponseException(resp);
            }

            await ChannelSession.Services.MixPlay.BroadcastEvent(broadcast.Scopes, broadcast.Data);
        }

        [Route("broadcast/users")]
        [HttpPost]
        public async Task Broadcast([FromBody] MixPlayUserBroadcast broadcast)
        {
            if (broadcast == null || broadcast.Data == null || broadcast.Users == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to parse broadcast from POST body." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            if (!ChannelSession.Services.MixPlay.IsConnected)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to broadcast because to MixPlay is not connected" }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "MixPlay Service Not Connected"
                };
                throw new HttpResponseException(resp);
            }

            MixPlayBroadcastTargetBase[] targets;

            var mixplayUsers = ChannelSession.Services.User.GetUsersByMixerID(broadcast.Users.Select(x => x.UserID).ToArray());

            targets = mixplayUsers.Where(x => x.IsMixerMixPlayParticipant).SelectMany(x => x.InteractiveIDs.Keys).Select(x => new MixPlayBroadcastParticipant(x)).ToArray();

            if (targets == null || targets.Count() == 0)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "No Matching Users Found For The Provided IDs" }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "No Users Found"
                };
                throw new HttpResponseException(resp);
            }

            await Broadcast(new MixPlayTargetBroadcast() { Data = broadcast.Data, Scopes = targets.Select(t => t.ScopeString()).ToArray() });
        }
    }
}