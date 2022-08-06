using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
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
        WebPage,
    }

    [DataContract]
    public abstract class OverlayItemV3ModelBase : OverlayOutputV3Model
    {
        public const string InnerHTMLProperty = "InnerHTML";
        public const string PositionedHTML = "<div style=\"position: absolute; width: 100%; max-width: 100%; min-width: 100%; height: 100%; max-height: 100%; min-height: 100%; margin: 0px;\"><div style=\"position: absolute; margin: 0px; left: {XPosition}{PositionType}; top: {YPosition}{PositionType}; transform: translate({XTranslation}%, {YTranslation}%); {Width} {Height}\">{InnerHTML}</div></div>";

        public static int zIndexCounter = 0;

        public static string ReplaceProperty(string text, string name, string value)
        {
            return text.Replace($"{{{name}}}", value);
        }

        [DataMember]
        public OverlayItemV3Type Type { get; set; }

        [DataMember]
        public int XPosition { get; set; }
        [DataMember]
        public int YPosition { get; set; }
        [DataMember]
        public bool IsPercentagePosition { get; set; }

        [DataMember]
        public int XTranslation { get; set; }
        [DataMember]
        public int YTranslation { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public int Layer { get; set; }

        private OverlayItemV3ModelBase() { }

        public OverlayItemV3ModelBase(OverlayItemV3Type type) { this.Type = type; }

        public async Task<OverlayOutputV3Model> GetProcessedItem(OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            OverlayOutputV3Model result = new OverlayOutputV3Model();
            result.ID = Guid.NewGuid();
            result.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.HTML, parameters);
            result.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.CSS, parameters);
            result.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Javascript, parameters);

            if (this.Layer == 0)
            {
                zIndexCounter++;
                result.HTML = ReplaceProperty(result.HTML, "Layer", zIndexCounter.ToString());
                result.CSS = ReplaceProperty(result.CSS, "Layer", zIndexCounter.ToString());
                result.Javascript = ReplaceProperty(result.Javascript, "Layer", zIndexCounter.ToString());
            }
            else
            {
                result.HTML = ReplaceProperty(result.HTML, "Layer", this.Layer.ToString());
                result.CSS = ReplaceProperty(result.CSS, "Layer", this.Layer.ToString());
                result.Javascript = ReplaceProperty(result.Javascript, "Layer", this.Layer.ToString());
            }

            result.HTML = ReplaceProperty(result.HTML, "XPosition", this.XPosition.ToString());
            result.CSS = ReplaceProperty(result.CSS, "XPosition", this.XPosition.ToString());
            result.Javascript = ReplaceProperty(result.Javascript, "XPosition", this.XPosition.ToString());

            result.HTML = ReplaceProperty(result.HTML, "YPosition", this.YPosition.ToString());
            result.CSS = ReplaceProperty(result.CSS, "YPosition", this.YPosition.ToString());
            result.Javascript = ReplaceProperty(result.Javascript, "YPosition", this.YPosition.ToString());

            result.HTML = ReplaceProperty(result.HTML, "PositionType", this.IsPercentagePosition ? "%" : "px");
            result.CSS = ReplaceProperty(result.CSS, "PositionType", this.IsPercentagePosition ? "%" : "px");
            result.Javascript = ReplaceProperty(result.Javascript, "PositionType", this.IsPercentagePosition ? "%" : "px");

            result.HTML = ReplaceProperty(result.HTML, "XTranslation", this.XTranslation.ToString());
            result.CSS = ReplaceProperty(result.CSS, "XTranslation", this.XTranslation.ToString());
            result.HTML = ReplaceProperty(result.HTML, "XTranslation", this.XTranslation.ToString());

            result.HTML = ReplaceProperty(result.HTML, "YTranslation", this.YTranslation.ToString());
            result.CSS = ReplaceProperty(result.CSS, "YTranslation", this.YTranslation.ToString());
            result.Javascript = ReplaceProperty(result.Javascript, "YTranslation", this.YTranslation.ToString());

            if (this.Width > 0)
            {
                result.HTML = ReplaceProperty(result.HTML, "Width", $"width: {this.Width}px;");
                result.CSS = ReplaceProperty(result.CSS, "Width", $"{this.Width}px");
                result.Javascript = ReplaceProperty(result.Javascript, "Width", $"{this.Width}px");
            }
            else
            {
                result.HTML = ReplaceProperty(result.HTML, "Width", $"width: auto;");
            }

            if (this.Height > 0)
            {
                result.HTML = ReplaceProperty(result.HTML, "Height", $"height: {this.Height}px;");
                result.CSS = ReplaceProperty(result.CSS, "Height", $"{this.Height}px");
                result.Javascript = ReplaceProperty(result.Javascript, "Height", $"{this.Height}px");
            }
            else
            {
                result.HTML = ReplaceProperty(result.HTML, "Height", $"height: auto;");
            }

            result.Duration = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Duration, parameters);

            result.EntranceAnimation = this.EntranceAnimation;
            result.EntranceAnimation.ApplyAnimationReplacements(result);

            result.VisibleAnimation = this.VisibleAnimation;
            result.VisibleAnimation.ApplyAnimationReplacements(result);

            result.ExitAnimation = this.ExitAnimation;
            result.ExitAnimation.ApplyAnimationReplacements(result);

            return await this.GetProcessedItem(result, overlayEndpointService, parameters);
        }

        protected virtual Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            return Task.FromResult(item);
        }
    }
}
