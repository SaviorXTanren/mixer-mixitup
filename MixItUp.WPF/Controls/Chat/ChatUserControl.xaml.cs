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

        public ChatUserControl()
        {
            this.DataContextChanged += ChatUserControl_DataContextChanged;
            InitializeComponent();
        }

        public ChatUserControl(UserViewModel user) : this()
        {
            InitializeComponent();
            this.DataContext = user;
        }

        public bool MatchesUser(UserViewModel user) { return this.User.Equals(user); }

        private void ChatUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.UserNameTextBlock.Foreground = Application.Current.FindResource(this.User.PrimaryRoleColorName) as SolidColorBrush;

            if (!string.IsNullOrEmpty(this.User.AvatarLink))
            {
                Task.Run(() => this.Dispatcher.Invoke(() => this.UserAvatar.SetUserAvatarUrl(this.User)));
            }

            if (ChatControl.SubscriberBadgeBitmap != null && this.User.IsSubscriber)
            {
                this.SubscriberImage.Visibility = Visibility.Visible;
                this.SubscriberImage.Source = ChatControl.SubscriberBadgeBitmap;
            }
        }
    }
}
