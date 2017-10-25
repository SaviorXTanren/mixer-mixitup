using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;
using System.Windows;

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
            if (!string.IsNullOrEmpty(this.User.AvatarLink))
            {
                this.UserAvatar.SetImageUrl(this.User.AvatarLink);
            }

            if (ChatControl.SubscriberBadgeBitmap != null && this.User.IsSubscriber)
            {
                this.SubscriberImage.Visibility = Visibility.Visible;
                this.SubscriberImage.Source = ChatControl.SubscriberBadgeBitmap;
            }
        }
    }
}
