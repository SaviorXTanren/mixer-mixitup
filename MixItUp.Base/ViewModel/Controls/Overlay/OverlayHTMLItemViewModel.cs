using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayHTMLItemViewModel : OverlayItemViewModelBase
    {
        public string HTML
        {
            get { return this.html; }
            set
            {
                this.html = value;
                this.NotifyPropertyChanged();
            }
        }
        private string html;

        public override bool SupportsRefreshUpdating { get { return true; } }

        public OverlayHTMLItemViewModel() { }

        public OverlayHTMLItemViewModel(OverlayHTMLItemModel item)
            : this()
        {
            this.HTML = item.HTML;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.HTML))
            {
                return new OverlayHTMLItemModel(this.HTML);
            }
            return null;
        }
    }
}
