using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayWidgetV3Type
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        WebPage,
        Timer,
        Label,
    }

    [DataContract]
    public abstract class OverlayWidgetV3ModelBase
    {
        [DataMember]
        public string ID { get; set; } = OverlayItemV3ModelBase.GenerateOverlayItemID();

        [DataMember]
        public OverlayWidgetV3Type Type { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        public Dictionary<string, string> CurrentReplacements { get; set; } = new Dictionary<string, string>();

        public OverlayWidgetV3ModelBase(OverlayWidgetV3Type type, string name, Guid overlayEndpointID)
        {
            this.Type = type;
            this.Name = name;
            this.OverlayEndpointID = overlayEndpointID;
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

        public async Task Update()
        {
            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.UpdateWidget(this, new CommandParametersModel());
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