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
            this.AvatarImage.Size = ChannelSession.Settings.ChatFontSize;
            this.PlatformImage.Width = this.PlatformImage.Height = ChannelSession.Settings.ChatFontSize;
            this.SubscriberImage.Width = this.SubscriberImage.Height = ChannelSession.Settings.ChatFontSize;
            this.UsernameTextBlock.FontSize = ChannelSession.Settings.ChatFontSize;
        }
    }
}
