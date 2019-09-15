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

namespace MixItUp.Desktop.Services.DeveloperAPI
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
                IsConnected = ChannelSession.Interactive.IsConnected(),
                GameName = ChannelSession.Interactive.Game?.name ?? string.Empty
            };
        }

        [Route("users")]
        [HttpGet]
        public async Task<IEnumerable<MixPlayUser>> GetUsers()
        {
            var mixplayUsers = ChannelSession.Services.User.GetAllWorkableUsers();
            return mixplayUsers.Where(x => x.IsInteractiveParticipant).Select(x => new MixPlayUser()
            {
                ID = x.ID,
                UserName = x.UserName,
                ParticipantIDs = x.InteractiveIDs.Keys.ToList(),
            });
        }

        [Route("user/{userID}")]
        [HttpGet]
        public async Task<MixPlayUser> GetUser(uint userID)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByID(userID);
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user by ID: {userID}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "User not found"
                };
                throw new HttpResponseException(resp);
            }

            return new MixPlayUser()
            {
                ID = user.ID,
                UserName = user.UserName,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            };
        }

        [Route("user/search/username/{userName}")]
        [HttpGet]
        public async Task<MixPlayUser> GetUserByUserName(string userName)
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

            return new MixPlayUser()
            {
                ID = user.ID,
                UserName = user.UserName,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            };
        }

        [Route("user/search/participant/{participantID}")]
        [HttpGet]
        public async Task<MixPlayUser> GetUserByParticipantID(string participantID)
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

            return new MixPlayUser()
            {
                ID = user.ID,
                UserName = user.UserName,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            };
        }

        [Route("broadcast")]
        [HttpPost]
        public async Task Broadcast([FromBody] MixPlayTargetBroadcast broadcast)
        {
            if (broadcast == null || broadcast.Data == null || broadcast.Targets == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to parse broadcast from POST body." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            if (!ChannelSession.Interactive.IsConnected())
            {
                var resp = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to broadcast because to MixPlay is not connected" }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "MixPlay Service Not Connected"
                };
                throw new HttpResponseException(resp);
            }

            await ChannelSession.Interactive.BroadcastEvent(broadcast.Targets.Select(x => x.ScopeString()).ToList(), broadcast.Data);
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

            if (!ChannelSession.Interactive.IsConnected())
            {
                var resp = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to broadcast because to MixPlay is not connected" }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "MixPlay Service Not Connected"
                };
                throw new HttpResponseException(resp);
            }

            MixPlayBroadcastTargetBase[] targets;

            var mixplayUsers = ChannelSession.Services.User.GetUsersByID(broadcast.Users.Select(x => x.UserID).ToArray());

            targets = mixplayUsers.Where(x => x.IsInteractiveParticipant).SelectMany(x => x.InteractiveIDs.Keys).Select(x => new MixPlayBroadcastParticipant(x)).ToArray();

            if (targets == null || targets.Count() == 0)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "No Matching Users Found For The Provided IDs" }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "No Users Found"
                };
                throw new HttpResponseException(resp);
            }

            await Broadcast(new MixPlayTargetBroadcast() { Data = broadcast.Data, Targets = targets });
        }
    }
}