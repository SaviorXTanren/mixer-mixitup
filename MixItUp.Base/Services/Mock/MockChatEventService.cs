using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mock
{
    [Obsolete]
    public class MockChatEventService : StreamingPlatformServiceBase
    {
        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private List<ChatMessageViewModel> messages = new List<ChatMessageViewModel>();

        public override string Name { get { return "Mock Chat/Event"; } }

        public Task<Result> ConnectUser() { return Task.FromResult(new Result()); }

        public Task<Result> ConnectBot() { return Task.FromResult(new Result()); }

        public Task DisconnectUser() { return Task.CompletedTask; }

        public Task DisconnectBot() { return Task.CompletedTask; }

        public async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            await this.messageSemaphore.WaitAsync();

            UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Mock,
                platformUsername: sendAsStreamer ? ServiceManager.Get<MockSessionService>().Username : ServiceManager.Get<MockSessionService>().Botname);

            ChatMessageViewModel messageViewModel = new ChatMessageViewModel(Guid.NewGuid().ToString(), StreamingPlatformTypeEnum.Mock, user);

            this.messages.Add(messageViewModel);

            await ServiceManager.Get<ChatService>().AddMessage(messageViewModel);

            this.messageSemaphore.Release();
        }

        public Task<bool> DeleteMessage(ChatMessageViewModel message)
        {
            this.messages.Remove(message);

            return Task.FromResult(true);
        }
    }
}
