using MixItUp.Base.ViewModel;
using System.Windows.Controls;

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
        }
    }
}
