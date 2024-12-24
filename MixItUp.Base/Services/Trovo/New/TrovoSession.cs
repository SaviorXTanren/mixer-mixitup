using Google.Apis.YouTubePartner.v1.Data;
using MixItUp.Base.Model.Trovo.Category;
using MixItUp.Base.Model.Trovo.Channels;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.Trovo.Users;
using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.New
{
    public class TrovoSession : StreamingPlatformSessionBase
    {
        private const int MaxMessageLength = 500;

        public override IEnumerable<string> StreamerScopes { get; protected set; } = new List<string>()
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

        public override IEnumerable<string> BotScopes { get; protected set; } = new List<string>()
        {
            "chat_connect",
            "chat_send_self",
            "end_to_my_channel",
            "manage_messages",

            "user_details_self",
        };

        public IDictionary<string, TrovoChatEmoteViewModel> ChannelEmotes { get { return channelEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> channelEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public IDictionary<string, TrovoChatEmoteViewModel> EventEmotes { get { return eventEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> eventEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public IDictionary<string, TrovoChatEmoteViewModel> GlobalEmotes { get { return globalEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> globalEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public override string StreamerID { get { return Streamer?.userId; } }
        public override string StreamerUsername { get { return Streamer?.userName; } }
        public override string BotID { get { return Bot?.userId; } }
        public override string BotUsername { get { return Bot?.userName; } }
        public override string ChannelID { get { return Streamer?.channelId; } }
        public override string ChannelLink { get { return string.Format("trovo.live/{0}", StreamerUsername?.ToLower()); } }

        public PrivateUserModel Streamer { get; private set; }
        public PrivateUserModel Bot { get; private set; }
        public ChannelModel Channel { get; private set; }

        public TrovoService StreamerService { get; private set; }
        public TrovoService BotService { get; private set; }

        private TrovoClient Client;

        public override async Task RefreshDetails()
        {
            ChannelModel channel = await StreamerService.GetChannelByID(ChannelID);
            if (channel != null)
            {
                await this.UpdateChannelData(channel);
            }
        }

        protected override async Task<Result> ConnectStreamer()
        {
            Result result = await StreamerService.Connect();
            if (!result.Success)
            {
                return result;
            }

            Streamer = await StreamerService.GetUser();
            if (Streamer == null)
            {
                return new Result(Resources.TrovoFailedToGetUserData);
            }

            Channel = await StreamerService.GetChannelByID(ChannelID);
            if (Channel == null)
            {
                return new Result(Resources.TrovoFailedToGetChannelData);
            }
            await this.UpdateChannelData(Channel);

            string chatToken = await this.StreamerService.GetChatToken();
            if (string.IsNullOrEmpty(chatToken))
            {
                return new Result(Resources.TrovoChatConnectionCouldNotBeEstablished);
            }

            result = await Client.Connect(chatToken);
            if (!result.Success)
            {
                await Client.Disconnect();
                return result;
            }

            ChatEmotePackageModel emotePackage = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetPlatformAndChannelEmotes(ServiceManager.Get<TrovoSessionService>().ChannelID);
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
            }
            else
            {
                Logger.Log(LogLevel.Error, "Failed to get available Trovo emotes");
            }

            return new Result();
        }

        protected override async Task DisconnectStreamer()
        {
            await Client.Disconnect();
        }

        protected override async Task<Result> ConnectBot()
        {
            Result result = await BotService.Connect();
            if (!result.Success)
            {
                return result;
            }

            Bot = await BotService.GetUser();
            if (Bot == null)
            {
                return new Result(Resources.TrovoFailedToGetUserData);
            }

            return new Result();
        }

        protected override Task DisconnectBot()
        {
            return Task.CompletedTask;
        }

        private async Task UpdateChannelData(ChannelModel channel)
        {
            Channel = channel;

            this.IsLive = Channel.is_live;

            if (!string.Equals(this.StreamCategoryID, Channel.category_id, StringComparison.OrdinalIgnoreCase))
            {
                IEnumerable<CategoryModel> categories = await this.StreamerService.SearchCategories(Channel.category_name, maxResults: 10);
                if (categories != null && categories.Count() > 0)
                {
                    CategoryModel category = categories.FirstOrDefault(c => string.Equals(c.id, Channel.category_id, StringComparison.OrdinalIgnoreCase));
                    if (category != null)
                    {
                        this.StreamCategoryImageURL = category.icon_url;
                    }
                }
            }

            this.StreamTitle = Channel.live_title;
            this.StreamCategoryID = Channel.category_id;
            this.StreamCategoryName = Channel.category_name;
            this.StreamViewerCount = (int)Channel?.current_viewers;
        }
    }
}
