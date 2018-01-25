using MixItUp.Base.Actions;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ChatActionControl.xaml
    /// </summary>
    public partial class ChatActionControl : ActionControlBase
    {
        private ChatAction action;

        public ChatActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public ChatActionControl(ActionContainerControl containerControl, ChatAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.ChatMessageTextBox.Text = this.action.ChatText;
                this.ChatSendAsStreamerToggleButton.IsChecked = this.action.SendAsStreamer;
                this.ChatWhisperToggleButton.IsChecked = this.action.IsWhisper;
                this.ChatWhisperUserNameTextBox.Text = this.action.WhisperUserName;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.ChatMessageTextBox.Text))
            {
                if (this.ChatWhisperToggleButton.IsChecked.GetValueOrDefault())
                {
                    return new ChatAction(this.ChatMessageTextBox.Text, this.ChatSendAsStreamerToggleButton.IsChecked.GetValueOrDefault(),
                        isWhisper: true, whisperUserName: this.ChatWhisperUserNameTextBox.Text);
                }
                else
                {
                    return new ChatAction(this.ChatMessageTextBox.Text, this.ChatSendAsStreamerToggleButton.IsChecked.GetValueOrDefault());
                }
            }
            return null;
        }

        private void ChatWhisperToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ChatWhisperUserNameTextBox.IsEnabled = this.ChatWhisperToggleButton.IsChecked.GetValueOrDefault();
        }
    }
}
