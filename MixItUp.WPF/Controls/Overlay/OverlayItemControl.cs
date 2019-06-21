using MixItUp.Base.Model.Overlay;
using System;
using System.Diagnostics;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Overlay
{
    public abstract class OverlayItemControl : LoadingControlBase
    {
        [Obsolete]
        public abstract void SetItem(OverlayItemBase item);

        [Obsolete]
        public abstract OverlayItemBase GetItem();

        public virtual void SetOverlayItem(OverlayItemModelBase item) { }

        public virtual OverlayItemModelBase GetOverlayItem() { return null; }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
