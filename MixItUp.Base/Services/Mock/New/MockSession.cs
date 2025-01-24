using MixItUp.Base.Model;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mock.New
{
    public class MockSession : StreamingPlatformSessionBase
    {
        public override int MaxMessageLength { get { return 500; } }
        public override StreamingPlatformTypeEnum Platform { get { return StreamingPlatformTypeEnum.Mock; } }

        public override OAuthServiceBase StreamerOAuthService { get { return this.StreamerService; } }
        public override OAuthServiceBase BotOAuthService { get { return this.BotService; } }

        public MockService StreamerService { get; private set; } = new MockService();
        public MockService BotService { get; private set; } = new MockService(isBotService: true);

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private List<ChatMessageViewModel> messages = new List<ChatMessageViewModel>();

        protected override Task<Result> InitializeStreamerInternal()
        {
            this.StreamerID = "0";
            this.StreamerUsername = "Streamer";

            this.ChannelID = "0";
            this.ChannelLink = "https://mixitupapp.com";

            this.Streamer = UserV2ViewModel.CreateUnassociated(this.StreamerUsername);

            return Task.FromResult(new Result());
        }

        protected override Task DisconnectStreamerInternal()
        {
            return Task.CompletedTask;
        }

        protected override Task<Result> InitializeBotInternal()
        {
            this.BotID = "1";
            this.BotUsername = "Bot";

            this.Bot = UserV2ViewModel.CreateUnassociated(this.BotUsername);

            return Task.FromResult(new Result());
        }

        protected override Task DisconnectBotInternal()
        {
            return Task.CompletedTask;
        }

        public override Task RefreshOAuthTokenIfCloseToExpiring()
        {
            return Task.CompletedTask;
        }

        public override Task<Result> RefreshDetails()
        {
            return Task.FromResult(new Result());
        }

        public override Task<Result> SetStreamTitle(string title)
        {
            return Task.FromResult(new Result());
        }

        public override Task<Result> SetStreamCategory(string category)
        {
            return Task.FromResult(new Result());
        }

        public override async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            await this.messageSemaphore.WaitAsync();

            ChatMessageViewModel messageViewModel = new ChatMessageViewModel(Guid.NewGuid().ToString(), StreamingPlatformTypeEnum.Mock, sendAsStreamer ? this.Streamer : this.Bot);

            this.messages.Add(messageViewModel);

            await ServiceManager.Get<ChatService>().AddMessage(messageViewModel);

            this.messageSemaphore.Release();
        }

        public override Task DeleteMessage(ChatMessageViewModel message)
        {
            this.messages.Remove(message);

            return Task.CompletedTask;
        }

        public override Task ClearMessages()
        {
            return Task.CompletedTask;
        }

        public override Task TimeoutUser(UserV2ViewModel user, int durationInSeconds, string reason = null)
        {
            return Task.CompletedTask;
        }

        public override Task ModUser(UserV2ViewModel user)
        {
            return Task.CompletedTask;
        }

        public override Task UnmodUser(UserV2ViewModel user)
        {
            return Task.CompletedTask;
        }

        public override Task BanUser(UserV2ViewModel user, string reason = null)
        {
            return Task.CompletedTask;
        }

        public override Task UnbanUser(UserV2ViewModel user)
        {
            return Task.CompletedTask;
        }
    }
}
