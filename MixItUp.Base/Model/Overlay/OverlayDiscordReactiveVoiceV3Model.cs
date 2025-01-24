using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayDiscordReactiveVoiceNameDisplayTypeEnum
    {
        Hide,
        Username,
        DisplayName,
        ServerDisplayName
    }

    [DataContract]
    public class OverlayDiscordReactiveVoiceUserV3Model
    {
        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string ServerDisplayName { get; set; }

        [DataMember]
        public List<OverlayDiscordReactiveVoiceUserCustomVisualV3Model> CustomVisuals { get; set; } = new List<OverlayDiscordReactiveVoiceUserCustomVisualV3Model>();
    }

    [DataContract]
    public class OverlayDiscordReactiveVoiceUserCustomVisualV3Model
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string CustomActiveImageFilePath { get; set; }
        [DataMember]
        public string CustomInactiveImageFilePath { get; set; }
        [DataMember]
        public string CustomMutedImageFilePath { get; set; }
        [DataMember]
        public string CustomDeafenImageFilePath { get; set; }

        [DataMember]
        public int CustomWidth { get; set; }
        [DataMember]
        public int CustomHeight { get; set; }
    }

    [DataContract]
    public class OverlayDiscordReactiveVoiceV3Model : OverlayVisualTextV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayDiscordReactiveVoiceDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayDiscordReactiveVoiceDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayDiscordReactiveVoiceDefaultJavascript;

        [DataMember]
        public string DiscordVoiceChannelID { get; set; }

        [DataMember]
        public int UserWidth { get; set; }
        [DataMember]
        public int UserHeight { get; set; }
        [DataMember]
        public int UserSpacing { get; set; }

        [DataMember]
        public bool IncludeSelf { get; set; }
        [DataMember]
        public bool OnlyShowAddedUsers { get; set; }
        [DataMember]
        public bool DimInactiveUsers { get; set; }

        [DataMember]
        public OverlayDiscordReactiveVoiceNameDisplayTypeEnum NameDisplay { get; set; }

        [DataMember]
        public Dictionary<string, OverlayDiscordReactiveVoiceUserV3Model> Users { get; set; } = new Dictionary<string, OverlayDiscordReactiveVoiceUserV3Model>();

        [DataMember]
        public OverlayAnimationV3Model ActiveAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model InactiveAnimation { get; set; } = new OverlayAnimationV3Model();

        [DataMember]
        public OverlayAnimationV3Model MutedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model UnmutedAnimation { get; set; } = new OverlayAnimationV3Model();

        [DataMember]
        public OverlayAnimationV3Model DeafenAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model UndeafenAnimation { get; set; } = new OverlayAnimationV3Model();

        [DataMember]
        public OverlayAnimationV3Model JoinedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model LeftAnimation { get; set; } = new OverlayAnimationV3Model();

        [JsonIgnore]
        private Dictionary<string, DiscordServerUser> userCache = new Dictionary<string, DiscordServerUser>();

        public OverlayDiscordReactiveVoiceV3Model() : base(OverlayItemV3Type.DiscordReactiveVoice) { }

        public override async Task Initialize()
        {
            await base.Initialize();

            this.RemoveEventHandlers();

            if (ServiceManager.Get<DiscordService>().IsConnected && ServiceManager.Get<DiscordService>().IsUsingCustomApplication && !string.IsNullOrWhiteSpace(this.DiscordVoiceChannelID))
            {
                if (ServiceManager.Get<DiscordService>().ConnectedVoiceChannelID != null && string.Equals(this.DiscordVoiceChannelID, ServiceManager.Get<DiscordService>().ConnectedVoiceChannelID))
                {
                    // Already connected to the same voice channel, don't re-connect
                }

                if (await ServiceManager.Get<DiscordService>().ConnectToVoice(ServiceManager.Get<DiscordService>().Server, this.DiscordVoiceChannelID))
                {
                    ServiceManager.Get<DiscordService>().OnUserJoinedVoice += OverlayDiscordReactiveVoiceV3Model_OnUserJoinedVoice;
                    ServiceManager.Get<DiscordService>().OnUserLeftVoice += OverlayDiscordReactiveVoiceV3Model_OnUserLeftVoice;
                    ServiceManager.Get<DiscordService>().OnUserStartedSpeaking += OverlayDiscordReactiveVoiceV3Model_OnUserStartedSpeaking;
                    ServiceManager.Get<DiscordService>().OnUserStoppedSpeaking += OverlayDiscordReactiveVoiceV3Model_OnUserStoppedSpeaking;

                    await this.CallFunction("connected", new Dictionary<string, object>());
                }
                else
                {
                    await this.CallFunction("disconnected", new Dictionary<string, object>());
                }
            }
        }

        public override async Task Uninitialize()
        {
            await base.Uninitialize();

            this.RemoveEventHandlers();

            await ServiceManager.Get<DiscordService>().DisconnectFromVoice();
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.UserWidth)] = this.UserWidth;
            properties[nameof(this.UserHeight)] = this.UserHeight;
            properties[nameof(this.UserSpacing)] = this.UserSpacing;

            properties[nameof(this.IncludeSelf)] = this.IncludeSelf.ToString().ToLower();
            properties[nameof(this.OnlyShowAddedUsers)] = this.OnlyShowAddedUsers.ToString().ToLower();
            properties[nameof(this.DimInactiveUsers)] = this.DimInactiveUsers.ToString().ToLower();

            properties[nameof(this.NameDisplay)] = this.NameDisplay.ToString();

            properties[nameof(this.Users)] = JSONSerializerHelper.SerializeToString(this.Users);

            this.ActiveAnimation.AddAnimationProperties(properties, nameof(this.ActiveAnimation));
            this.InactiveAnimation.AddAnimationProperties(properties, nameof(this.InactiveAnimation));
            this.MutedAnimation.AddAnimationProperties(properties, nameof(this.MutedAnimation));
            this.UnmutedAnimation.AddAnimationProperties(properties, nameof(this.UnmutedAnimation));
            this.DeafenAnimation.AddAnimationProperties(properties, nameof(this.DeafenAnimation));
            this.UndeafenAnimation.AddAnimationProperties(properties, nameof(this.UndeafenAnimation));
            this.JoinedAnimation.AddAnimationProperties(properties, nameof(this.JoinedAnimation));
            this.LeftAnimation.AddAnimationProperties(properties, nameof(this.LeftAnimation));

            return properties;
        }

        public async Task UserJoined(DiscordServerUser user)
        {
            await this.CallFunction("userJoined", this.GetDiscordUserProperties(user));
        }

        public async Task UserLeft(DiscordServerUser user)
        {
            await this.CallFunction("userLeft", this.GetDiscordUserProperties(user));
        }

        public async Task UserActive(DiscordServerUser user)
        {
            await this.CallFunction("userActive", this.GetDiscordUserProperties(user));
        }

        public async Task UserInactive(DiscordServerUser user)
        {
            await this.CallFunction("userInactive", this.GetDiscordUserProperties(user));
        }

        public async Task UserMuted(DiscordServerUser user)
        {
            await this.CallFunction("userMuted", this.GetDiscordUserProperties(user));
        }

        public async Task UserUnmuted(DiscordServerUser user)
        {
            await this.CallFunction("userUnmuted", this.GetDiscordUserProperties(user));
        }

        public async Task UserDeafened(DiscordServerUser user)
        {
            await this.CallFunction("userDeafened", this.GetDiscordUserProperties(user));
        }

        public async Task UserUndeafened(DiscordServerUser user)
        {
            await this.CallFunction("userUndeafened", this.GetDiscordUserProperties(user));
        }

        private Dictionary<string, object> GetDiscordUserProperties(DiscordServerUser user)
        {
            return new Dictionary<string, object>()
            {
                { "UserID", user.User.ID },
                { "Username", user.User.UserName },
                { "DisplayName", user.User.GlobalName },
                { "ServerDisplayName", user.Nickname ?? user.User.GlobalName }
            };
        }

        private void RemoveEventHandlers()
        {
            ServiceManager.Get<DiscordService>().OnUserJoinedVoice -= OverlayDiscordReactiveVoiceV3Model_OnUserJoinedVoice;
            ServiceManager.Get<DiscordService>().OnUserLeftVoice -= OverlayDiscordReactiveVoiceV3Model_OnUserLeftVoice;
            ServiceManager.Get<DiscordService>().OnUserStartedSpeaking -= OverlayDiscordReactiveVoiceV3Model_OnUserStartedSpeaking;
            ServiceManager.Get<DiscordService>().OnUserStoppedSpeaking -= OverlayDiscordReactiveVoiceV3Model_OnUserStoppedSpeaking;
        }

        private void OverlayDiscordReactiveVoiceV3Model_OnUserJoinedVoice(object sender, string userID)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                DiscordServerUser user = await this.GetUser(userID);
                if (user != null)
                {
                    this.UserJoined(user);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void OverlayDiscordReactiveVoiceV3Model_OnUserLeftVoice(object sender, string userID)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                DiscordServerUser user = await this.GetUser(userID);
                if (user != null)
                {
                    this.UserLeft(user);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void OverlayDiscordReactiveVoiceV3Model_OnUserStartedSpeaking(object sender, string userID)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                DiscordServerUser user = await this.GetUser(userID);
                if (user != null)
                {
                    this.UserActive(user);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void OverlayDiscordReactiveVoiceV3Model_OnUserStoppedSpeaking(object sender, string userID)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                DiscordServerUser user = await this.GetUser(userID);
                if (user != null)
                {
                    this.UserInactive(user);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task<DiscordServerUser> GetUser(string userID)
        {
            if (this.userCache.TryGetValue(userID, out DiscordServerUser user))
            {
                return user;
            }

            user = await ServiceManager.Get<DiscordService>().GetServerMember(ServiceManager.Get<DiscordService>().Server, userID);
            if (user != null)
            {
                this.userCache[userID] = user;

                if (this.Users.TryGetValue(userID, out OverlayDiscordReactiveVoiceUserV3Model userSettings))
                {
                    userSettings.Username = user.User.UserName;
                    userSettings.DisplayName = user.User.GlobalName;
                    userSettings.ServerDisplayName = user.Nickname;
                }
            }

            return user;
        }
    }
}
