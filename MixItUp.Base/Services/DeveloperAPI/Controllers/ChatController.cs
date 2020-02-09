using Microsoft.AspNetCore.Mvc;
using MixItUp.API.Models;
using MixItUp.Base.Model;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.DeveloperAPI.Controllers
{
    [Route("api/chat")]
    public class ChatController : BaseController
    {
        [Route("users")]
        [HttpGet]
        public IActionResult GetChatUsers()
        {
            List<User> users = new List<User>();

            var chatUsers = ChannelSession.Services.User.GetAllWorkableUsers();
            foreach (var chatUser in chatUsers)
            {
                users.Add(UserController.UserFromUserDataViewModel(chatUser.Data));
            }

            return Ok(users);
        }

        [Route("message")]
        [HttpDelete]
        public async Task<IActionResult> ClearChat()
        {
            await ChannelSession.Services.Chat.ClearMessages();
            return Ok();
        }

        [Route("message")]
        [HttpPost]
        public async Task<IActionResult> SendChatMessage([FromBody]SendChatMessage chatMessage)
        {
            if (chatMessage == null)
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = "Unable to parse chat message from POST body." });
            }

            await ChannelSession.Services.Chat.SendMessage(chatMessage.Message, chatMessage.SendAsStreamer);
            return Ok();
        }

        [Route("whisper")]
        [HttpPost]
        public async Task<IActionResult> SendWhisper([FromBody]SendChatWhisper chatWhisper)
        {
            if (chatWhisper == null)
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = "Unable to parse chat whisper from POST body." });
            }

            await ChannelSession.Services.Chat.Whisper(StreamingPlatformTypeEnum.All, chatWhisper.UserName, chatWhisper.Message, chatWhisper.SendAsStreamer);
            return Ok();
        }
    }
}
