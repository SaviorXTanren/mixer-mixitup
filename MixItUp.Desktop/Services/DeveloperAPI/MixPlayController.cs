using MixItUp.API.Models;
using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    [RoutePrefix("api/mixplay")]
    public class MixPlayController : ApiController
    {
        [Route("broadcast")]
        [HttpPost]
        public async Task Broadcast([FromBody] MixPlayBroadcast broadcastEvent)
        {
            if (broadcastEvent == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (!ChannelSession.Interactive.IsConnected())
            {
                throw new HttpResponseException(HttpStatusCode.ServiceUnavailable);
            }

            await ChannelSession.Interactive.BroadcastEvent(broadcastEvent.Scopes, broadcastEvent.Data);
        }

        [Route("users")]
        [HttpGet]
        public async Task<IEnumerable<MixPlayUser>> GetUsers()
        {
            var mixplayUsers = await ChannelSession.ActiveUsers.GetAllWorkableUsers();
            return mixplayUsers.Where(x => x.IsInteractiveParticipant).Select(x => new MixPlayUser()
            {
                ID = x.ID,
                UserName = x.UserName,
                ParticipantIDs = x.InteractiveIDs.Keys.ToList(),
            });
        }

        [Route("user/{participantIDOrUserNameOrUserId}")]
        [HttpGet]
        public async Task<MixPlayUser> GetUser(string participantIDOrUserNameOrUserId)
        {
            UserViewModel user = await ChannelSession.ActiveUsers.GetUserByID(participantIDOrUserNameOrUserId);
            if (user == null)
            {
                user = await ChannelSession.ActiveUsers.GetUserByUsername(participantIDOrUserNameOrUserId);
            }

            if (user == null)
            {
                if (uint.TryParse(participantIDOrUserNameOrUserId, out uint userId))
                {
                    user = await ChannelSession.ActiveUsers.GetUserByID(userId);
                }
            }

            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new MixPlayUser()
            {
                ID = user.ID,
                UserName = user.UserName,
                ParticipantIDs = user.InteractiveIDs.Keys.ToList(),
            };
        }
    }
}