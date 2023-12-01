using Google.Apis.YouTubePartner.v1.Data;
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

        [DataMember]
        public string Name { get; set; }

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
            properties[nameof(this.Width)] = (this.Width > 0) ? $"{this.Width}px" : "auto";
            properties[nameof(this.Height)] = (this.Height > 0) ? $"{this.Height}px" : "auto";
            this.Position.SetPositionProperties(properties);
            return properties;
        }

        public virtual Task ProcessGenerationProperties(Dictionary<string, string> properties, CommandParametersModel parameters) { return Task.CompletedTask; }

        public virtual Task WidgetEnable() { return Task.CompletedTask; }

        public virtual Task WidgetDisable() { return Task.CompletedTask; }

        protected async Task CallFunction(string functionName, CommandParametersModel parameters, Dictionary<string, string> data)
        {
            Dictionary<string, string> dataParameters = new Dictionary<string, string>();
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    if (kvp.Value != null)
                    {
                        dataParameters[kvp.Key] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(kvp.Value, parameters);
                    }
                }
            }

            //OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(OverlayEndpointID);
            //if (overlay != null)
            //{
                //await overlay.Function(this.Item, functionName, dataParameters);
            //}
        }
    }
}
