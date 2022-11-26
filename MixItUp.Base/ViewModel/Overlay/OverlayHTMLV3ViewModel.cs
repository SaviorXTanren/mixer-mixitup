using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayHTMLV3ViewModel : OverlayItemV3ViewModelBase
    {
        public OverlayHTMLV3ViewModel(bool addDefaultAnimation = false) : base(OverlayItemV3Type.HTML, addDefaultAnimation) { }

        public OverlayHTMLV3ViewModel(OverlayHTMLV3Model item) : base(item) { }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            return new OverlayHTMLV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,
            };
        }
    }
}
