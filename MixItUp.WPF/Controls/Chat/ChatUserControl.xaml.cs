using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;
using System.Windows;
using MixItUp.WPF.Controls.MainControls;
using System.Windows.Media;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : UserControl
    {
        public UserViewModel User { get { return this.DataContext as UserViewModel; } }

        public ChatUserControl(UserViewModel user)
        {
            this.DataContext = user;

            this.Loaded += ChatUserControl_Loaded;

            InitializeComponent();
        }

        public bool MatchesUser(UserViewModel other)
        {
            UserViewModel user = this.User;
            return user != null && user.Equals(other);
        }

        private void ChatUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeControls();
        }

        private void ChatUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.InitializeControls();
        }

        private void InitializeControls()
        {
            UserViewModel user = this.User;
            if (this.IsLoaded && user != null)
            {
                this.UserNameTextBlock.Foreground = Application.Current.FindResource(user.PrimaryRoleColorName) as SolidColorBrush;

                if (!string.IsNullOrEmpty(user.AvatarLink))
                {
                    Task.Run(() => this.Dispatcher.Invoke(() => this.UserAvatar.SetUserAvatarUrl(user)));
                }

                if (ChatControl.SubscriberBadgeBitmap != null && user.IsSubscriber)
                {
                    this.SubscriberImage.Visibility = Visibility.Visible;
                    this.SubscriberImage.Source = ChatControl.SubscriberBadgeBitmap;
                }
            }
        }
    }
}
