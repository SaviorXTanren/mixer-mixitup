using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.API.Models;
using System.Collections.Generic;
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
        public async Task<IEnumerable<User>> GetChatUsers()
        {
            List<User> users = new List<User>();

            var chatUsers = await ChannelSession.ActiveUsers.GetAllWorkableUsers();
            foreach (var chatUser in chatUsers)
            {
                if (ChannelSession.Settings.UserData.ContainsKey(chatUser.ID))
                {
                    users.Add(UserController.UserFromUserDataViewModel(ChannelSession.Settings.UserData[chatUser.ID]));
                }
            }

            return users;
        }

        [Route("message")]
        [HttpDelete]
        public async Task ClearChat()
        {
            await ChannelSession.Chat.ClearMessages();
        }

        [Route("message")]
        [HttpPost]
        public async Task SendChatMessage([FromBody] SendChatMessage chatMessage)
        {
            if (chatMessage == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            await ChannelSession.Chat.SendMessage(chatMessage.Message, chatMessage.SendAsStreamer);
        }
    }
}
