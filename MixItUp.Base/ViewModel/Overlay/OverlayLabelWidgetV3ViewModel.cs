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

        public OverlayLabelWidgetV3ViewModel(OverlayItemV3ModelBase widget)
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

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayLabelItemV3Model widget = (OverlayLabelItemV3Model)this.Item.GetItem();
            widget.ID = this.ID;
            widget.Name = this.Name;
            widget.OverlayEndpointID = this.OverlayEndpointID;

            widget.LabelType = OverlayLabelWidgetV3Type.Counter;

            return widget;
        }
    }
}
