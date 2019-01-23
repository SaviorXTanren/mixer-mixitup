using MixItUp.API.Models;
using MixItUp.Base;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
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
        public async Task SendChatMessage([FromBody]SendChatMessage chatMessage)
        {
            if (chatMessage == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to parse chat message from POST body."}, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            await ChannelSession.Chat.SendMessage(chatMessage.Message, chatMessage.SendAsStreamer);
        }

        [Route("whisper")]
        [HttpPost]
        public async Task SendWhisper([FromBody]SendChatWhisper chatWhisper)
        {
            if (chatWhisper == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new  ObjectContent<Error>(new Error { Message = "Unable to parse chat whisper from POST body." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            await ChannelSession.Chat.Whisper(chatWhisper.UserName, chatWhisper.Message, chatWhisper.SendAsStreamer);
        }
    }
}
