using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayHeaderV3ViewModelBase : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return string.Empty; } }

        public override string DefaultCSS { get { return string.Empty; } }

        public override string DefaultJavascript { get { return string.Empty; } }

        public OverlayHeaderV3ViewModelBase() : base(OverlayItemV3Type.Text) { }

        public OverlayHeaderV3ViewModelBase(OverlayHeaderV3ModelBase model) : base(model) { }
    }
}
