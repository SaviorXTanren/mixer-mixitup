using MixItUp.Base.Model.Actions;
using MixItUp.Base.ViewModel.Actions;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ActionEditorContainerControl.xaml
    /// </summary>
    public partial class ActionEditorContainerControl : LoadingControlBase
    {
        public ActionEditorControlViewModelBase ViewModel { get; private set; }

        public ContentControl ContentControl { get; private set; }

        public ActionEditorControlBase ActionControl { get; private set; }

        public ActionEditorContainerControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.ContentControl = (ContentControl)this.GetByUid("ActionContentControl");

            if (this.IsLoaded && this.DataContext != null && this.DataContext is ActionEditorControlViewModelBase && this.ContentControl.Content == null)
            {
                this.ViewModel = (ActionEditorControlViewModelBase)this.DataContext;
                switch (this.ViewModel.Type)
                {
                    case ActionTypeEnum.Chat: this.ActionControl = new ChatActionEditorControl(); break;
                    case ActionTypeEnum.Command: this.ActionControl = new CommandActionEditorControl(); break;
                    case ActionTypeEnum.Conditional: this.ActionControl = new ConditionalActionEditorControl(); break;
                    case ActionTypeEnum.Consumables: this.ActionControl = new ConsumablesActionEditorControl(); break;
                    case ActionTypeEnum.Counter: this.ActionControl = new CounterActionEditorControl(); break;
                    case ActionTypeEnum.Discord: this.ActionControl = new DiscordActionEditorControl(); break;
                    case ActionTypeEnum.ExternalProgram: this.ActionControl = new ExternalProgramActionEditorControl(); break;
                    case ActionTypeEnum.File: this.ActionControl = new FileActionEditorControl(); break;
                    case ActionTypeEnum.GameQueue: this.ActionControl = new GameQueueActionEditorControl(); break;
                    case ActionTypeEnum.Group: this.ActionControl = new GroupActionEditorControl(); break;
                    case ActionTypeEnum.IFTTT: this.ActionControl = new IFTTTActionEditorControl(); break;
                    case ActionTypeEnum.InfiniteAlbum: this.ActionControl = new InfiniteAlbumActionEditorControl(); break;
                    case ActionTypeEnum.Input: this.ActionControl = new InputActionEditorControl(); break;
                    case ActionTypeEnum.LumiaStream: this.ActionControl = new LumiaStreamActionEditorControl(); break;
                    case ActionTypeEnum.MeldStudio: this.ActionControl = new MeldStudioActionEditorControl(); break;
                    case ActionTypeEnum.Moderation: this.ActionControl = new ModerationActionEditorControl(); break;
                    case ActionTypeEnum.MtionStudio: this.ActionControl = new MtionStudioActionEditorControl(); break;
                    case ActionTypeEnum.MusicPlayer: this.ActionControl = new MusicPlayerActionEditorControl(); break;
                    case ActionTypeEnum.Overlay: this.ActionControl = new OverlayActionEditorControl(); break;
                    case ActionTypeEnum.OvrStream: this.ActionControl = new OvrStreamActionEditorControl(); break;
                    case ActionTypeEnum.PixelChat: this.ActionControl = new PixelChatActionEditorControl(); break;
                    case ActionTypeEnum.PolyPop: this.ActionControl = new PolyPopActionEditorControl(); break;
                    case ActionTypeEnum.Random: this.ActionControl = new RandomActionEditorControl(); break;
                    case ActionTypeEnum.Repeat: this.ActionControl = new RepeatActionEditorControl(); break;
                    case ActionTypeEnum.SAMMI: this.ActionControl = new SAMMIActionEditorControl(); break;
                    case ActionTypeEnum.Script: this.ActionControl = new ScriptActionEditorControl(); break;
                    case ActionTypeEnum.Serial: this.ActionControl = new SerialActionEditorControl(); break;
                    case ActionTypeEnum.Sound: this.ActionControl = new SoundActionEditorControl(); break;
                    case ActionTypeEnum.SpecialIdentifier: this.ActionControl = new SpecialIdentifierActionEditorControl(); break;
                    case ActionTypeEnum.StreamingSoftware: this.ActionControl = new StreamingSoftwareActionEditorControl(); break;
                    case ActionTypeEnum.Streamlabs: this.ActionControl = new StreamlabsActionEditorControl(); break;
                    case ActionTypeEnum.TextToSpeech: this.ActionControl = new TextToSpeechActionEditorControl(); break;
                    case ActionTypeEnum.TITS: this.ActionControl = new TITSActionEditorControl(); break;
                    case ActionTypeEnum.Trovo: this.ActionControl = new TrovoActionEditorControl(); break;
                    case ActionTypeEnum.Twitch: this.ActionControl = new TwitchActionEditorControl(); break;
                    case ActionTypeEnum.Voicemod: this.ActionControl = new VoicemodActionEditorControl(); break;
                    case ActionTypeEnum.VTSPog: this.ActionControl = new VTSPogActionEditorControl(); break;
                    case ActionTypeEnum.VTubeStudio: this.ActionControl = new VTubeStudioActionEditorControl(); break;
                    case ActionTypeEnum.Wait: this.ActionControl = new WaitActionEditorControl(); break;
                    case ActionTypeEnum.WebRequest: this.ActionControl = new WebRequestActionEditorControl(); break;
                    case ActionTypeEnum.YouTube: this.ActionControl = new YouTubeActionEditorControl(); break;
                }

                if (this.ActionControl != null)
                {
                    this.ContentControl.Content = this.ActionControl;
                    if (this.ViewModel.IsMinimized)
                    {
                        this.ActionContainer.Minimize();
                    }
                }
            }
            return Task.CompletedTask;
        }

        private void ActionContainer_Maximized(object sender, RoutedEventArgs e)
        {
            this.ViewModel.IsMinimized = false;
        }

        private void ActionContainer_Minimized(object sender, RoutedEventArgs e)
        {
            this.ViewModel.IsMinimized = true;
        }
    }
}
