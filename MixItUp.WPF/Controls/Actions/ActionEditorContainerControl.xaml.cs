using MixItUp.Base.Model.Actions;
using MixItUp.Base.ViewModel.Controls.Actions;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ActionEditorContainerControl.xaml
    /// </summary>
    public partial class ActionEditorContainerControl : LoadingControlBase
    {
        public ActionEditorControlViewModelBase ViewModel { get; private set; }

        public ActionEditorControlBase Control { get; private set; }

        public ActionEditorContainerControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            if (this.IsLoaded && this.DataContext != null && this.DataContext is ActionEditorControlViewModelBase && this.ActionContentControl.Content == null)
            {
                this.ViewModel = (ActionEditorControlViewModelBase)this.DataContext;
                switch (this.ViewModel.Type)
                {
                    case ActionTypeEnum.Chat: this.Control = new ChatActionEditorControl(); break;
                    case ActionTypeEnum.Command: this.Control = new CommandActionEditorControl(); break;
                    case ActionTypeEnum.Conditional: this.Control = new ConditionalActionEditorControl(); break;
                    case ActionTypeEnum.Consumables: this.Control = new ConsumablesActionEditorControl(); break;
                    case ActionTypeEnum.Counter: this.Control = new CounterActionEditorControl(); break;
                    case ActionTypeEnum.Discord: this.Control = new DiscordActionEditorControl(); break;
                    case ActionTypeEnum.ExternalProgram: this.Control = new ExternalProgramActionEditorControl(); break;
                    case ActionTypeEnum.File: this.Control = new FileActionEditorControl(); break;
                    case ActionTypeEnum.GameQueue: this.Control = new GameQueueActionEditorControl(); break;
                    case ActionTypeEnum.IFTTT: this.Control = new IFTTTActionEditorControl(); break;
                    case ActionTypeEnum.Input: this.Control = new InputActionEditorControl(); break;
                    case ActionTypeEnum.Moderation: this.Control = new ModerationActionEditorControl(); break;
                    case ActionTypeEnum.Overlay: this.Control = new OverlayActionEditorControl(); break;
                    case ActionTypeEnum.OvrStream: this.Control = new OvrStreamActionEditorControl(); break;
                    case ActionTypeEnum.Serial: this.Control = new SerialActionEditorControl(); break;
                    case ActionTypeEnum.Sound: this.Control = new SoundActionEditorControl(); break;
                    case ActionTypeEnum.SpecialIdentifier: this.Control = new SpecialIdentifierActionEditorControl(); break;
                    case ActionTypeEnum.StreamingSoftware: this.Control = new StreamingSoftwareActionEditorControl(); break;
                    case ActionTypeEnum.Streamlabs: this.Control = new StreamlabsActionEditorControl(); break;
                    case ActionTypeEnum.TextToSpeech: this.Control = new TextToSpeechActionEditorControl(); break;
                    case ActionTypeEnum.Translation: this.Control = new TranslationActionEditorControl(); break;
                    case ActionTypeEnum.Twitch: this.Control = new TwitchActionEditorControl(); break;
                    case ActionTypeEnum.Twitter: this.Control = new TwitterActionEditorControl(); break;
                    case ActionTypeEnum.Wait: this.Control = new WaitActionEditorControl(); break;
                    case ActionTypeEnum.WebRequest: this.Control = new WebRequestActionEditorControl(); break;
                }

                if (this.Control != null)
                {
                    this.ActionContentControl.Content = this.Control;
                }
            }
            return Task.FromResult(0);
        }
    }
}
