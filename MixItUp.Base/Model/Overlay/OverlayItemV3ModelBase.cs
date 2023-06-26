using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
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
        EventList,
        Goal,
        Chat,
    }

    [DataContract]
    public abstract class OverlayItemV3ModelBase : OverlayOutputV3Model
    {
        public const string InnerHTMLProperty = "InnerHTML";

        public static readonly string PositionedHTML = Resources.OverlayPositionedItemDefaultHTML;
        public static readonly string PositionedCSS = Resources.OverlayPositionedItemDefaultCSS;

        public static int zIndexCounter = 0;

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid OverlayEndpointID { get; set; }

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

        protected OverlayItemV3ModelBase() { }

        public OverlayItemV3ModelBase(OverlayItemV3Type type) { this.Type = type; }

        public virtual async Task Update(string functionName, Dictionary<string, string> data, CommandParametersModel parameters)
        {
            JObject jobj = new JObject();
            jobj[nameof(this.ID)] = this.TextID;
            jobj["FunctionName"] = functionName;
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    jobj[kvp.Key] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(kvp.Value, parameters);
                }
            }

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.SendUpdate(this, jobj);
            }
        }

        public async Task<OverlayOutputV3Model> GetProcessedItem(CommandParametersModel parameters)
        {
            OverlayOutputV3Model result = new OverlayOutputV3Model();

            result.ID = this.ID;
            if (result.ID == Guid.Empty)
            {
                result.ID = Guid.NewGuid();
            }

            result.HTML = this.HTML;
            result.CSS = this.CSS;
            result.Javascript = this.Javascript;

            string id = result.ID.ToString();
            result.HTML = ReplaceProperty(result.HTML, nameof(this.ID), id);
            result.CSS = ReplaceProperty(result.CSS, nameof(this.ID), id);
            result.Javascript = ReplaceProperty(result.Javascript, nameof(this.ID), id);

            if (this.Layer == 0)
            {
                zIndexCounter++;
                result.HTML = ReplaceProperty(result.HTML, nameof(this.Layer), zIndexCounter.ToString());
                result.CSS = ReplaceProperty(result.CSS, nameof(this.Layer), zIndexCounter.ToString());
                result.Javascript = ReplaceProperty(result.Javascript, nameof(this.Layer), zIndexCounter.ToString());
            }
            else
            {
                result.HTML = ReplaceProperty(result.HTML, nameof(this.Layer), this.Layer.ToString());
                result.CSS = ReplaceProperty(result.CSS, nameof(this.Layer), this.Layer.ToString());
                result.Javascript = ReplaceProperty(result.Javascript, nameof(this.Layer), this.Layer.ToString());
            }

            result.HTML = ReplaceProperty(result.HTML, nameof(this.XPosition), this.XPosition.ToString());
            result.CSS = ReplaceProperty(result.CSS, nameof(this.XPosition), this.XPosition.ToString());
            result.Javascript = ReplaceProperty(result.Javascript, nameof(this.XPosition), this.XPosition.ToString());

            result.HTML = ReplaceProperty(result.HTML, nameof(this.YPosition), this.YPosition.ToString());
            result.CSS = ReplaceProperty(result.CSS, nameof(this.YPosition), this.YPosition.ToString());
            result.Javascript = ReplaceProperty(result.Javascript, nameof(this.YPosition), this.YPosition.ToString());

            result.HTML = ReplaceProperty(result.HTML, "PositionType", this.IsPercentagePosition ? "%" : "px");
            result.CSS = ReplaceProperty(result.CSS, "PositionType", this.IsPercentagePosition ? "%" : "px");
            result.Javascript = ReplaceProperty(result.Javascript, "PositionType", this.IsPercentagePosition ? "%" : "px");

            result.HTML = ReplaceProperty(result.HTML, nameof(this.XTranslation), this.XTranslation.ToString());
            result.CSS = ReplaceProperty(result.CSS, nameof(this.XTranslation), this.XTranslation.ToString());
            result.Javascript = ReplaceProperty(result.Javascript, nameof(this.XTranslation), this.XTranslation.ToString());

            result.HTML = ReplaceProperty(result.HTML, nameof(this.YTranslation), this.YTranslation.ToString());
            result.CSS = ReplaceProperty(result.CSS, nameof(this.YTranslation), this.YTranslation.ToString());
            result.Javascript = ReplaceProperty(result.Javascript, nameof(this.YTranslation), this.YTranslation.ToString());

            if (this.Width > 0)
            {
                result.HTML = ReplaceProperty(result.HTML, nameof(this.Width), $"{this.Width}px");
                result.CSS = ReplaceProperty(result.CSS, nameof(this.Width), $"{this.Width}px");
                result.Javascript = ReplaceProperty(result.Javascript, nameof(this.Width), $"{this.Width}px");
            }
            else
            {
                result.HTML = ReplaceProperty(result.HTML, nameof(this.Width), "auto");
                result.CSS = ReplaceProperty(result.CSS, nameof(this.Width), "auto");
                result.Javascript = ReplaceProperty(result.Javascript, nameof(this.Width), "auto");
            }

            if (this.Height > 0)
            {
                result.HTML = ReplaceProperty(result.HTML, nameof(this.Height), $"{this.Height}px");
                result.CSS = ReplaceProperty(result.CSS, nameof(this.Height), $"{this.Height}px");
                result.Javascript = ReplaceProperty(result.Javascript, nameof(this.Height), $"{this.Height}px");
            }
            else
            {
                result.HTML = ReplaceProperty(result.HTML, nameof(this.Height), "auto");
                result.CSS = ReplaceProperty(result.CSS, nameof(this.Height), "auto");
                result.Javascript = ReplaceProperty(result.Javascript, nameof(this.Height), "auto");
            }

            result.Duration = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Duration, parameters);

            result.HTML = ReplaceProperty(result.HTML, nameof(this.Duration), result.Duration);
            result.CSS = ReplaceProperty(result.CSS, nameof(this.Duration), result.Duration);
            result.Javascript = ReplaceProperty(result.Javascript, nameof(this.Duration), result.Duration);

            foreach (var animation in this.Animations)
            {
                result.Animations.Add(animation);
            }

            result = await this.GetProcessedItem(result, parameters);

            result.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.HTML, parameters);
            result.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.CSS, parameters);
            result.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.Javascript, parameters);

            return result;
        }

        public virtual Task Enable() { return Task.CompletedTask; }

        public virtual Task Disable() { return Task.CompletedTask; }

        public virtual async Task Test() { await Task.Delay(5000); }

        protected virtual Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            return Task.FromResult(item);
        }
    }
}
