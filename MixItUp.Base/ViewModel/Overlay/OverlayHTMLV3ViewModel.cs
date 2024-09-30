using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayHTMLV3ViewModel : OverlayItemV3ViewModelBase
    {
        public OverlayHTMLV3ViewModel() : base(OverlayItemV3Type.HTML) { }

        public OverlayHTMLV3ViewModel(OverlayHTMLV3Model item) : base(item) { }

        public override string DefaultHTML { get { return OverlayHTMLV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayHTMLV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayHTMLV3Model.DefaultJavascript; } }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            return new OverlayHTMLV3Model();
        }
    }
}
