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
        public int XTranslation { get; set; }
        [DataMember]
        public int YTranslation { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public string Duration { get; set; }

        //[DataMember]
        //public List<OverlayAnimationV3Model> Animations { get; set; } = new List<OverlayAnimationV3Model>();

        protected OverlayItemV3ModelBase() { }

        public OverlayItemV3ModelBase(OverlayItemV3Type type) { this.Type = type; }

        public async Task<OverlayOutputV3Model> GenerateOutput(CommandParametersModel parameters)
        {
            OverlayOutputV3Model output = new OverlayOutputV3Model();

            output.ID = this.ID;
            if (output.ID == Guid.Empty)
            {
                output.ID = Guid.NewGuid();
            }

            output.HTML = this.HTML;
            output.CSS = this.CSS;
            output.Javascript = this.Javascript;

            string id = output.ID.ToString();
            output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, nameof(this.ID), id);
            output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, nameof(this.ID), id);
            output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, nameof(this.ID), id);

            this.Position.SetPosition(output);

            output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, nameof(this.XTranslation), this.XTranslation.ToString());
            output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, nameof(this.XTranslation), this.XTranslation.ToString());
            output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, nameof(this.XTranslation), this.XTranslation.ToString());

            output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, nameof(this.YTranslation), this.YTranslation.ToString());
            output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, nameof(this.YTranslation), this.YTranslation.ToString());
            output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, nameof(this.YTranslation), this.YTranslation.ToString());

            if (this.Width > 0)
            {
                output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, nameof(this.Width), $"{this.Width}px");
                output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, nameof(this.Width), $"{this.Width}px");
                output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, nameof(this.Width), $"{this.Width}px");
            }
            else
            {
                output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, nameof(this.Width), "auto");
                output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, nameof(this.Width), "auto");
                output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, nameof(this.Width), "auto");
            }

            if (this.Height > 0)
            {
                output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, nameof(this.Height), $"{this.Height}px");
                output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, nameof(this.Height), $"{this.Height}px");
                output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, nameof(this.Height), $"{this.Height}px");
            }
            else
            {
                output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, nameof(this.Height), "auto");
                output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, nameof(this.Height), "auto");
                output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, nameof(this.Height), "auto");
            }

            string duration = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Duration, parameters);

            output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, nameof(this.Duration), duration);
            output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, nameof(this.Duration), duration);
            output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, nameof(this.Duration), duration);

            //string animationJavascript = string.Empty;
            //foreach (var animation in this.Animations)
            //{
            //    result.Animations.Add(animation);
            //}
            //result.Javascript = animationJavascript + "\n\n" + result.Javascript;

            foreach (var kvp in await this.GetCustomProperties(parameters))
            {
                output.HTML = OverlayV3Service.ReplaceProperty(output.HTML, kvp.Key, kvp.Value);
                output.CSS = OverlayV3Service.ReplaceProperty(output.CSS, kvp.Key, kvp.Value);
                output.Javascript = OverlayV3Service.ReplaceProperty(output.Javascript, kvp.Key, kvp.Value);
            }

            output.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(output.HTML, parameters);
            output.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(output.CSS, parameters);
            output.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(output.Javascript, parameters);

            return output;
        }

        protected virtual Task<Dictionary<string, string>> GetCustomProperties(CommandParametersModel parameters)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }
}
