using MixItUp.API.V2.Models;
using MixItUp.Base.Model;
using MixItUp.Base.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    [RoutePrefix("api/v2/chat")]
    public class ChatControllerV2 : ApiController
    {
        // Send chat message?
        // Get users in chat?
        // Clear chat

        [Route("message")]
        [HttpPost]
        public async Task<IHttpActionResult> SendChatMessage([FromBody] SendChatMessage chatMessage)
        {
            if (chatMessage == null)
            {
                return BadRequest($"Missing chat message");
            }

            if (!Enum.TryParse<StreamingPlatformTypeEnum>(chatMessage.Platform, ignoreCase: true, out var platformEnum))
            {
                return BadRequest($"Unknown platform: {chatMessage.Platform}");
            }

            await ServiceManager.Get<ChatService>().SendMessage(chatMessage.Message, platformEnum, chatMessage.SendAsStreamer);

            return Ok();
        }

        [Route("users")]
        [HttpGet]
        public async Task<IHttpActionResult> GetAllUsers(int skip = 0, int pageSize = 25)
        {
            await ServiceManager.Get<UserService>().LoadAllUserData();

            var users = ServiceManager.Get<UserService>().GetActiveUsers()
                .OrderBy(u => u.ID)
                .Skip(skip)
                .Take(pageSize);

            var result = new GetListOfUsersResponse();
            result.TotalCount = ServiceManager.Get<UserService>().GetActiveUserCount();
            foreach (var user in users)
            {
                result.Users.Add(UserMapper.ToUser(user.Model));
            }

            return Ok(result);
        }
    }
}
