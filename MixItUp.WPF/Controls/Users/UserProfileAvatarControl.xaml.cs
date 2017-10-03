using System;
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

        public void SetImageUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.EndInit();

                this.ProfileAvatarImage.Source = bitmap;
            }
        }
    }
}
