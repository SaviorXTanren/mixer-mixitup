using MixItUp.Base.Model.Twitch.Ads;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Streams;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.New
{
    public class TwitchSession : StreamingPlatformSessionBase
    {
        public override IEnumerable<string> StreamerScopes { get; protected set; } = new List<string>()
        {
            "bits:read",

            "channel:edit:commercial",

            "channel:manage:ads",
            "channel:manage:broadcast",
            "channel:manage:moderators",
            "channel:manage:polls",
            "channel:manage:predictions",
            "channel:manage:raids",
            "channel:manage:redemptions",
            "channel:manage:vips",

            "channel:moderate",

            "channel:read:ads",
            "channel:read:charity",
            "channel:read:editors",
            "channel:read:goals",
            "channel:read:hype_train",
            "channel:read:polls",
            "channel:read:predictions",
            "channel:read:redemptions",
            "channel:read:subscriptions",

            "clips:edit",

            "chat:edit",
            "chat:read",

            "moderation:read",

            "moderator:read:chatters",
            "moderator:read:chat_settings",
            "moderator:read:followers",

            "moderator:manage:announcements",
            "moderator:manage:banned_users",
            "moderator:manage:chat_messages",
            "moderator:manage:chat_settings",
            "moderator:manage:shoutouts",

            "user:edit",

            "user:manage:blocked_users",
            "user:manage:whispers",

            "user:read:blocked_users",
            "user:read:broadcast",
            "user:read:follows",
            "user:read:subscriptions",

            "whispers:read",
            "whispers:edit",
        };

        public override IEnumerable<string> BotScopes { get; protected set; } = new List<string>()
        {
            "bits:read",

            "channel:moderate",

            "chat:edit",
            "chat:read",

            "moderator:manage:announcements",

            "user:edit",

            "whispers:read",
            "whispers:edit",
        };

        public HashSet<string> ChannelEditors { get; private set; } = new HashSet<string>();
        public UserModel Streamer { get; set; }
        public UserModel Bot { get; set; }
        public ChannelInformationModel Channel { get; set; }
        public StreamModel Stream { get; set; }
        public List<ChannelContentClassificationLabelModel> ContentClassificationLabels { get; private set; } = new List<ChannelContentClassificationLabelModel>();
        public AdScheduleModel AdSchedule { get; set; }
        public DateTimeOffset NextAdTimestamp { get; set; } = DateTimeOffset.MinValue;

        public override string StreamerID { get { return this.Streamer?.id; } }
        public override string StreamerUsername { get { return this.Streamer?.login; } }
        public override string BotID { get { return this.Bot?.id; } }
        public override string BotUsername { get { return this.Bot?.login; } }
        public override string ChannelID { get { return this.Streamer?.id; } }
        public override string ChannelLink { get { return string.Format("twitch.tv/{0}", this.StreamerUsername?.ToLower()); } }

        public override bool IsLive { get { return this.Stream != null || ServiceManager.Get<TwitchEventSubService>().StreamLiveStatus; } }

        public override int ViewerCount { get { return (int)(this.Stream?.viewer_count ?? 0); } }

        public DateTimeOffset StreamStart
        {
            get
            {
                if (this.IsLive)
                {
                    return TwitchPlatformService.GetTwitchDateTime(this.Stream?.started_at);
                }
                return DateTimeOffset.MinValue;
            }
        }

        public TwitchService StreamerService { get; private set; }
        public TwitchService BotService { get; private set; }
        private TwitchClient Client;

        private StreamModel streamCache;

        public override async Task<Result> Connect()
        {
            throw new NotImplementedException();
        }

        public override Task Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
