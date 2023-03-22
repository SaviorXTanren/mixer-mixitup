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
        [Obsolete]
        public string OverlayName { get; set; }
        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        [Obsolete]
        public OverlayItemModelBase OverlayItem { get; set; }

        [DataMember]
        public OverlayItemV3ModelBase OverlayItemV3 { get; set; }

        [DataMember]
        public Guid WidgetID { get; set; }
        [DataMember]
        public bool ShowWidget { get; set; }

        [Obsolete]
        public OverlayActionModel(Guid overlayEndpointID, OverlayItemModelBase overlayItem)
            : base(ActionTypeEnum.Overlay)
        {
            //var overlays = ServiceManager.Get<OverlayService>().GetOverlayNames();
            //if (overlays.Contains(overlayName))
            //{
            //    this.OverlayName = overlayName;
            //}
            //else
            //{
            //    this.OverlayName = ServiceManager.Get<OverlayService>().DefaultOverlayName;
            //}

            this.OverlayEndpointID = overlayEndpointID;
            this.OverlayItem = overlayItem;
        }

        public OverlayActionModel(Guid overlayEndpointID, OverlayItemV3ModelBase overlayItem)
            : base(ActionTypeEnum.Overlay)
        {
            //var overlays = ServiceManager.Get<OverlayService>().GetOverlayNames();
            //if (overlays.Contains(overlayName))
            //{
            //    this.OverlayName = overlayName;
            //}
            //else
            //{
            //    this.OverlayName = ServiceManager.Get<OverlayService>().DefaultOverlayName;
            //}

            this.OverlayEndpointID = overlayEndpointID;
            this.OverlayItemV3 = overlayItem;
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
#pragma warning disable CS0612 // Type or member is obsolete
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
#pragma warning restore CS0612 // Type or member is obsolete
            }
            else
            {
                OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
                if (overlay != null)
                {
                    if (this.OverlayItemV3.Type == OverlayItemV3Type.YouTube)
                    {
                        await overlay.SendYouTube(this.OverlayItemV3, parameters);
                    }
                    else
                    {
                        await overlay.SendBasic(this.OverlayItemV3, parameters);
                    }
                }
            }
        }
    }
}
