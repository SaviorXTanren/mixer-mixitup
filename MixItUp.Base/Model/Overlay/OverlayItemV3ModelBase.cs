using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
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
        Label,
        EventList
    }

    [DataContract]
    public abstract class OverlayItemV3ModelBase : OverlayOutputV3Model
    {
        public const string InnerHTMLProperty = "InnerHTML";

        public static readonly string PositionedHTML = Resources.OverlayPositionedItemDefaultHTML;
        public static readonly string PositionedCSS = Resources.OverlayPositionedItemDefaultCSS;

        public static int zIndexCounter = 0;

        public static string ReplaceProperty(string text, string name, string value) { return text.Replace($"{{{name}}}", value ?? string.Empty); }

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

        [DataMember]
        public int RefreshTime { get; set; }

        private CancellationTokenSource refreshCancellationTokenSource;

        protected OverlayItemV3ModelBase() { }

        public OverlayItemV3ModelBase(OverlayItemV3Type type) { this.Type = type; }

        public virtual async Task Enable()
        {
            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.SendItem("Enable", this, new CommandParametersModel());
            }

            if (this.RefreshTime > 0)
            {
                if (this.refreshCancellationTokenSource != null)
                {
                    this.refreshCancellationTokenSource.Cancel();
                }
                this.refreshCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    do
                    {
                        await Task.Delay(1000 * this.RefreshTime);

                        await this.Update("Update", null, new CommandParametersModel());

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public virtual async Task Update(string type, Dictionary<string, string> data, CommandParametersModel parameters)
        {
            JObject jobj = new JObject();
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    jobj[kvp.Key] = kvp.Value;
                }
            }

            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.SendJObject(type, this.TextID, jobj, parameters);
            }
        }

        public virtual async Task Disable()
        {
            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
            }
            this.refreshCancellationTokenSource = null;

            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.SendItem("Disable", this, new CommandParametersModel());
            }
        }

        public async Task<OverlayOutputV3Model> GetProcessedItem(OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
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

            string id = result.TextID;
            result.HTML = ReplaceProperty(result.HTML, "ID", id);
            result.CSS = ReplaceProperty(result.CSS, "ID", id);
            result.Javascript = ReplaceProperty(result.Javascript, "ID", id);

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

            result = await this.GetProcessedItem(result, overlayEndpointService, parameters);

            result.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.HTML, parameters);
            result.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.CSS, parameters);
            result.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.Javascript, parameters);

            return result;
        }

        protected virtual Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            return Task.FromResult(item);
        }
    }
}
