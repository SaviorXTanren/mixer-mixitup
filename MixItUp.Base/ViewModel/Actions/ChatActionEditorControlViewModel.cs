using MixItUp.Base.Model;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class ChatActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Chat; } }

        public string ChatText
        {
            get { return this.chatText; }
            set
            {
                this.chatText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string chatText;

        public bool SendAsStreamer
        {
            get { return this.sendAsStreamer; }
            set
            {
                this.sendAsStreamer = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool sendAsStreamer = false;

        public bool IsWhisper
        {
            get { return this.isWhisper; }
            set
            {
                this.isWhisper = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isWhisper = false;

        public string WhisperUserName
        {
            get { return this.whisperUserName; }
            set
            {
                this.whisperUserName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string whisperUserName;

        public ChatActionEditorControlViewModel(ChatActionModel action)
            : base(action)
        {
            this.ChatText = action.ChatText;
            this.SendAsStreamer = action.SendAsStreamer;
            this.IsWhisper = action.IsWhisper;
            this.WhisperUserName = action.WhisperUserName;
        }

        public ChatActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.ChatText))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ChatActionMissingChatText));
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            return Task.FromResult<ActionModelBase>(new ChatActionModel(this.ChatText, this.SendAsStreamer, this.IsWhisper, this.WhisperUserName));
        }
    }
}
