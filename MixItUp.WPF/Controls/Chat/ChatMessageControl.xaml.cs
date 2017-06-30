using MixItUp.Base.ViewModels;
using System.Windows.Controls;
using System.Windows.Media;

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
            switch (this.Message.User.Role)
            {
                case UserRole.Streamer:
                case UserRole.Mod:
                    this.UserTextBlock.Foreground = Brushes.Green;
                    break;
                case UserRole.Subscriber:
                case UserRole.Pro:
                    this.UserTextBlock.Foreground = Brushes.Purple;
                    break;
                case UserRole.User:
                    this.UserTextBlock.Foreground = Brushes.Blue;
                    break;
            }
        }
    }
}
