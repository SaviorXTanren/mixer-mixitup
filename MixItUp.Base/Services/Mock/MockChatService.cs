using MixItUp.Base.Model;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mock
{
    public class MockChatService : TwitchChatService
    {
        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private List<ChatMessageViewModel> messages = new List<ChatMessageViewModel>();

        public override string Name { get { return "Mock Chat"; } }

        public new Task<Result> ConnectUser() { return Task.FromResult(new Result()); }

        public new Task<Result> ConnectBot() { return Task.FromResult(new Result()); }

        public new Task DisconnectUser() { return Task.CompletedTask; }

        public new Task DisconnectBot() { return Task.CompletedTask; }

        public new async Task SendMessage(string message, bool sendAsStreamer = false, string replyMessageID = null)
        {
            await this.messageSemaphore.WaitAndRelease(async () =>
            {
                UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(StreamingPlatformTypeEnum.Twitch,
                    (sendAsStreamer) ? ServiceManager.Get<MockSessionService>().Username : ServiceManager.Get<MockSessionService>().Botname);

                ChatMessageViewModel messageViewModel = new ChatMessageViewModel(Guid.NewGuid().ToString(), StreamingPlatformTypeEnum.Twitch, user);

                this.messages.Add(messageViewModel);

                await ServiceManager.Get<ChatService>().AddMessage(messageViewModel);
            });
        }

        public new Task<bool> DeleteMessage(ChatMessageViewModel message)
        {
            this.messages.Remove(message);

            return Task.FromResult(true);
        }
    }
}
