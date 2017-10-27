using MixItUp.Base.ViewModel.Chat;
using System.Windows;
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
            this.Loaded += ChatMessageControl_Loaded;

            InitializeComponent();

            this.DataContext = this.Message = message;
        }

        private void ChatMessageControl_Loaded(object sender, RoutedEventArgs e)
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

        public void DeleteMessage(string reason = null)
        {
            if (!string.IsNullOrEmpty(reason))
            {
                this.Message.AddToMessage(" (Auto-Moderated: " + reason + ")");
                this.DataContext = null;
                this.DataContext = this.Message;
            }
            this.Message.IsDeleted = true;
            this.MessageTextBlock.TextDecorations = TextDecorations.Strikethrough;
        }
    }
}
