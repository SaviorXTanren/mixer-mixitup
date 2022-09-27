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

namespace MixItUp.Base.Services.Demo
{
    public class DemoChatService : StreamingPlatformServiceBase
    {
        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private List<ChatMessageViewModel> messages = new List<ChatMessageViewModel>();

        public override string Name { get { return "Mock Chat"; } }

        public Task<Result> ConnectUser() { return Task.FromResult(new Result()); }

        public Task<Result> ConnectBot() { return Task.FromResult(new Result()); }

        public Task DisconnectUser() { return Task.CompletedTask; }

        public Task DisconnectBot() { return Task.CompletedTask; }

        public async Task SendMessage(string message, bool sendAsStreamer = false, string replyMessageID = null)
        {
            await this.messageSemaphore.WaitAndRelease(async () =>
            {
#pragma warning disable CS0612 // Type or member is obsolete
                UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(StreamingPlatformTypeEnum.Demo,
                    (sendAsStreamer) ? ServiceManager.Get<DemoSessionService>().Username : ServiceManager.Get<DemoSessionService>().Botname,
                    performPlatformSearch: true);

                UserChatMessageViewModel messageViewModel = new UserChatMessageViewModel(Guid.NewGuid().ToString(), StreamingPlatformTypeEnum.Demo, user);
#pragma warning restore CS0612 // Type or member is obsolete

                foreach (string part in message.Split(new char[] {' '}))
                {
                    messageViewModel.AddStringMessagePart(part);
                }

                this.messages.Add(messageViewModel);

                await ServiceManager.Get<ChatService>().AddMessage(messageViewModel);
            });
        }

        public Task<bool> DeleteMessage(ChatMessageViewModel message)
        {
            this.messages.Remove(message);

            return Task.FromResult(true);
        }
    }
}
