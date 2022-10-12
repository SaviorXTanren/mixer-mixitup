﻿using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayHTMLItemV3ViewModel : OverlayItemV3ViewModelBase
    {
        public OverlayHTMLItemV3ViewModel() : base(OverlayItemV3Type.HTML) { }

        public OverlayHTMLItemV3ViewModel(OverlayHTMLItemV3Model item) : base(item) { }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            return new OverlayHTMLItemV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,
            };
        }
    }
}