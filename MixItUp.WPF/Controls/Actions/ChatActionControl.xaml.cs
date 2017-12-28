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
                this.ChatWhisperToggleButton.IsChecked = this.action.IsWhisper;
                this.ChatSendAsStreamerToggleButton.IsChecked = this.action.SendAsStreamer;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.ChatMessageTextBox.Text))
            {
                return new ChatAction(this.ChatMessageTextBox.Text, this.ChatWhisperToggleButton.IsChecked.GetValueOrDefault(), this.ChatSendAsStreamerToggleButton.IsChecked.GetValueOrDefault());
            }
            return null;
        }
    }
}
