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
        [DataMember]
        public bool IsEnabled { get; set; }

        private CancellationTokenSource refreshCancellationTokenSource;

        protected OverlayItemV3ModelBase() { }

        public OverlayItemV3ModelBase(OverlayItemV3Type type) { this.Type = type; }

        public virtual async Task Enable()
        {
            this.IsEnabled = true;

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

            this.IsEnabled = false;
        }

        public virtual async Task Test()
        {
            await this.Enable();
            await this.TestInternal();
            await this.Disable();
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
                result.Animations[animation.Key] = animation.Value;
                animation.Value.ApplyAnimationReplacements(result);
            }

            result = await this.GetProcessedItem(result, overlayEndpointService, parameters);

            result.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.HTML, parameters);
            result.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.CSS, parameters);
            result.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(result.Javascript, parameters);

            return result;
        }

        protected virtual async Task TestInternal()
        {
            await Task.Delay(5000);
        }

        protected virtual Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            return Task.FromResult(item);
        }
    }
}
