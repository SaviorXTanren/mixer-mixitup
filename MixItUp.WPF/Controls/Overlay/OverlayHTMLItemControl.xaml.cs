using MixItUp.Base.Model.Overlay;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayHTMLItemControl.xaml
    /// </summary>
    public partial class OverlayHTMLItemControl : OverlayItemControl
    {
        private static readonly List<int> sampleFontSize = new List<int>() { 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };

        private OverlayHTMLItem item;

        public OverlayHTMLItemControl()
        {
            InitializeComponent();
        }

        public OverlayHTMLItemControl(OverlayHTMLItem item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayHTMLItem)item;
            this.HTMLTextBox.Text = this.item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.HTMLTextBox.Text))
            {
                return new OverlayHTMLItem(this.HTMLTextBox.Text);
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
