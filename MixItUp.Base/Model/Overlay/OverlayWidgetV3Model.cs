using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayWidgetV3Model
    {
        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        public OverlayItemV3ModelBase Item { get; set; }

        [DataMember]
        public int RefreshTime { get; set; }

        public async Task Enable()
        {
            await this.Item.Enable();

            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.EnableWidget(this, new CommandParametersModel());
            }
        }

        public async Task Refresh()
        {
            CommandParametersModel parameters = new CommandParametersModel();

            await this.Item.Update(parameters);

            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                //await overlay.UpdateItem(this.Item, parameters);
            }
        }

        public async Task Disable()
        {
            await this.Item.Disable();

            OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                await overlay.DisableWidget(this, new CommandParametersModel());
            }
        }
    }
}