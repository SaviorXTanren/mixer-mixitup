using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayWidgetV3Model
    {
        [DataMember]
        public OverlayItemV3ModelBase Item { get; set; }

        [DataMember]
        public int RefreshTime { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }

        public Guid ID { get { return this.Item.ID; } }
        public string Name { get { return this.Item.Name; } }

        private CancellationTokenSource refreshCancellationTokenSource;

        public OverlayWidgetV3Model(OverlayItemV3ModelBase item)
        {
            this.Item = item;
            this.IsEnabled = true;
        }

        [Obsolete]
        public OverlayWidgetV3Model() { }

        public async Task Enable()
        {
            this.IsEnabled = true;

            await this.Item.Enable();

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.Item.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.SendAdd(this.Item, new CommandParametersModel());
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

                        await this.Item.Update("Update", null, null);

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async Task Disable()
        {
            await this.Item.Disable();

            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
            }
            this.refreshCancellationTokenSource = null;

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.Item.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.SendRemove(this.Item);
            }

            this.IsEnabled = false;
        }

        public virtual async Task Test()
        {
            await this.Enable();
            await this.Item.Test();
            await this.Disable();
        }
    }
}
