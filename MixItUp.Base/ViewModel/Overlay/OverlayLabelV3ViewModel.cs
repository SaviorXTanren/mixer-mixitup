using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayLabelV3ViewModel : OverlayItemV3ViewModelBase
    {
        public const string UpdatedAnimationName = "Updated";

        public OverlayLabelV3ViewModel()
            : base(OverlayItemV3Type.Label)
        {
            this.Animations.Add(new OverlayAnimationV3ViewModel(OverlayLabelV3ViewModel.UpdatedAnimationName));
        }

        public OverlayLabelV3ViewModel(OverlayLabelV3Model item)
            : base(item)
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
            OverlayLabelV3Model item = (OverlayLabelV3Model)this.GetItem();

            item.LabelType = OverlayLabelWidgetV3Type.Counter;

            return item;
        }
    }
}
