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
    [DataContract]
    public class OverlayWidgetV3Model
    {
        [DataMember]
        public string ID { get { return this.Item.ID; } }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        public OverlayItemV3ModelBase Item { get; set; }

        [DataMember]
        public OverlayItemV3Type Type { get { return this.Item.Type; } }

        [DataMember]
        public int RefreshTime { get; set; }

        [DataMember]
        public Dictionary<string, string> CurrentReplacements { get; set; } = new Dictionary<string, string>();

        private CancellationTokenSource refreshCancellationTokenSource;

        public OverlayWidgetV3Model(string id, string name, Guid overlayEndpointID, OverlayItemV3ModelBase item)
        {
            this.Name = name;
            this.OverlayEndpointID = overlayEndpointID;
            this.Item = item;

            this.Item.ID = this.ID;
        }

        [Obsolete]
        public OverlayWidgetV3Model() { }

        public async Task Enable()
        {
            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.SendItem("Enable", this.Item, new CommandParametersModel());
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

                        await this.Update("Update", new JObject());

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async Task Update(string type, JObject data)
        {
            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.UpdateWidget(this, type, data, new CommandParametersModel());
            }
        }

        public async Task Disable()
        {
            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
            }
            this.refreshCancellationTokenSource = null;

            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.SendItem("Disable", this.Item, new CommandParametersModel());
            }
        }
    }
}