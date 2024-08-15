using MixItUp.Base.Services.External;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

        public OverlayDiscordReactiveVoiceV3Model() : base(OverlayItemV3Type.DiscordReactiveVoice) { }

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

            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.ActiveAnimation), this.ActiveAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.InactiveAnimation), this.InactiveAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.MutedAnimation), this.MutedAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.UnmutedAnimation), this.UnmutedAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.DeafenAnimation), this.DeafenAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.UndeafenAnimation), this.UndeafenAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.JoinedAnimation), this.JoinedAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.LeftAnimation), this.LeftAnimation);

            return properties;
        }

        public async Task UserJoined(DiscordUser user)
        {
            await this.CallFunction("userJoined", this.GetDiscordUserProperties(user));
        }

        public async Task UserLeft(DiscordUser user)
        {
            await this.CallFunction("userLeft", this.GetDiscordUserProperties(user));
        }

        public async Task UserActive(DiscordUser user)
        {
            await this.CallFunction("userActive", this.GetDiscordUserProperties(user));
        }

        public async Task UserInactive(DiscordUser user)
        {
            await this.CallFunction("userInactive", this.GetDiscordUserProperties(user));
        }

        public async Task UserMuted(DiscordUser user)
        {
            await this.CallFunction("userMuted", this.GetDiscordUserProperties(user));
        }

        public async Task UserUnmuted(DiscordUser user)
        {
            await this.CallFunction("userUnmuted", this.GetDiscordUserProperties(user));
        }

        public async Task UserDeafened(DiscordUser user)
        {
            await this.CallFunction("userDeafened", this.GetDiscordUserProperties(user));
        }

        public async Task UserUndeafened(DiscordUser user)
        {
            await this.CallFunction("userUndeafened", this.GetDiscordUserProperties(user));
        }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();
        }

        private Dictionary<string, object> GetDiscordUserProperties(DiscordUser user)
        {
            return new Dictionary<string, object>()
            {
                { "UserID", user.ID },
                { "Username", user.UserName },
            };
        }
    }
}
