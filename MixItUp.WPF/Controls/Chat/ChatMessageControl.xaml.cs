using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Util;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatMessageControl.xaml
    /// </summary>
    public partial class ChatMessageControl : UserControl
    {
        public ChatMessageViewModel Message { get; private set; }

        public ChatMessageControl(ChatMessageViewModel message)
        {
            InitializeComponent();

            this.DataContext = this.Message = message;

            this.Loaded += ChatMessageControl_Loaded;
        }

        private void ChatMessageControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.Message.User != null)
            {
                this.UserTextBlock.Foreground = ColorHelper.GetColorForUser(this.Message.User);
            }
        }
    }
}
