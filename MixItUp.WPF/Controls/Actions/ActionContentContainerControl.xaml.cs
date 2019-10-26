using MixItUp.Base.Actions;
using MixItUp.WPF.Util;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ActionContentContainerControl.xaml
    /// </summary>
    public partial class ActionContentContainerControl : UserControl
    {
        private ContentControl ActionControlContentControl;

        private ActionBase action;
        private ActionTypeEnum type;

        private ActionControlBase actionControl;

        public ActionContentContainerControl()
        {
            InitializeComponent();
        }

        public void AssignAction(ActionBase action)
        {
            this.action = action;
            this.type = action.Type;
            this.SetControlContent();
        }

        public void AssignAction(ActionTypeEnum type)
        {
            this.action = null;
            this.type = type;
            this.ResetAction();
            this.SetControlContent();
        }

        public void ResetAction()
        {
            this.ActionControlContentControl = (ContentControl)this.GetByUid("ActionControlContentControl");
            if (this.ActionControlContentControl != null)
            {
                this.ActionControlContentControl.Content = null;
            }
            this.actionControl = null;
        }

        public ActionBase GetAction()
        {
            ActionBase action = null;
            if (this.actionControl != null)
            {
                action = this.actionControl.GetAction();
            }
            return action;
        }

        private void SetControlContent()
        {
            switch (this.type)
            {
                case ActionTypeEnum.Chat:
                    this.actionControl = (this.action != null) ? new ChatActionControl((ChatAction)this.action) : new ChatActionControl();
                    break;
                case ActionTypeEnum.Currency:
                    this.actionControl = (this.action != null) ? new CurrencyActionControl((CurrencyAction)this.action) : new CurrencyActionControl();
                    break;
                case ActionTypeEnum.ExternalProgram:
                    this.actionControl = (this.action != null) ? new ExternalProgramActionControl((ExternalProgramAction)this.action) : new ExternalProgramActionControl();
                    break;
                case ActionTypeEnum.Input:
                    this.actionControl = (this.action != null) ? new InputActionControl((InputAction)this.action) : new InputActionControl();
                    break;
                case ActionTypeEnum.Overlay:
                    this.actionControl = (this.action != null) ? new OverlayActionControl((OverlayAction)this.action) : new OverlayActionControl();
                    break;
                case ActionTypeEnum.Sound:
                    this.actionControl = (this.action != null) ? new SoundActionControl((SoundAction)this.action) : new SoundActionControl();
                    break;
                case ActionTypeEnum.Wait:
                    this.actionControl = (this.action != null) ? new WaitActionControl((WaitAction)this.action) : new WaitActionControl();
                    break;
                case ActionTypeEnum.Counter:
                    this.actionControl = (this.action != null) ? new CounterActionControl((CounterAction)this.action) : new CounterActionControl();
                    break;
                case ActionTypeEnum.GameQueue:
                    this.actionControl = (this.action != null) ? new GameQueueActionControl((GameQueueAction)this.action) : new GameQueueActionControl();
                    break;
                case ActionTypeEnum.Interactive:
                    this.actionControl = (this.action != null) ? new InteractiveActionControl((InteractiveAction)this.action) : new InteractiveActionControl();
                    break;
                case ActionTypeEnum.TextToSpeech:
                    this.actionControl = (this.action != null) ? new TextToSpeechActionControl((TextToSpeechAction)this.action) : new TextToSpeechActionControl();
                    break;
                case ActionTypeEnum.WebRequest:
                    this.actionControl = (this.action != null) ? new WebRequestActionControl((WebRequestAction)this.action) : new WebRequestActionControl();
                    break;
#pragma warning disable CS0612 // Type or member is obsolete
                case ActionTypeEnum.ActionGroup:
                    if (this.action != null)
                    {
                        ActionGroupAction aaction = (ActionGroupAction)this.action;
                        this.actionControl = new CommandActionControl(new CommandAction()
                        {
                            CommandActionType = CommandActionTypeEnum.RunCommand,
                            CommandID = aaction.ActionGroupID
                        });
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                    break;
                case ActionTypeEnum.SpecialIdentifier:
                    this.actionControl = (this.action != null) ? new SpecialIdentifierActionControl((SpecialIdentifierAction)this.action) : new SpecialIdentifierActionControl();
                    break;
                case ActionTypeEnum.File:
                    this.actionControl = (this.action != null) ? new FileActionControl((FileAction)this.action) : new FileActionControl();
                    break;
                case ActionTypeEnum.SongRequest:
                    this.actionControl = (this.action != null) ? new SongRequestActionControl((SongRequestAction)this.action) : new SongRequestActionControl();
                    break;
                case ActionTypeEnum.Spotify:
                    this.actionControl = (this.action != null) ? new SpotifyActionControl((SpotifyAction)this.action) : new SpotifyActionControl();
                    break;
                case ActionTypeEnum.Discord:
                    this.actionControl = (this.action != null) ? new DiscordActionControl((DiscordAction)this.action) : new DiscordActionControl();
                    break;
                case ActionTypeEnum.Translation:
                    this.actionControl = (this.action != null) ? new TranslationActionControl((TranslationAction)this.action) : new TranslationActionControl();
                    break;
                case ActionTypeEnum.Twitter:
                    this.actionControl = (this.action != null) ? new TwitterActionControl((TwitterAction)this.action) : new TwitterActionControl();
                    break;
                case ActionTypeEnum.Conditional:
                    this.actionControl = (this.action != null) ? new ConditionalActionControl((ConditionalAction)this.action) : new ConditionalActionControl();
                    break;
                case ActionTypeEnum.StreamingSoftware:
                    this.actionControl = (this.action != null) ? new StreamingSoftwareActionControl((StreamingSoftwareAction)this.action) : new StreamingSoftwareActionControl();
                    break;
                case ActionTypeEnum.Streamlabs:
                    this.actionControl = (this.action != null) ? new StreamlabsActionControl((StreamlabsAction)this.action) : new StreamlabsActionControl();
                    break;
                case ActionTypeEnum.MixerClips:
                    this.actionControl = (this.action != null) ? new MixerClipsActionControl((MixerClipsAction)this.action) : new MixerClipsActionControl();
                    break;
                case ActionTypeEnum.Command:
                    this.actionControl = (this.action != null) ? new CommandActionControl((CommandAction)this.action) : new CommandActionControl();
                    break;
                case ActionTypeEnum.Serial:
                    this.actionControl = (this.action != null) ? new SerialActionControl((SerialAction)this.action) : new SerialActionControl();
                    break;
                case ActionTypeEnum.Moderation:
                    this.actionControl = (this.action != null) ? new ModerationActionControl((ModerationAction)this.action) : new ModerationActionControl();
                    break;
                case ActionTypeEnum.OvrStream:
                    this.actionControl = (this.action != null) ? new OvrStreamActionControl((OvrStreamAction)this.action) : new OvrStreamActionControl();
                    break;
                case ActionTypeEnum.StreamingPlatform:
                    this.actionControl = (this.action != null) ? new StreamingPlatformActionControl((StreamingPlatformAction)this.action) : new StreamingPlatformActionControl();
                    break;
                case ActionTypeEnum.IFTTT:
                    this.actionControl = (this.action != null) ? new IFTTTActionControl((IFTTTAction)this.action) : new IFTTTActionControl();
                    break;
            }

            this.ActionControlContentControl = (ContentControl)this.GetByUid("ActionControlContentControl");
            if (this.ActionControlContentControl != null && this.actionControl != null)
            {
                this.ActionControlContentControl.Content = this.actionControl;
            }
        }
    }
}
