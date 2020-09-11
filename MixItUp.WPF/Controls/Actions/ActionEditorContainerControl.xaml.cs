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

            this.DataContextChanged += ActionEditorContainerControl_DataContextChanged;
        }

        protected override async Task OnLoaded()
        {
            this.ActionContentControl.Content = this.Control;
            await base.OnLoaded();
        }

        private void ActionEditorContainerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is ActionEditorControlViewModelBase)
            {
                this.ViewModel = (ActionEditorControlViewModelBase)this.DataContext;
                switch (this.ViewModel.Type)
                {
                    case ActionTypeEnum.Chat: this.Control = new ChatActionEditorControl(); break;
                    case ActionTypeEnum.Command: this.Control = new CommandActionEditorControl(); break;
                    case ActionTypeEnum.Conditional: break;
                    case ActionTypeEnum.Consumables: this.Content = new ConsumablesActionEditorControl(); break;
                    case ActionTypeEnum.Counter: this.Content = new CounterActionEditorControl(); break;
                    case ActionTypeEnum.Discord: this.Content = new DiscordActionEditorControl(); break;
                    case ActionTypeEnum.ExternalProgram: this.Content = new ExternalProgramActionEditorControl(); break;
                    case ActionTypeEnum.File: this.Content = new FileActionEditorControl(); break;
                    case ActionTypeEnum.GameQueue: this.Content = new GameQueueActionEditorControl(); break;
                    case ActionTypeEnum.IFTTT: this.Content = new IFTTTActionEditorControl(); break;
                    case ActionTypeEnum.Input: this.Content = new InputActionEditorControl(); break;
                    case ActionTypeEnum.Moderation: this.Content = new ModerationActionEditorControl(); break;
                    case ActionTypeEnum.Overlay: break;
                    case ActionTypeEnum.OvrStream: break;
                    case ActionTypeEnum.Serial: break;
                    case ActionTypeEnum.Sound: break;
                    case ActionTypeEnum.SpecialIdentifier: break;
                    case ActionTypeEnum.StreamingSoftware: break;
                    case ActionTypeEnum.Streamlabs: break;
                    case ActionTypeEnum.TextToSpeech: break;
                    case ActionTypeEnum.Translation: break;
                    case ActionTypeEnum.Twitch: break;
                    case ActionTypeEnum.Twitter: break;
                    case ActionTypeEnum.Wait: break;
                    case ActionTypeEnum.WebRequest: break;
                }

                if (this.IsLoaded)
                {
                    this.ActionContentControl.Content = this.Control;
                }
            }
        }
    }
}
