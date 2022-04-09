using MixItUp.API.V2.Models;
using MixItUp.Base.Model;
using MixItUp.Base.Services;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    [RoutePrefix("api/v2/chat")]
    public class ChatV2Controller : ApiController
    {
        [Route("message")]
        [HttpPost]
        public async Task<IHttpActionResult> SendChatMessage([FromBody] SendChatMessage chatMessage)
        {
            if (chatMessage == null)
            {
                return BadRequest($"Missing chat message");
            }

            StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.All;
            if (!string.IsNullOrEmpty(chatMessage.Platform) && !Enum.TryParse<StreamingPlatformTypeEnum>(chatMessage.Platform, ignoreCase: true, out platform))
            {
                return BadRequest($"Unknown platform: {chatMessage.Platform}");
            }

            await ServiceManager.Get<ChatService>().SendMessage(chatMessage.Message, platform, chatMessage.SendAsStreamer);

            return Ok();
        }

        [Route("clear")]
        [HttpPost]
        public async Task<IHttpActionResult> ClearChat()
        {
            await ServiceManager.Get<ChatService>().ClearMessages(StreamingPlatformTypeEnum.All);
            return Ok();
        }
    }
}