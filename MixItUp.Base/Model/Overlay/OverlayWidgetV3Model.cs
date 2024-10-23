using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay.Widgets
{
    [DataContract]
    public class OverlayWidgetV3Model
    {
        public const string WidgetLoadedPacketType = "WidgetLoaded";

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public OverlayItemV3ModelBase Item { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public int RefreshTime { get; set; }

        [JsonIgnore]
        public Guid ID { get { return this.Item.ID; } }
        [JsonIgnore]
        public OverlayItemV3Type Type { get { return this.Item.Type; } }
        [JsonIgnore]
        public Guid OverlayEndpointID { get { return this.Item.OverlayEndpointID; } }

        [JsonIgnore]
        public string SingleWidgetURL { get { return this.Item.SingleWidgetURL; } }
        [JsonIgnore]
        public bool IsResettable { get { return this.Item.IsResettable; } }

        [JsonIgnore]
        private SemaphoreSlim stateSemaphore = new SemaphoreSlim(1); 

        private CancellationTokenSource refreshCancellationTokenSource;

        public OverlayWidgetV3Model(OverlayItemV3ModelBase item)
        {
            this.Item = item;
        }

        [Obsolete]
        public OverlayWidgetV3Model() { }

        public async Task Enable()
        {
            await this.stateSemaphore.WaitAsync();

            if (!this.IsEnabled)
            {
                this.IsEnabled = true;

#pragma warning disable CS0612 // Type or member is obsolete
                await this.Initialize();
#pragma warning restore CS0612 // Type or member is obsolete
            }

            this.stateSemaphore.Release();
        }

        public async Task Disable()
        {
            await this.stateSemaphore.WaitAsync();

            if (this.IsEnabled)
            {
                this.IsEnabled = false;

#pragma warning disable CS0612 // Type or member is obsolete
                await this.Uninitialize();
#pragma warning restore CS0612 // Type or member is obsolete
            }

            this.stateSemaphore.Release();
        }

        public async Task Reset()
        {
            bool isEnabled = this.IsEnabled;

            await this.Disable();

            await this.Item.Reset();

            if (isEnabled)
            {
                await this.Enable();
            }
        }

        public async Task SendInitial()
        {
            OverlayEndpointV3Service overlay = this.GetOverlayEndpointService();
            if (overlay != null)
            {
                Dictionary<string, object> properties = this.Item.GetGenerationProperties();

                string iframeHTML = overlay.GetItemIFrameHTML();
                iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.Item.HTML), this.Item.HTML);
                iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.Item.CSS), this.Item.CSS);
                iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.Item.Javascript), this.Item.Javascript);

                CommandParametersModel parametersModel = new CommandParametersModel();

                await this.Item.ProcessGenerationProperties(properties, parametersModel);
                foreach (var property in properties)
                {
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, property.Key, property.Value);
                }

                iframeHTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(iframeHTML, parametersModel);

                await overlay.Add(this.ID.ToString(), iframeHTML, this.Item.Layer);
            }
        }

        public OverlayEndpointV3Service GetOverlayEndpointService() { return this.Item.GetOverlayEndpointService(); }

        [Obsolete]
        internal async Task Initialize()
        {
            if (this.RefreshTime > 0)
            {
                this.Item.WidgetLoaded += Item_WidgetLoaded;
            }

            await this.Item.Initialize();
            await this.SendInitial();
        }

        [Obsolete]
        internal async Task Uninitialize()
        {
            OverlayEndpointV3Service overlay = this.GetOverlayEndpointService();
            if (overlay != null)
            {
                await overlay.Remove(this.ID.ToString());
            }

            this.Item.WidgetLoaded -= Item_WidgetLoaded;

            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
                this.refreshCancellationTokenSource = null;
            }

            await this.Item.Uninitialize();
        }

        private void Item_WidgetLoaded(object sender, EventArgs e)
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

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        CommandParametersModel parameters = new CommandParametersModel();
                        Dictionary<string, object> data = this.Item.GetGenerationProperties();
                        foreach (string key in data.Keys.ToList())
                        {
                            if (data[key] != null)
                            {
                                data[key] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(data[key].ToString(), parameters);
                            }
                        }
                        await this.Item.ProcessGenerationProperties(data, new CommandParametersModel());
                        await this.Item.CallFunction("update", data);
                    }

                } while (!cancellationToken.IsCancellationRequested);

            }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
