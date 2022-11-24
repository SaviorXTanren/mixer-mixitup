using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayGoalV3ViewModel : OverlayItemV3ViewModelBase
    {
        public OverlayGoalV3ViewModel()
            : base(OverlayItemV3Type.Goal)
        {

        }

        public OverlayGoalV3ViewModel(OverlayGoalV3Model item)
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
            OverlayGoalV3Model item = (OverlayGoalV3Model)this.GetItem();

            return item;
        }
    }
}
