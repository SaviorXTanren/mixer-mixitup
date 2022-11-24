using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public abstract class OverlayListV3ModelBase : OverlayVisualTextV3ModelBase
    {
        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }

        [DataMember]
        public int MaxToShow { get; set; }

        public OverlayListV3ModelBase(OverlayItemV3Type type) : base(type) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.BorderColor), this.BorderColor);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.BorderColor), this.BorderColor);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.BorderColor), this.BorderColor);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.BackgroundColor), this.BackgroundColor);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.BackgroundColor), this.BackgroundColor);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.BackgroundColor), this.BackgroundColor);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.MaxToShow), this.MaxToShow.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.MaxToShow), this.MaxToShow.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.MaxToShow), this.MaxToShow.ToString());

            return item;
        }
    }
}
