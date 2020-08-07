using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class OverlayActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return OverlayActionModel.asyncSemaphore; } }

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
            this.OverlayName = overlayName;
            this.OverlayItem = overlayItem;
        }

        public OverlayActionModel(Guid widgetID, bool showWidget)
            : base(ActionTypeEnum.Overlay)
        {
            this.WidgetID = widgetID;
            this.ShowWidget = showWidget;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.WidgetID != Guid.Empty)
            {
                OverlayWidgetModel widget = ChannelSession.Settings.OverlayWidgets.FirstOrDefault(w => w.Item.ID.Equals(this.WidgetID));
                if (widget != null)
                {
                    if (this.ShowWidget)
                    {
                        await widget.Enable(user, arguments, specialIdentifiers);
                    }
                    else
                    {
                        await widget.Disable();
                    }
                }
            }
            else
            {
                string overlayName = (string.IsNullOrEmpty(this.OverlayName)) ? ChannelSession.Services.Overlay.DefaultOverlayName : this.OverlayName;
                IOverlayEndpointService overlay = ChannelSession.Services.Overlay.GetOverlay(overlayName);
                if (overlay != null)
                {
                    await overlay.ShowItem(this.OverlayItem, user, arguments, specialIdentifiers, platform);
                }
            }
        }
    }
}
