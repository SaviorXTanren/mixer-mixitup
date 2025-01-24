using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTextV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayTextV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayTextV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayTextV3Model.DefaultJavascript; } }

        public OverlayTextV3ViewModel() : base(OverlayItemV3Type.Text) { }

        public OverlayTextV3ViewModel(OverlayTextV3Model item) : base(item) { }

        public override Result Validate()
        {
            if (string.IsNullOrEmpty(this.Text))
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
