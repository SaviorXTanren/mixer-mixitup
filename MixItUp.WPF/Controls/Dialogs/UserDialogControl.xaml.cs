using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    public enum UserDialogResult
    {
        Purge,
        Timeout1,
        Timeout5,
        Ban,
        Unban,
        Close,
        Follow,
        Unfollow,
        PromoteToMod,
        DemoteFromMod,
        ChannelPage,
        EditUser,
    }

    /// <summary>
    /// Interaction logic for UserDialogControl.xaml
    /// </summary>
    public partial class UserDialogControl : UserControl
    {
        private UserViewModel user;

        public UserDialogControl(UserViewModel user)
        {
            this.user = user;

            InitializeComponent();

            this.Loaded += UserDialogControl_Loaded;
        }

        private async void UserDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.user != null && !this.user.IsAnonymous && !string.IsNullOrEmpty(this.user.Username))
            {
                await this.user.RefreshDetails(force: true);

                this.DataContext = this.user;

                this.PromoteToModButton.IsEnabled = this.DemoteFromModButton.IsEnabled = this.EditUserButton.IsEnabled = ChannelSession.IsStreamer;

                bool follows = false;
                if (follows)
                {
                    this.UnfollowButton.Visibility = System.Windows.Visibility.Visible;
                    this.FollowButton.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (this.user.UserRoles.Contains(UserRoleEnum.Banned))
                {
                    this.UnbanButton.Visibility = System.Windows.Visibility.Visible;
                    this.BanButton.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (this.user.UserRoles.Contains(UserRoleEnum.Mod))
                {
                    this.DemoteFromModButton.Visibility = System.Windows.Visibility.Visible;
                    this.PromoteToModButton.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
    }
}
