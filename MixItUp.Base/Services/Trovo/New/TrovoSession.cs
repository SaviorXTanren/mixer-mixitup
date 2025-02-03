using MixItUp.Base.Model;
using MixItUp.Base.Model.Trovo.Category;
using MixItUp.Base.Model.Trovo.Channels;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.Trovo.Users;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.New
{
    public class TrovoSession : StreamingPlatformSessionBase
    {
        public static readonly IEnumerable<string> StreamerScopes = new List<string>()
        {
            "chat_connect",
            "chat_send_self",
            "send_to_my_channel",
            "manage_messages",

            "channel_details_self",
            "channel_update_self",
            "channel_subscriptions",

            "user_details_self",
        };

        public static readonly IEnumerable<string> BotScopes = new List<string>()
        {
            "chat_connect",
            "chat_send_self",
            "send_to_my_channel",
            "manage_messages",

            "user_details_self",
        };

        public static DateTimeOffset GetTrovoDateTime(string dateTime)
        {
            try
            {
                if (!string.IsNullOrEmpty(dateTime) && long.TryParse(dateTime, out long seconds))
                {
                    DateTimeOffset result = DateTimeOffsetExtensions.FromUTCUnixTimeSeconds(seconds);
                    if (result > DateTimeOffset.MinValue)
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{dateTime} - {ex}");
            }
            return DateTimeOffset.MinValue;
        }

        public override int MaxMessageLength { get { return 500; } }
        public override StreamingPlatformTypeEnum Platform { get { return StreamingPlatformTypeEnum.Trovo; } }

        public override OAuthServiceBase StreamerOAuthService { get { return this.StreamerService; } }
        public override OAuthServiceBase BotOAuthService { get { return this.BotService; } }

        public TrovoService StreamerService { get; private set; } = new TrovoService(StreamerScopes);
        public TrovoService BotService { get; private set; } = new TrovoService(BotScopes, isBotService: true);

        public TrovoClient Client { get; private set; } = new TrovoClient();

        public IDictionary<string, TrovoChatEmoteViewModel> ChannelEmotes { get { return channelEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> channelEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public IDictionary<string, TrovoChatEmoteViewModel> EventEmotes { get { return eventEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> eventEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public IDictionary<string, TrovoChatEmoteViewModel> GlobalEmotes { get { return globalEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> globalEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public PrivateUserModel StreamerModel { get; private set; }
        public PrivateUserModel BotModel { get; private set; }
        public ChannelModel ChannelModel { get; private set; }

        public Dictionary<string, ChannelSubscriberModel> Subscribers { get; private set; } = new Dictionary<string, ChannelSubscriberModel>();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected override async Task<Result> InitializeStreamerInternal()
        {
            this.StreamerModel = await this.StreamerService.GetCurrentUser();
            if (this.StreamerModel == null)
            {
                return new Result(Resources.TrovoFailedToGetUserData);
            }

            this.StreamerID = this.StreamerModel?.userId;
            this.StreamerUsername = this.StreamerModel?.userName;
            this.StreamerAvatarURL = this.StreamerModel?.profilePic;

            this.ChannelID = this.StreamerModel?.channelId;
            this.ChannelLink = string.Format("trovo.live/{0}", StreamerUsername?.ToLower());

            this.Streamer = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformID: this.StreamerID);
            if (this.Streamer == null)
            {
                this.Streamer = await ServiceManager.Get<UserService>().CreateUser(new TrovoUserPlatformV2Model(this.StreamerModel));
            }

            string chatToken = await this.StreamerService.GetChatToken();
            if (string.IsNullOrEmpty(chatToken))
            {
                return new Result(Resources.TrovoChatConnectionCouldNotBeEstablished);
            }
            this.Client.ChatToken = chatToken;

            Result result = await this.Client.Connect();
            if (!result.Success)
            {
                await this.Client.Disconnect();
                return result;
            }

            List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
            platformServiceTasks.Add(this.GetEmotes());
            platformServiceTasks.Add(this.GetSubscriberCache());

            await Task.WhenAll(platformServiceTasks);

            if (platformServiceTasks.Any(c => !c.Result.Success))
            {
                string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                return new Result(MixItUp.Base.Resources.TrovoFailedToConnectHeader + Environment.NewLine + Environment.NewLine + errors);
            }

            return new Result();
        }

        protected override async Task DisconnectStreamerInternal()
        {
            await this.Client.Disconnect();

            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
            }
        }

        protected override async Task<Result> InitializeBotInternal()
        {
            this.BotModel = await this.BotService.GetCurrentUser();
            if (this.BotModel == null)
            {
                return new Result(Resources.TrovoFailedToGetUserData);
            }

            this.BotID = this.BotModel?.userId;
            this.BotUsername = this.BotModel?.userName;
            this.BotAvatarURL = this.BotModel.profilePic;

            this.Bot = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformID: this.BotID);
            if (this.Bot == null)
            {
                this.Bot = await ServiceManager.Get<UserService>().CreateUser(new TrovoUserPlatformV2Model(this.BotModel));
            }

            return new Result();
        }

        protected override Task DisconnectBotInternal()
        {
            return Task.CompletedTask;
        }

        public override async Task RefreshOAuthTokenIfCloseToExpiring()
        {
            await this.StreamerService.RefreshOAuthTokenIfCloseToExpiring();
            await this.BotService.RefreshOAuthTokenIfCloseToExpiring();
        }

        public override async Task<Result> RefreshDetails()
        {
            ChannelModel channel = await StreamerService.GetChannelByID(ChannelID);
            if (channel == null)
            {
                return new Result(Resources.TrovoFailedToGetChannelData);
            }

            this.ChannelModel = channel;

            this.IsLive = this.ChannelModel.is_live;

            if (!string.Equals(this.StreamCategoryID, this.ChannelModel.category_id, StringComparison.OrdinalIgnoreCase))
            {
                IEnumerable<CategoryModel> categories = await this.StreamerService.SearchCategories(ChannelModel.category_name, maxResults: 10);
                if (categories != null && categories.Count() > 0)
                {
                    CategoryModel category = categories.FirstOrDefault(c => string.Equals(c.id, ChannelModel.category_id, StringComparison.OrdinalIgnoreCase));
                    if (category != null)
                    {
                        this.StreamCategoryImageURL = category.icon_url;
                    }
                }
            }

            this.StreamTitle = this.ChannelModel.live_title;
            this.StreamCategoryID = this.ChannelModel.category_id;
            this.StreamCategoryName = this.ChannelModel.category_name;
            this.StreamViewerCount = (int)this.ChannelModel?.current_viewers;
            this.StreamStart = TrovoSession.GetTrovoDateTime(this.ChannelModel?.started_at);

            return new Result();
        }

        public override async Task<Result> SetStreamTitle(string title)
        {
            return await this.StreamerService.UpdateChannel(this.ChannelID, title: title);
        }

        public override async Task<Result> SetStreamCategory(string category)
        {
            IEnumerable<CategoryModel> categories = await this.StreamerService.SearchCategories(category, maxResults: 10);
            if (categories != null && categories.Count() > 0)
            {
                CategoryModel c = categories.FirstOrDefault();
                if (c != null)
                {
                    return await this.StreamerService.UpdateChannel(this.ChannelID, categoryID: c?.id);
                }
            }

            return new Result(success: false);
        }

        public override async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            foreach (string m in this.SplitLargeMessage(message))
            {
                if (sendAsStreamer || !this.IsBotConnected)
                {
                    await this.StreamerService.SendMessage(m);
                }
                else
                {
                    await this.BotService.SendMessage(m, this.ChannelID);
                }
            }
        }

        public override async Task DeleteMessage(ChatMessageViewModel message)
        {
            await this.StreamerService.DeleteMessage(this.ChannelID, message.ID, message.User?.PlatformID);
        }

        public override async Task ClearMessages()
        {
            await this.StreamerService.ClearChat(this.ChannelID);
        }

        public override async Task TimeoutUser(UserV2ViewModel user, int durationInSeconds, string reason = null)
        {
            await this.StreamerService.TimeoutUser(this.ChannelID, user.Username, durationInSeconds);
        }

        public override async Task ModUser(UserV2ViewModel user)
        {
            await this.StreamerService.ModUser(this.ChannelID, user.Username);
        }

        public override async Task UnmodUser(UserV2ViewModel user)
        {
            await this.StreamerService.UnmodUser(this.ChannelID, user.Username);
        }

        public override async Task BanUser(UserV2ViewModel user, string reason = null)
        {
            await this.StreamerService.BanUser(this.ChannelID, user.Username);
        }

        public override async Task UnbanUser(UserV2ViewModel user)
        {
            await this.StreamerService.UnbanUser(this.ChannelID, user.Username);
        }

        private async Task<Result> GetEmotes()
        {
            ChatEmotePackageModel emotePackage = await this.StreamerService.GetPlatformAndChannelEmotes(this.ChannelID);
            if (emotePackage != null)
            {
                if (emotePackage.customizedEmotes?.channel != null)
                {
                    foreach (ChannelChatEmotesModel channel in emotePackage.customizedEmotes.channel)
                    {
                        foreach (ChatEmoteModel emote in channel.emotes)
                        {
                            this.ChannelEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                        }
                    }
                }

                if (emotePackage.eventEmotes != null)
                {
                    foreach (EventChatEmoteModel emote in emotePackage.eventEmotes)
                    {
                        this.EventEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                    }
                }

                if (emotePackage.globalEmotes != null)
                {
                    foreach (GlobalChatEmoteModel emote in emotePackage.globalEmotes)
                    {
                        this.GlobalEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                    }
                }

                return new Result();
            }
            return new Result(Resources.TrovoFailedToGetEmoteData);
        }

        private async Task<Result> GetSubscriberCache()
        {
            IEnumerable<ChannelSubscriberModel> subscribers = await this.StreamerService.GetSubscribers(this.ChannelID, int.MaxValue);
            if (subscribers != null)
            {
                this.Subscribers.Clear();
                foreach (ChannelSubscriberModel subscriber in subscribers)
                {
                    this.Subscribers[subscriber.user.user_id] = subscriber;
                }
                return new Result();
            }
            return new Result(Resources.TrovoFailedToGetSubscriberData);
        }
    }
}
