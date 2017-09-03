using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Util;
using System.Windows.Controls;

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
            this.UserNameTextBlock.Foreground = ColorHelper.GetColorForUser(this.User);
        }
    }
}
