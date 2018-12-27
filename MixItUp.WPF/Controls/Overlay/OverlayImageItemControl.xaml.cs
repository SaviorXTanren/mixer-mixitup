using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayImageItemControl.xaml
    /// </summary>
    public partial class OverlayImageItemControl : OverlayItemControl
    {
        private OverlayImageItem item;

        public OverlayImageItemControl()
        {
            InitializeComponent();
        }

        public OverlayImageItemControl(OverlayImageItem item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayImageItem)item;
            this.ImageFilePathTextBox.Text = this.item.FilePath;
            this.ImageWidthTextBox.Text = this.item.Width.ToString();
            this.ImageHeightTextBox.Text = this.item.Height.ToString();
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.ImageFilePathTextBox.Text))
            {
                int width;
                int height;
                if (int.TryParse(this.ImageWidthTextBox.Text, out width) && width > 0 &&
                    int.TryParse(this.ImageHeightTextBox.Text, out height) && height > 0)
                {
                    return new OverlayImageItem(this.ImageFilePathTextBox.Text, width, height);
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

        private void ImageFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.ImageFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.ImageFilePathTextBox.Text = filePath;
            }
        }
    }
}
