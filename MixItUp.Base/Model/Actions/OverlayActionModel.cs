using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class OverlayActionModel : ActionModelBase
    {
        [DataMember]
        public string OverlayName { get; set; }

        [DataMember]
        public OverlayItemModelBase OverlayItem { get; set; }

        [DataMember]
        public Guid WidgetID { get; set; }
        [DataMember]
        public bool ShowWidget { get; set; }

        public OverlayActionModel(string overlayName, OverlayItemModelBase overlayItem)
            : base(ActionTypeEnum.Overlay)
        {
            var overlays = ServiceManager.Get<OverlayService>().GetOverlayNames();
            if (overlays.Contains(overlayName))
            {
                this.OverlayName = overlayName;
            }
            else
            {
                this.OverlayName = ServiceManager.Get<OverlayService>().DefaultOverlayName;
            }

            this.OverlayItem = overlayItem;
        }

        public OverlayActionModel(Guid widgetID, bool showWidget)
            : base(ActionTypeEnum.Overlay)
        {
            this.WidgetID = widgetID;
            this.ShowWidget = showWidget;
        }

        [Obsolete]
        public OverlayActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.WidgetID != Guid.Empty)
            {
                OverlayWidgetModel widget = ChannelSession.Settings.OverlayWidgets.FirstOrDefault(w => w.Item.ID.Equals(this.WidgetID));
                if (widget != null)
                {
                    if (this.ShowWidget)
                    {
                        await widget.Enable(parameters);
                    }
                    else
                    {
                        await widget.Disable();
                    }
                }
            }
            else
            {
                string overlayName = (string.IsNullOrEmpty(this.OverlayName)) ? ServiceManager.Get<OverlayService>().DefaultOverlayName : this.OverlayName;
                OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlay(overlayName);
                if (overlay != null)
                {
                    await overlay.ShowItem(this.OverlayItem, parameters);
                }
            }
        }
    }
}
