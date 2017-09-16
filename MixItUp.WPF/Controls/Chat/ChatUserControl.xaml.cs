using MixItUp.Base.ViewModel.Chat;
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
        }
    }
}
