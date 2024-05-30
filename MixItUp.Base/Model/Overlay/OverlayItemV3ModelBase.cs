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
        public const string MainDivElement = "document.getElementById('maindiv')";

        public const string InnerHTMLProperty = "InnerHTML";

        public const string UserProperty = "User";

        public static int zIndexCounter = 0;

        public static readonly string PositionedHTML = OverlayResources.OverlayPositionedItemDefaultHTML;
        public static readonly string PositionedCSS = OverlayResources.OverlayPositionedItemDefaultCSS;

        public event EventHandler LoadedInWidget = delegate { };

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
        public virtual bool IsTestable { get { return false; } }
        [JsonIgnore]
        public virtual bool IsResettable { get { return false; } }

        [JsonIgnore]
        public bool IsLivePreview { get; set; }

        [JsonIgnore]
        public virtual int LatestVersion { get { return 0; } }

        [JsonIgnore]
        public int LayerProcessed
        {
            get
            {
                if (this.Layer == 0)
                {
                    return OverlayItemV3ModelBase.zIndexCounter;
                }
                return this.Layer;
            }
        }

        protected OverlayItemV3ModelBase()
        {
            this.Version = this.LatestVersion;
        }

        public OverlayItemV3ModelBase(OverlayItemV3Type type) { this.Type = type; }

        public virtual Dictionary<string, object> GetGenerationProperties()
        {
            if (this.Layer == 0)
            {
                OverlayItemV3ModelBase.zIndexCounter++;
            }

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
            properties[nameof(this.Layer)] = this.LayerProcessed;

#pragma warning disable CS0612 // Type or member is obsolete
            properties[nameof(this.IsLivePreview)] = this.IsLivePreview.ToString().ToLower();
#pragma warning restore CS0612 // Type or member is obsolete

            return properties;
        }

        public virtual Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters) { return Task.CompletedTask; }

        public async Task WidgetInitialize()
        {
            await this.WidgetInitializeInternal();
        }

        public async Task WidgetEnable()
        {
            await this.WidgetEnableInternal();

            await this.WidgetSendInitial();
        }

        public async Task WidgetDisable()
        {
            await this.WidgetDisableInternal();

            OverlayEndpointV3Service overlay = this.GetOverlayEndpointService();
            if (overlay != null)
            {
                await overlay.Remove(this.ID.ToString());
            }
        }

        public async Task WidgetReset()
        {
            await this.WidgetResetInternal();
        }

        public async Task WidgetFullReset()
        {
            await this.WidgetReset();

            await this.WidgetDisable();

            await this.WidgetEnable();
        }

        public async Task WidgetUpdate()
        {
            CommandParametersModel parameters = new CommandParametersModel();
            Dictionary<string, object> data = this.GetGenerationProperties();
            foreach (string key in data.Keys.ToList())
            {
                if (data[key] != null)
                {
                    data[key] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(data[key].ToString(), parameters);
                }
            }
            await this.ProcessGenerationProperties(data, new CommandParametersModel());
            await this.CallFunction("update", data);
        }

        public async Task WidgetSendInitial()
        {
            OverlayEndpointV3Service overlay = this.GetOverlayEndpointService();
            if (overlay != null)
            {
                Dictionary<string, object> properties = this.GetGenerationProperties();

                string iframeHTML = overlay.GetItemIFrameHTML();
                iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.HTML), this.HTML);
                iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.CSS), this.CSS);
                iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.Javascript), this.Javascript);

                CommandParametersModel parametersModel = new CommandParametersModel();

                await this.ProcessGenerationProperties(properties, parametersModel);
                foreach (var property in properties)
                {
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, property.Key, property.Value);
                }

                iframeHTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(iframeHTML, parametersModel);

                await overlay.Add(this.ID.ToString(), iframeHTML, this.LayerProcessed);
            }
        }

        public async Task CallFunction(string functionName, Dictionary<string, object> data)
        {
            OverlayEndpointV3Service overlay = this.GetOverlayEndpointService();
            if (overlay != null)
            {
                await overlay.Function(this.ID.ToString(), functionName, data);
            }
        }

        public virtual Task ProcessPacket(OverlayV3Packet packet)
        {
            if (string.Equals(packet.Type, OverlayWidgetV3Model.WidgetLoadedPacketType))
            {
                this.LoadedInWidget(this, new EventArgs());
            }
            return Task.CompletedTask;
        }

        protected virtual Task WidgetInitializeInternal() { return Task.CompletedTask; }

        protected virtual Task WidgetEnableInternal() { return Task.CompletedTask; }

        protected virtual Task WidgetDisableInternal() { return Task.CompletedTask; }

        protected virtual Task WidgetResetInternal() { return Task.CompletedTask; }

        protected virtual Task<Dictionary<string, object>> WidgetUpdateInternal()
        {
            return Task.FromResult(new Dictionary<string, object>());
        }

        protected OverlayEndpointV3Service GetOverlayEndpointService()
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
    }
}
