using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
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
            if (this.user != null && !this.user.IsAnonymous && !string.IsNullOrEmpty(this.user.UserName))
            {
                await this.user.RefreshDetails(force: true);

                this.UserAvatar.SetSize(100);
                await this.UserAvatar.SetUserAvatarUrl(this.user);

                PromoteToModButton.IsEnabled = ChannelSession.IsStreamer;
                DemoteFromModButton.IsEnabled = ChannelSession.IsStreamer;
                EditUserButton.IsEnabled = ChannelSession.IsStreamer;

                bool follows = false;
                if (this.user.ChannelID > 0)
                {
                    ExpandedChannelModel channelToCheck = await ChannelSession.Connection.GetChannel(this.user.ChannelID);
                    if (channelToCheck != null)
                    {
                        follows = (await ChannelSession.Connection.CheckIfFollows(channelToCheck, ChannelSession.User)).HasValue;
                        if (channelToCheck.online)
                        {
                            this.StreamStatusTextBlock.Text = $"{channelToCheck.viewersCurrent} Viewers";
                        }
                        else
                        {
                            this.StreamStatusTextBlock.Text = "Offline";
                        }
                    }
                }

                if (follows)
                {
                    this.UnfollowButton.Visibility = System.Windows.Visibility.Visible;
                    this.FollowButton.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (this.user.MixerRoles.Contains(MixerRoleEnum.Banned))
                {
                    this.UnbanButton.Visibility = System.Windows.Visibility.Visible;
                    this.BanButton.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (this.user.MixerRoles.Contains(MixerRoleEnum.Mod))
                {
                    this.DemoteFromModButton.Visibility = System.Windows.Visibility.Visible;
                    this.PromoteToModButton.Visibility = System.Windows.Visibility.Collapsed;
                }

                this.DataContext = this.user;
            }
        }
    }
}
