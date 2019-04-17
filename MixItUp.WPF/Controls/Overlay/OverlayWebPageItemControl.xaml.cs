using MixItUp.Base.Model.Overlay;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWebPageItemControl.xaml
    /// </summary>
    public partial class OverlayWebPageItemControl : OverlayItemControl
    {
        private static readonly List<int> sampleFontSize = new List<int>() { 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };

        private OverlayWebPageItem item;

        public OverlayWebPageItemControl()
        {
            InitializeComponent();
        }

        public OverlayWebPageItemControl(OverlayWebPageItem item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayWebPageItem)item;
            this.WebPageFilePathTextBox.Text = this.item.URL;
            this.WebPageWidthTextBox.Text = this.item.Width.ToString();
            this.WebPageHeightTextBox.Text = this.item.Height.ToString();
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.WebPageFilePathTextBox.Text))
            {
                int width;
                int height;
                if (int.TryParse(this.WebPageWidthTextBox.Text, out width) && width > 0 &&
                    int.TryParse(this.WebPageHeightTextBox.Text, out height) && height > 0)
                {
                    return new OverlayWebPageItem(this.WebPageFilePathTextBox.Text, width, height);
                }
            }
            return null;
        }

        protected override Task OnLoaded()
        {
            if (this.item != null)
            {
                this.SetItem(this.item);
            }
            return Task.FromResult(0);
        }
    }
}
