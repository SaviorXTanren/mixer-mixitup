using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayItemV3Type
    {
        [OverlayWidget]
        Text,
        [OverlayWidget]
        Image,
        [OverlayWidget]
        Video,
        [OverlayWidget]
        YouTube,
        [OverlayWidget]
        HTML,
        Timer,
        TwitchClip,

        Sound,

        [OverlayWidget]
        PersistentTimer,
        [OverlayWidget]
        Label,
        [OverlayWidget]
        StreamBoss,
        [OverlayWidget]
        Goal,
        [OverlayWidget]
        Chat,
        [OverlayWidget]
        EndCredits,
        [OverlayWidget]
        GameQueue,
        [OverlayWidget]
        EventList,
        [OverlayWidget]
        Leaderboard,
        [OverlayWidget]
        Wheel,
        EmoteEffect,
        [OverlayWidget]
        PersistentEmoteEffect,
        [OverlayWidget]
        Poll,
        [Obsolete]
        [OverlayWidget]
        DiscordReactiveVoice,

        [Obsolete]
        JavascriptScript = 998,

        [OverlayWidget]
        Custom = 999,
    }

    public enum OverlayItemV3DisplayOptionsType
    {
        OverlayEndpoint,
        SingleWidgetURL,
    }

    public enum OverlayPositionV3Type
    {
        Simple,
        Percentage,
        Pixel,
        Random
    }

    [AttributeUsage(AttributeTargets.All)]
    public class OverlayWidgetAttribute : Attribute
    {
        public static readonly OverlayWidgetAttribute Default;

        public OverlayWidgetAttribute() { }

        public override bool IsDefaultAttribute() { return this.Equals(OverlayWidgetAttribute.Default); }
    }

    [DataContract]
    public abstract class OverlayItemV3ModelBase : OverlayOutputV3Model
    {
        public const string InnerHTMLProperty = "InnerHTML";

        public const string UserProperty = "User";

        public static readonly string PositionedHTML = OverlayResources.OverlayPositionedItemDefaultHTML;
        public static readonly string PositionedCSS = OverlayResources.OverlayPositionedItemDefaultCSS;

        public event EventHandler WidgetLoaded = delegate { };

        public static string GetPositionWrappedHTML(string innerHTML)
        {
            if (!string.IsNullOrEmpty(innerHTML))
            {
                return OverlayV3Service.ReplaceProperty(OverlayItemV3ModelBase.PositionedHTML, OverlayItemV3ModelBase.InnerHTMLProperty, innerHTML);
            }
            return innerHTML;
        }

        public static string GetPositionWrappedCSS(string innerCSS)
        {
            if (!string.IsNullOrEmpty(innerCSS))
            {
                return OverlayItemV3ModelBase.PositionedCSS + Environment.NewLine + Environment.NewLine + innerCSS;
            }
            return innerCSS;
        }

        public static string GetRandomHTMLColor(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = Guid.NewGuid().ToString();
            }
            int index = Math.Abs(text.GetHashCode() % ColorSchemes.HTMLColors.Count);
            return ColorSchemes.HTMLColors.ElementAt(index);
        }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        public OverlayItemV3Type Type { get; set; }

        [DataMember]
        public OverlayItemV3DisplayOptionsType DisplayOption { get; set; } = OverlayItemV3DisplayOptionsType.OverlayEndpoint;

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayPositionV3Type PositionType { get; set; }

        [DataMember]
        public int XPosition { get; set; }
        [DataMember]
        public int YPosition { get; set; }

        [DataMember]
        public int XMaximum { get; set; }
        [DataMember]
        public int YMaximum { get; set; }

        [DataMember]
        public int Layer { get; set; }

        [Obsolete]
        [DataMember]
        public string OldCustomHTML { get; set; }

        [JsonIgnore]
        public virtual bool JQuery { get { return false; } }

        [JsonIgnore]
        public bool PositionTypeIsPercentage { get { return this.PositionType == OverlayPositionV3Type.Simple || this.PositionType == OverlayPositionV3Type.Percentage; } }
        [JsonIgnore]
        public string PositionTypeUnit { get { return this.PositionTypeIsPercentage ? "%" : "px"; } }

        [JsonIgnore]
        public int XTranslation { get { return this.PositionTypeIsPercentage ? -50 : 0; } }
        [JsonIgnore]
        public int YTranslation { get { return this.PositionTypeIsPercentage ? -50 : 0; } }

        [JsonIgnore]
        public string SingleWidgetURL
        {
            get
            {
                OverlayEndpointV3Service widgetEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.ID);
                if (widgetEndpoint != null)
                {
                    return widgetEndpoint.HttpAddress;
                }
                return null;
            }
        }

        [JsonIgnore]
        public virtual bool IsResettable { get { return false; } }

        [JsonIgnore]
        public bool IsLivePreview { get; set; }

        [JsonIgnore]
        public virtual int LatestVersion { get { return 0; } }

        protected OverlayItemV3ModelBase()
        {
            this.Version = this.LatestVersion;
        }

        public OverlayItemV3ModelBase(OverlayItemV3Type type) { this.Type = type; }

        public virtual Task Initialize() { return Task.CompletedTask; }

        public virtual Task Uninitialize() { return Task.CompletedTask; }

        public virtual Task Reset() { return Task.CompletedTask; }

        public virtual void ImportReset()
        {
            this.ID = Guid.NewGuid();
        }

        public virtual Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[nameof(this.ID)] = (this.ID == Guid.Empty) ? Guid.NewGuid().ToString() : this.ID.ToString();
            properties[nameof(this.Width)] = (this.Width > 0) ? $"{this.Width}px" : "max-content";
            properties[nameof(this.Height)] = (this.Height > 0) ? $"{this.Height}px" : "max-content";

            if (this.PositionType == OverlayPositionV3Type.Random)
            {
                int x = RandomHelper.GenerateRandomNumber(this.XPosition, this.XMaximum);
                int y = RandomHelper.GenerateRandomNumber(this.YPosition, this.YMaximum);
                properties[nameof(this.XPosition)] = x;
                properties[nameof(this.YPosition)] = y;
            }
            else
            {
                properties[nameof(this.XPosition)] = this.XPosition;
                properties[nameof(this.YPosition)] = this.YPosition;
            }

            properties[nameof(this.PositionType)] = this.PositionType;
            properties[nameof(this.PositionTypeUnit)] = this.PositionTypeUnit;
            properties[nameof(this.XTranslation)] = this.XTranslation;
            properties[nameof(this.YTranslation)] = this.YTranslation;
            properties[nameof(this.Layer)] = this.Layer;

            properties[nameof(this.JQuery)] = this.JQuery ? "<script src=\"/scripts/jquery-3.6.0.min.js\"></script>" : string.Empty;

            properties[nameof(this.IsLivePreview)] = this.IsLivePreview.ToString().ToLower();

            return properties;
        }

        public virtual Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters) { return Task.CompletedTask; }

        public OverlayEndpointV3Service GetOverlayEndpointService()
        {
            if (this.DisplayOption == OverlayItemV3DisplayOptionsType.OverlayEndpoint)
            {
                return ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
            }
            else if (this.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL)
            {
                return ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.ID);
            }
            return null;
        }

        public async Task CallFunction(string functionName, Dictionary<string, object> data)
        {
            OverlayEndpointV3Service overlay = this.GetOverlayEndpointService();
            if (overlay != null)
            {
                await overlay.Function(this.ID.ToString(), functionName, data);
            }
        }

        public virtual async Task ProcessPacket(OverlayV3Packet packet)
        {
            if (string.Equals(packet.Type, OverlayWidgetV3Model.WidgetLoadedPacketType))
            {
                await this.Loaded();
                this.WidgetLoaded(this, new EventArgs());
            }
        }

        protected virtual Task Loaded() { return Task.CompletedTask; }
    }
}
