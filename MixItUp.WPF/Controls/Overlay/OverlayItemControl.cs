using MixItUp.Base.Model.Overlay;
using System.Diagnostics;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Overlay
{
    public abstract class OverlayItemControl : LoadingControlBase
    {
        public virtual void SetItem(OverlayItemModelBase item) { }

        public virtual OverlayItemModelBase GetItem() { return null; }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
