﻿using MixItUp.Base.Model.Trovo.Channels;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.Trovo.Users;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using System.Collections.Generic;
using System.Threading;
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

        public override bool IsLive
        {
            get
            {
                bool? isLive = this.Channel?.is_live;
                return isLive.GetValueOrDefault();
            }
        }

        public override int ViewerCount { get { return (int)this.Channel?.current_viewers; } }

        public PrivateUserModel Streamer { get; private set; }
        public PrivateUserModel Bot { get; private set; }
        public ChannelModel Channel { get; private set; }

        public TrovoService StreamerService { get; private set; }
        public TrovoService BotService { get; private set; }

        private TrovoClient Client;

        public override async Task<Result> Connect()
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

            result = await BotService.Connect();
            if (!result.Success)
            {
                return result;
            }

            Bot = await BotService.GetUser();
            if (Bot == null)
            {
                return new Result(Resources.TrovoFailedToGetUserData);
            }

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

        public override async Task Disconnect()
        {
            await Client.Disconnect();
        }
    }
}