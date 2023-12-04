using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayItemV3Type
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        Timer,
        Label,
        TwitchClip,
    }

    [DataContract]
    public abstract class OverlayItemV3ModelBase : OverlayOutputV3Model
    {
        public const string InnerHTMLProperty = "InnerHTML";

        public static readonly string PositionedHTML = OverlayResources.OverlayPositionedItemDefaultHTML;
        public static readonly string PositionedCSS = OverlayResources.OverlayPositionedItemDefaultCSS;

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
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        public OverlayItemV3Type Type { get; set; }

        [DataMember]
        public OverlayPositionV3Model Position { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        protected OverlayItemV3ModelBase() { }

        public OverlayItemV3ModelBase(OverlayItemV3Type type) { this.Type = type; }

        public virtual Dictionary<string, string> GetGenerationProperties()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[nameof(this.ID)] = (this.ID == Guid.Empty) ? Guid.NewGuid().ToString() : this.ID.ToString();
            properties[nameof(this.Width)] = (this.Width > 0) ? $"{this.Width}px" : "max-content";
            properties[nameof(this.Height)] = (this.Height > 0) ? $"{this.Height}px" : "max-content";
            this.Position.SetPositionProperties(properties);
            return properties;
        }

        public virtual Task ProcessGenerationProperties(Dictionary<string, string> properties, CommandParametersModel parameters) { return Task.CompletedTask; }

        public async Task WidgetEnable()
        {
            await this.WidgetEnableInternal();

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                Dictionary<string, string> properties = this.GetGenerationProperties();

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

                await overlay.Add(this.ID.ToString(), iframeHTML);
            }
        }

        public async Task WidgetDisable()
        {
            await this.WidgetDisableInternal();

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.Remove(this.ID.ToString());
            }
        }

        protected virtual Task WidgetEnableInternal() { return Task.CompletedTask; }

        protected virtual Task WidgetDisableInternal() { return Task.CompletedTask; }

        protected async Task CallFunction(string functionName, Dictionary<string, string> data)
        {
            // TODO: Change to support different overlay endpoints or direct URLs
            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.Function(this.ID.ToString(), functionName, data);
            }
        }
    }
}
