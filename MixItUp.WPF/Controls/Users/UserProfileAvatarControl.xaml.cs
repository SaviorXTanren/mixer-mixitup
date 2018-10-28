using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
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
        private static Dictionary<uint, BitmapImage> userAvatarCache = new Dictionary<uint, BitmapImage>();

        public UserProfileAvatarControl()
        {
            InitializeComponent();
        }

        public async Task SetUserAvatarUrl(uint userID)
        {
            await this.SetUserAvatarUrl(new UserViewModel() { ID = userID });
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
                else
                {
                    using (WebClient client = new WebClient())
                    {
                        var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(user.AvatarLink); });

                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = new MemoryStream(bytes);
                        bitmap.EndInit();
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
