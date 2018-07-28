using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Users
{
    /// <summary>
    /// Interaction logic for UserProfileAvatarControl.xaml
    /// </summary>
    public partial class UserProfileAvatarControl : UserControl
    {
        public UserProfileAvatarControl()
        {
            InitializeComponent();
        }

        public async Task SetImageUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                using (WebClient client = new WebClient())
                {
                    var bytes = await client.DownloadDataTaskAsync(url);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = new MemoryStream(bytes);
                    bitmap.EndInit();

                    this.ProfileAvatarImage.ImageSource = bitmap;
                }
            }
        }

        public void SetSize(int size)
        {
            this.ProfileAvatarContainer.Width = size;
            this.ProfileAvatarContainer.Height = size;
        }
    }
}
