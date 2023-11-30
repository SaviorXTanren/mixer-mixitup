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

        public async Task<OverlayOutputV3Model> GenerateOutput(CommandParametersModel parameters)
        {
            OverlayOutputV3Model output = new OverlayOutputV3Model();

            output.ID = this.ID;
            if (output.ID == Guid.Empty)
            {
                output.ID = Guid.NewGuid();
            }

            output.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(output.HTML, parameters);
            output.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(output.CSS, parameters);
            output.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(output.Javascript, parameters);

            return output;
        }
    }
}
