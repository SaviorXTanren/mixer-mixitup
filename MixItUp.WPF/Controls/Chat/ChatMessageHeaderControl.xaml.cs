using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Controls.MainControls;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatMessageHeaderControl.xaml
    /// </summary>
    public partial class ChatMessageHeaderControl : UserControl
    {
        public ChatMessageViewModel Message { get; private set; }

        public ChatMessageHeaderControl(ChatMessageViewModel message)
        {
            this.Loaded += ChatMessageHeaderControl_Loaded;

            InitializeComponent();

            this.DataContext = this.Message = message;
        }

        private void ChatMessageHeaderControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Message.User.AvatarLink))
            {
                this.UserAvatar.SetImageUrl(this.Message.User.AvatarLink);
            }

            if (ChatControl.SubscriberBadgeBitmap != null && this.Message.User.IsSubscriber)
            {
                this.SubscriberImage.Visibility = Visibility.Visible;
                this.SubscriberImage.Source = ChatControl.SubscriberBadgeBitmap;
            }
        }

        public void DeleteMessage()
        {
            this.UserTextBlock.TextDecorations = TextDecorations.Strikethrough;
            this.TargetUserTextBlock.TextDecorations = TextDecorations.Strikethrough;
        }
    }
}
