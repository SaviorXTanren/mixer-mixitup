using MixItUp.Base.Util;
using System;
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
        public int RefreshTime { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }

        [JsonIgnore]
        public bool IsInitialized { get; set; }

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
        public bool IsTestable { get { return this.Item.IsTestable; } }

        private CancellationTokenSource refreshCancellationTokenSource;

        public OverlayWidgetV3Model(OverlayItemV3ModelBase item)
        {
            this.Item = item;
        }

        [Obsolete]
        public OverlayWidgetV3Model() { }

        public async Task Initialize()
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
                await this.Item.WidgetInitialize();
            }
        }

        public async Task Enable()
        {
            await this.Initialize();

            if (!this.IsEnabled)
            {
                this.IsEnabled = true;
#pragma warning disable CS0612 // Type or member is obsolete
                await this.EnableInternal();
#pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        public async Task Disable()
        {
            if (this.IsEnabled)
            {
                this.IsEnabled = false;

                await this.Item.WidgetDisable();

                this.Item.LoadedInWidget -= Item_LoadedInWidget;

                if (this.refreshCancellationTokenSource != null)
                {
                    this.refreshCancellationTokenSource.Cancel();
                }
                this.refreshCancellationTokenSource = null;
            }
        }

        public async Task WidgetFullReset()
        {
            await this.Item.WidgetFullReset();
        }

        public async Task SendInitial()
        {
            await this.Item.WidgetSendInitial();
        }

        [Obsolete]
        internal async Task EnableInternal()
        {
            await this.Item.WidgetEnable();

            if (this.RefreshTime > 0)
            {
                this.Item.LoadedInWidget += Item_LoadedInWidget;
            }
        }

        private void Item_LoadedInWidget(object sender, EventArgs e)
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
                        await this.Item.WidgetUpdate();
                    }

                } while (!cancellationToken.IsCancellationRequested);

            }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
