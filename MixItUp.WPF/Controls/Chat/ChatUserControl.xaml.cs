using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;
using System.Windows;
using MixItUp.WPF.Controls.MainControls;
using System.Windows.Media;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : UserControl
    {
        public UserViewModel User { get; private set; }

        public ChatUserControl(UserViewModel user)
        {
            this.Loaded += ChatUserControl_Loaded;

            InitializeComponent();

            this.DataContext = this.User = user;
        }

        private void ChatUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UserNameTextBlock.Foreground = Application.Current.FindResource(this.User.PrimaryRoleColorName) as SolidColorBrush;

            if (!string.IsNullOrEmpty(this.User.AvatarLink))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.UserAvatar.SetImageUrl(this.User.AvatarLink);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            if (ChatControl.SubscriberBadgeBitmap != null && this.User.IsSubscriber)
            {
                this.SubscriberImage.Visibility = Visibility.Visible;
                this.SubscriberImage.Source = ChatControl.SubscriberBadgeBitmap;
            }
        }
    }
}
