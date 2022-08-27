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
        Timer,
    }

    [DataContract]
    public abstract class OverlayItemV3ModelBase : OverlayOutputV3Model
    {
        public const string InnerHTMLProperty = "InnerHTML";

        public static readonly string PositionedHTML = "<div id=\"outerdiv-{ID}\">" + Environment.NewLine +
            "<div id=\"innerdiv-{ID}\">" + Environment.NewLine +
            Environment.NewLine +
            "{InnerHTML}" + Environment.NewLine +
            Environment.NewLine +
            "</div>" + Environment.NewLine +
            "</div>";

        public static readonly string PositionedCSS = "#outerdiv-{ID} {" + Environment.NewLine +
            "    position: absolute;" + Environment.NewLine +
            "    width: 100%;" + Environment.NewLine +
            "    max-width: 100%;" + Environment.NewLine +
            "    min-width: 100%;" + Environment.NewLine +
            "    height: 100%;" + Environment.NewLine +
            "    max-height: 100%;" + Environment.NewLine +
            "    min-height: 100%;" + Environment.NewLine +
            "    margin: 0px;" + Environment.NewLine +
            "    z-index: {Layer};" + Environment.NewLine +
            "}" + Environment.NewLine +
            Environment.NewLine +
            "#innerdiv-{ID} {" + Environment.NewLine +
            "    position: absolute;" + Environment.NewLine +
            "    margin: 0px;" + Environment.NewLine +
            "    left: {XPosition}{PositionType};" + Environment.NewLine +
            "    top: {YPosition}{PositionType};" + Environment.NewLine +
            "    transform: translate({XTranslation}%, {YTranslation}%);" + Environment.NewLine +
            "    width: {Width};" + Environment.NewLine +
            "    height: {Height};" + Environment.NewLine +
            "}" + Environment.NewLine + Environment.NewLine;

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
            result.ID = "X" + Guid.NewGuid().ToString().Replace('-', 'X');
            result.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.HTML, parameters);
            result.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.CSS, parameters);
            result.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Javascript, parameters);

            result.HTML = ReplaceProperty(result.HTML, "ID", result.ID);
            result.CSS = ReplaceProperty(result.CSS, "ID", result.ID);
            result.Javascript = ReplaceProperty(result.Javascript, "ID", result.ID);

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
            result.Javascript = ReplaceProperty(result.Javascript, "XTranslation", this.XTranslation.ToString());

            result.HTML = ReplaceProperty(result.HTML, "YTranslation", this.YTranslation.ToString());
            result.CSS = ReplaceProperty(result.CSS, "YTranslation", this.YTranslation.ToString());
            result.Javascript = ReplaceProperty(result.Javascript, "YTranslation", this.YTranslation.ToString());

            if (this.Width > 0)
            {
                result.HTML = ReplaceProperty(result.HTML, "Width", $"{this.Width}px");
                result.CSS = ReplaceProperty(result.CSS, "Width", $"{this.Width}px");
                result.Javascript = ReplaceProperty(result.Javascript, "Width", $"{this.Width}px");
            }
            else
            {
                result.HTML = ReplaceProperty(result.HTML, "Width", "auto");
                result.CSS = ReplaceProperty(result.CSS, "Width", "auto");
                result.Javascript = ReplaceProperty(result.Javascript, "Width", "auto");
            }

            if (this.Height > 0)
            {
                result.HTML = ReplaceProperty(result.HTML, "Height", $"{this.Height}px");
                result.CSS = ReplaceProperty(result.CSS, "Height", $"{this.Height}px");
                result.Javascript = ReplaceProperty(result.Javascript, "Height", $"{this.Height}px");
            }
            else
            {
                result.HTML = ReplaceProperty(result.HTML, "Height", "auto");
                result.CSS = ReplaceProperty(result.CSS, "Height", "auto");
                result.Javascript = ReplaceProperty(result.Javascript, "Height", "auto");
            }

            result.Duration = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Duration, parameters);
            result.HTML = ReplaceProperty(result.HTML, "Duration", result.Duration);
            result.CSS = ReplaceProperty(result.CSS, "Duration", result.Duration);
            result.Javascript = ReplaceProperty(result.Javascript, "Duration", result.Duration);

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
