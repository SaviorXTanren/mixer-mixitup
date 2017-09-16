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
            InitializeComponent();

            this.DataContext = this.Message = message;
        }

        public void MessageDeleted()
        {
            this.MessageTextBlock.TextDecorations = TextDecorations.Strikethrough;
        }
    }
}
