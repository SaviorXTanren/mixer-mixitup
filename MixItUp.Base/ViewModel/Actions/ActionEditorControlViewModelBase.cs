using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Actions
{
    public abstract class ActionEditorControlViewModelBase : UIViewModelBase
    {
        public abstract ActionTypeEnum Type { get; }

        public virtual string HelpLinkIdentifier { get { return this.Type.ToString(); } }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public ICommand PlayCommand { get; private set; }
        public ICommand MoveUpCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand HelpCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                this.enabled = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(Disabled));
            }
        }
        private bool enabled;

        public bool Disabled
        {
            get { return !this.enabled; }
        }

        public bool IsMinimized
        {
            get { return this.isMinimized; }
            set
            {
                this.isMinimized = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowNameTextBlock");
                this.NotifyPropertyChanged("ShowNameTextBox");
            }
        }
        private bool isMinimized;

        public bool ShowNameTextBlock { get { return this.IsMinimized; } }

        public bool ShowNameTextBox { get { return !this.IsMinimized; } }

        private ActionEditorListControlViewModel actionEditorListControlViewModel;

        public ActionEditorControlViewModelBase(ActionModelBase action)
        {
            this.Name = action.Name;
            this.Enabled = action.Enabled;
            this.IsMinimized = true;
        }

        public ActionEditorControlViewModelBase()
        {
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.Type);
            this.Enabled = true;
        }

        protected override Task OnOpenInternal()
        {
            this.PlayCommand = this.CreateCommand(async () =>
            {
                ActionModelBase action = await this.ValidateAndGetAction();
                if (action != null)
                {
                    await action.TestPerform(this.actionEditorListControlViewModel.GetTestSpecialIdentifiers());
                }
            });

            this.MoveUpCommand = this.CreateCommand(() =>
            {
                this.actionEditorListControlViewModel.MoveActionUp(this);
            });

            this.MoveDownCommand = this.CreateCommand(() =>
            {
                this.actionEditorListControlViewModel.MoveActionDown(this);
            });

            this.CopyCommand = this.CreateCommand(async () =>
            {
                await this.actionEditorListControlViewModel.DuplicateAction(this);
            });

            this.HelpCommand = this.CreateCommand(() =>
            {
                string actionPageName = string.Empty;
                switch (this.Type)
                {
                    case ActionTypeEnum.Chat: actionPageName = "chat-action"; break;
                    case ActionTypeEnum.Command: actionPageName = "command-action"; break;
                    case ActionTypeEnum.Conditional: actionPageName = "conditional-action"; break;
                    case ActionTypeEnum.Consumables: actionPageName = "consumable-action"; break;
                    case ActionTypeEnum.Counter: actionPageName = "counter-action"; break;
                    case ActionTypeEnum.Discord: actionPageName = "discord-action"; break;
                    case ActionTypeEnum.ExternalProgram: actionPageName = "external-program-action"; break;
                    case ActionTypeEnum.File: actionPageName = "file-action"; break;
                    case ActionTypeEnum.GameQueue: actionPageName = "game-queue-action"; break;
                    case ActionTypeEnum.Group: actionPageName = "group-action"; break;
                    case ActionTypeEnum.IFTTT: actionPageName = "ifttt-action"; break;
                    case ActionTypeEnum.InfiniteAlbum: actionPageName = "infinite-album"; break;
                    case ActionTypeEnum.Input: actionPageName = "input-action"; break;
                    case ActionTypeEnum.LumiaStream: actionPageName = "lumia-stream-action"; break;
                    case ActionTypeEnum.MeldStudio: actionPageName = "meld-studio-action"; break;
                    case ActionTypeEnum.Moderation: actionPageName = "moderation-action"; break;
                    case ActionTypeEnum.MtionStudio: actionPageName = "mtion-studio-action"; break;
                    case ActionTypeEnum.MusicPlayer: actionPageName = "music-player-action"; break;
                    case ActionTypeEnum.Overlay: actionPageName = "overlay-action"; break;
                    case ActionTypeEnum.OvrStream: actionPageName = "ovrstream-action"; break;
                    case ActionTypeEnum.PixelChat: actionPageName = "pixel-chat-action"; break;
                    case ActionTypeEnum.PolyPop: actionPageName = "polypop-action"; break;
                    case ActionTypeEnum.Random: actionPageName = "random-action"; break;
                    case ActionTypeEnum.Repeat: actionPageName = "repeat-action"; break;
                    case ActionTypeEnum.SAMMI: actionPageName = "sammi-action"; break;
                    case ActionTypeEnum.Script: actionPageName = "script-action"; break;
                    case ActionTypeEnum.Serial: actionPageName = "serial-action"; break;
                    case ActionTypeEnum.Sound: actionPageName = "sound-action"; break;
                    case ActionTypeEnum.SpecialIdentifier: actionPageName = "special-identifier-action"; break;
                    case ActionTypeEnum.StreamingSoftware: actionPageName = "streaming-software-action"; break;
                    case ActionTypeEnum.Streamlabs: actionPageName = "streamlabs-action"; break;
                    case ActionTypeEnum.TextToSpeech: actionPageName = "text-to-speech-action"; break;
                    case ActionTypeEnum.TITS: actionPageName = "tits-action"; break;
                    case ActionTypeEnum.Trovo: actionPageName = "trovo-action"; break;
                    case ActionTypeEnum.Twitch: actionPageName = "twitch-action"; break;
                    case ActionTypeEnum.Voicemod: actionPageName = "voicemod-action"; break;
                    case ActionTypeEnum.VTSPog: actionPageName = "vts-pog-action"; break;
                    case ActionTypeEnum.VTubeStudio: actionPageName = "vtube-studio-action"; break;
                    case ActionTypeEnum.Wait: actionPageName = "wait-action"; break;
                    case ActionTypeEnum.WebRequest: actionPageName = "web-request-action"; break;
                    case ActionTypeEnum.YouTube: actionPageName = "youtube-action"; break;
                    default:
                        actionPageName = this.Type.ToString().ToLower() + "-action";
                        break;
                }

                ServiceManager.Get<IProcessService>().LaunchLink("https://wiki.mixitupapp.com/actions/" + actionPageName);
            });

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.actionEditorListControlViewModel.DeleteAction(this);
            });

            return Task.CompletedTask;
        }

        public void Initialize(ActionEditorListControlViewModel actionEditorListControlViewModel)
        {
            this.actionEditorListControlViewModel = actionEditorListControlViewModel;
        }

        public virtual Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        public async Task<ActionModelBase> GetAction()
        {
            ActionModelBase action = await this.GetActionInternal();
            if (action != null)
            {
                action.Name = this.Name;
                action.Enabled = this.Enabled;
            }
            return action;
        }

        public async Task<ActionModelBase> ValidateAndGetAction()
        {
            Result result = await this.Validate();
            if (!result.Success)
            {
                await DialogHelper.ShowFailedResult(result);
                return null;
            }
            return await this.GetAction();
        }

        protected abstract Task<ActionModelBase> GetActionInternal();
    }
}
