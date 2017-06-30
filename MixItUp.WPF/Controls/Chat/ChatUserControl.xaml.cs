using MixItUp.Base.ViewModels;
using System.Windows.Controls;
using System.Windows.Media;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : UserControl
    {
        public ChatUserViewModel User { get; private set; }

        public ChatUserControl(ChatUserViewModel user)
        {
            InitializeComponent();

            this.DataContext = this.User = user;

            this.Loaded += ChatUserControl_Loaded;
        }

        private void ChatUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (this.User.Role)
            {
                case UserRole.Streamer:
                case UserRole.Mod:
                    this.UserNameTextBlock.Foreground = Brushes.Green;
                    break;
                case UserRole.Staff:
                    this.UserNameTextBlock.Foreground = Brushes.Gold;
                    break;
                case UserRole.Subscriber:
                case UserRole.Pro:
                    this.UserNameTextBlock.Foreground = Brushes.Purple;
                    break;
                case UserRole.User:
                    this.UserNameTextBlock.Foreground = Brushes.Blue;
                    break;
            }
        }
    }
}
