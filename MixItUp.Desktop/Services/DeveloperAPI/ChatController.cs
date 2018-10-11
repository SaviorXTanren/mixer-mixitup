using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Services.DeveloperAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    [RoutePrefix("api/chat")]
    public class ChatController : ApiController
    {
        [Route("users")]
        [HttpGet]
        public async Task<IEnumerable<UserDeveloperAPIModel>> GetChatUsers()
        {
            List<UserDeveloperAPIModel> users = new List<UserDeveloperAPIModel>();

            var chatUsers = await ChannelSession.ActiveUsers.GetAllWorkableUsers();
            foreach(var chatUser in chatUsers)
            {
                if (ChannelSession.Settings.UserData.ContainsKey(chatUser.ID))
                {
                    users.Add(new UserDeveloperAPIModel(ChannelSession.Settings.UserData[chatUser.ID]));
                }
            }

            return users;
        }

        [Route("message")]
        [HttpPost]
        public async Task SendChatMessage([FromBody] ChatMessageDeveloperAPIModel chatMessage)
        {
            if (chatMessage == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            await ChannelSession.Chat.SendMessage(chatMessage.Message, chatMessage.SendAsStreamer);
        }
    }
}
