using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTextV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public OverlayTextV3ViewModel() : base(OverlayItemV3Type.Text) { }

        public OverlayTextV3ViewModel(OverlayTextV3Model item) : base(item) { }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Text))
            {
                return new Result(Resources.OverlayTextMissingText);
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayTextV3Model result = new OverlayTextV3Model();
            this.AssignProperties(result);
            return result;
        }
    }
}
