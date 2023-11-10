using MixItUp.Base;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : UserControl
    {
        public ChatUserControl()
        {
            InitializeComponent();

            this.Loaded += ChatUserControl_Loaded;
        }

        private void ChatUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UsernameTextBlock.FontSize = ChannelSession.Settings.ChatFontSize;
        }
    }
}
