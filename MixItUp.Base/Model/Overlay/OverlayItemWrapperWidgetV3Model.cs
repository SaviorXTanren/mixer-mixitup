using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayItemWrapperWidgetV3Model : OverlayWidgetV3ModelBase
    {
        [DataMember]
        public int RefreshTime { get; set; }

        private CancellationTokenSource refreshCancellationTokenSource;

        public OverlayItemWrapperWidgetV3Model(string name, Guid overlayEndpointID, OverlayItemV3ModelBase item, int refreshTime)
            : base(name, overlayEndpointID, item)
        {
            this.Item = item;
            this.RefreshTime = refreshTime;

            this.Item.ID = this.ID;
        }

        [Obsolete]
        public OverlayItemWrapperWidgetV3Model() { }

        public override Task<OverlayOutputV3Model> GetProcessedItem(OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            return this.Item.GetProcessedItem(overlayEndpointService, parameters, this.CurrentReplacements);
        }

        protected override Task EnableInternal()
        {
            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
            }
            this.refreshCancellationTokenSource = new CancellationTokenSource();

            AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
            {
                do
                {
                    await this.Update("UpdateStatic", new JObject());

                    await Task.Delay(1000 * this.RefreshTime);

                } while (!cancellationToken.IsCancellationRequested);

            }, this.refreshCancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        protected override Task DisableInternal()
        {
            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
            }
            this.refreshCancellationTokenSource = null;

            return Task.CompletedTask;
        }
    }
}
