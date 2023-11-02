using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayLabelV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public OverlayLabelV3ViewModel() : base(OverlayItemV3Type.Label) { }

        public OverlayLabelV3ViewModel(OverlayLabelV3Model item) : base(item) { }

        public override Result Validate()
        {
            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayLabelV3Model result = new OverlayLabelV3Model();
            this.AssignProperties(result);
            return result;
        }
    }
}
