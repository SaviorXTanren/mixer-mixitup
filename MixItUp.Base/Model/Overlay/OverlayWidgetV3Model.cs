using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay.Widgets
{
    [DataContract]
    public class OverlayWidgetV3Model
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public OverlayItemV3ModelBase Item { get; set; }

        [DataMember]
        public int RefreshTime { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }

        public Guid ID { get { return this.Item.ID; } }
        public OverlayItemV3Type Type { get { return this.Item.Type; } }

        public virtual bool IsTestable { get { return true; } }

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

            await this.Item.WidgetEnable();

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
                        await Task.Delay(1000 * RefreshTime);

                        //await this.CallFunction("Update", null, null);

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async Task Disable()
        {
            await this.Item.WidgetDisable();

            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
            }
            this.refreshCancellationTokenSource = null;

            this.IsEnabled = false;
        }
    }
}
