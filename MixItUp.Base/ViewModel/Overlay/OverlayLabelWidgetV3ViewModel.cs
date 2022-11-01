using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayLabelWidgetV3ViewModel : OverlayWidgetV3ViewModelBase
    {
        public OverlayLabelWidgetV3ViewModel()
            : base(OverlayItemV3Type.Label)
        {

        }

        public OverlayLabelWidgetV3ViewModel(OverlayLabelWidgetV3Model widget)
            : base(widget)
        {

        }

        public override Result Validate()
        {
            Result result = base.Validate();

            if (result.Success)
            {
                
            }

            return result;
        }

        protected override OverlayWidgetV3ModelBase GetItemInternal()
        {
            OverlayLabelWidgetV3Model widget = new OverlayLabelWidgetV3Model(this.ID, this.Name, this.OverlayEndpointID, (OverlayTextItemV3Model)this.Item.GetItem());

            widget.LabelType = OverlayLabelWidgetV3Type.Counter;

            return widget;
        }
    }
}
