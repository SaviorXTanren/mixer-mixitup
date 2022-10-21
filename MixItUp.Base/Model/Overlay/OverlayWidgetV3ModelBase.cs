using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public abstract class OverlayWidgetV3ModelBase
    {
        [DataMember]
        public string ID { get; set; } = OverlayItemV3ModelBase.GenerateOverlayItemID();

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        public OverlayItemV3ModelBase Item { get; set; }

        [DataMember]
        public OverlayItemV3Type Type { get { return this.Item.Type; } }

        [DataMember]
        public Dictionary<string, string> CurrentReplacements { get; set; } = new Dictionary<string, string>();

        public OverlayWidgetV3ModelBase(string name, Guid overlayEndpointID, OverlayItemV3ModelBase item)
        {
            this.Name = name;
            this.OverlayEndpointID = overlayEndpointID;
            this.Item = item;

            this.Item.ID = this.ID;
        }

        [Obsolete]
        public OverlayWidgetV3ModelBase() { }

        public async Task Enable()
        {
            await this.EnableInternal();

            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.EnableWidget(this, new CommandParametersModel());
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
            await this.DisableInternal();

            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.DisableWidget(this, new CommandParametersModel());
            }
        }

        public abstract Task<OverlayOutputV3Model> GetProcessedItem(OverlayEndpointService overlayEndpointService, CommandParametersModel parameters);

        protected virtual Task EnableInternal() { return Task.CompletedTask; }

        protected virtual Task DisableInternal() { return Task.CompletedTask; }
    }
}