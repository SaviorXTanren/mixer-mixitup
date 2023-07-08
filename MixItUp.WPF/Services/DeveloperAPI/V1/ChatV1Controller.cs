using MixItUp.API.V1.Models;
using MixItUp.Base.Model;
using MixItUp.Base.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V1
{
    [RoutePrefix("api/chat")]
    public class ChatV1Controller : ApiController
    {
        [Route("users")]
        [HttpGet]
        public Task<IEnumerable<User>> GetChatUsers()
        {
            List<User> users = new List<User>();

            var chatUsers = ServiceManager.Get<UserService>().GetActiveUsers();
            foreach (var chatUser in chatUsers)
            {
                users.Add(UserV1Controller.UserFromUserDataViewModel(chatUser));
            }

            return Task.FromResult<IEnumerable<User>>(users);
        }

        [Route("message")]
        [HttpDelete]
        public async Task ClearChat()
        {
            await ServiceManager.Get<ChatService>().ClearMessages(StreamingPlatformTypeEnum.All);
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

            await ServiceManager.Get<ChatService>().SendMessage(chatMessage.Message, StreamingPlatformTypeEnum.All, chatMessage.SendAsStreamer);
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

            await ServiceManager.Get<ChatService>().Whisper(chatWhisper.UserName, StreamingPlatformTypeEnum.All, chatWhisper.Message, chatWhisper.SendAsStreamer);
        }
    }
}
