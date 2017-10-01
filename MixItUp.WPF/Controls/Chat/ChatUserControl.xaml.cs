using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
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
            InitializeComponent();

            this.DataContext = this.User = user;
        }
    }
}
