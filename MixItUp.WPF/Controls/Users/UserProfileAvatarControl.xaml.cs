using Mixer.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Services;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Users
{
    /// <summary>
    /// Interaction logic for UserProfileAvatarControl.xaml
    /// </summary>
    public partial class UserProfileAvatarControl : UserControl
    {
        private static Dictionary<Guid, BitmapImage> userAvatarCache = new Dictionary<Guid, BitmapImage>();

        // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(int), typeof(UserProfileAvatarControl), new PropertyMetadata(0));
        public int Size
        {
            get { return (int)GetValue(SizeProperty); }
            set
            {
                SetValue(SizeProperty, value);
                this.SetSize(value);
            }
        }

        public UserProfileAvatarControl()
        {
            InitializeComponent();

            this.Loaded += UserProfileAvatarControl_Loaded;
            this.DataContextChanged += UserProfileAvatarControl_DataContextChanged;
        }

        private void UserProfileAvatarControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UserProfileAvatarControl_DataContextChanged(sender, new System.Windows.DependencyPropertyChangedEventArgs());
        }

        private async void UserProfileAvatarControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is UserViewModel)
            {
                UserViewModel user = (UserViewModel)this.DataContext;
                if (this.Size > 0)
                {
                    this.SetSize(this.Size);
                }
                await this.SetUserAvatarUrl(user);
            }
        }

        public async Task SetMixerUserAvatarUrl(uint mixerUserID)
        {
            await this.SetUserAvatarUrl(new UserViewModel(new UserModel() { id = mixerUserID }));
        }

        public async Task SetUserAvatarUrl(UserViewModel user)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                if (userAvatarCache.ContainsKey(user.ID))
                {
                    bitmap = userAvatarCache[user.ID];
                }
                else if (!string.IsNullOrEmpty(user.AvatarLink))
                {
                    using (WebClient client = new WebClient())
                    {
                        var bytes = await Task.Run<byte[]>((Func<Task<byte[]>>)(async () => { return await client.DownloadDataTaskAsync((string)user.AvatarLink); }));
                        bitmap = WindowsImageService.Load(bytes);
                    }
                    userAvatarCache[user.ID] = bitmap;
                }

                this.ProfileAvatarImage.ImageSource = bitmap;
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void SetSize(int size)
        {
            this.ProfileAvatarContainer.Width = size;
            this.ProfileAvatarContainer.Height = size;
        }
    }
}
