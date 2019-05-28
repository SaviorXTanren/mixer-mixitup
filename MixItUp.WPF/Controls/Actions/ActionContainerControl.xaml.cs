using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ActionContainerControl.xaml
    /// </summary>
    public partial class ActionContainerControl : UserControl
    {
        public CommandWindow Window { get; private set; }
        public CommandEditorControlBase EditorControl { get; private set; }

        private TextBlock GroupBoxHeaderTextBlock;
        private TextBox GroupBoxHeaderTextBox;
        private ContentControl ActionControlContentControl;

        private ActionControlBase actionControl;

        private ActionBase action;
        private ActionTypeEnum type;

        public ActionContainerControl(CommandWindow window, CommandEditorControlBase editorControl, ActionTypeEnum type)
        {
            this.Window = window;
            this.EditorControl = editorControl;
            this.type = type;

            InitializeComponent();

            this.Loaded += ActionContainerControl_Loaded;
        }

        public ActionContainerControl(CommandWindow window, CommandEditorControlBase editorControl, ActionBase action) : this(window, editorControl, action.Type) { this.action = action; }

        private void ActionContainerControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.GroupBoxHeaderTextBlock = (TextBlock)this.GetByUid("GroupBoxHeaderTextBlock");
            this.GroupBoxHeaderTextBox = (TextBox)this.GetByUid("GroupBoxHeaderTextBox");
            this.ActionControlContentControl = (ContentControl)this.GetByUid("ActionControlContentControl");

            if (this.actionControl == null)
            {
                switch (this.type)
                {
                    case ActionTypeEnum.Chat:
                        this.actionControl = (this.action != null) ? new ChatActionControl(this, (ChatAction)this.action) : new ChatActionControl(this);
                        break;
                    case ActionTypeEnum.Currency:
                        this.actionControl = (this.action != null) ? new CurrencyActionControl(this, (CurrencyAction)this.action) : new CurrencyActionControl(this);
                        break;
                    case ActionTypeEnum.ExternalProgram:
                        this.actionControl = (this.action != null) ? new ExternalProgramActionControl(this, (ExternalProgramAction)this.action) : new ExternalProgramActionControl(this);
                        break;
                    case ActionTypeEnum.Input:
                        this.actionControl = (this.action != null) ? new InputActionControl(this, (InputAction)this.action) : new InputActionControl(this);
                        break;
                    case ActionTypeEnum.Overlay:
                        this.actionControl = (this.action != null) ? new OverlayActionControl(this, (OverlayAction)this.action) : new OverlayActionControl(this);
                        break;
                    case ActionTypeEnum.Sound:
                        this.actionControl = (this.action != null) ? new SoundActionControl(this, (SoundAction)this.action) : new SoundActionControl(this);
                        break;
                    case ActionTypeEnum.Wait:
                        this.actionControl = (this.action != null) ? new WaitActionControl(this, (WaitAction)this.action) : new WaitActionControl(this);
                        break;
                    case ActionTypeEnum.Counter:
                        this.actionControl = (this.action != null) ? new CounterActionControl(this, (CounterAction)this.action) : new CounterActionControl(this);
                        break;
                    case ActionTypeEnum.GameQueue:
                        this.actionControl = (this.action != null) ? new GameQueueActionControl(this, (GameQueueAction)this.action) : new GameQueueActionControl(this);
                        break;
                    case ActionTypeEnum.Interactive:
                        this.actionControl = (this.action != null) ? new InteractiveActionControl(this, (InteractiveAction)this.action) : new InteractiveActionControl(this);
                        break;
                    case ActionTypeEnum.TextToSpeech:
                        this.actionControl = (this.action != null) ? new TextToSpeechActionControl(this, (TextToSpeechAction)this.action) : new TextToSpeechActionControl(this);
                        break;
                    case ActionTypeEnum.WebRequest:
                        this.actionControl = (this.action != null) ? new WebRequestActionControl(this, (WebRequestAction)this.action) : new WebRequestActionControl(this);
                        break;
                    case ActionTypeEnum.ActionGroup:
                        this.actionControl = (this.action != null) ? new ActionGroupActionControl(this, (ActionGroupAction)this.action) : new ActionGroupActionControl(this);
                        break;
                    case ActionTypeEnum.SpecialIdentifier:
                        this.actionControl = (this.action != null) ? new SpecialIdentifierActionControl(this, (SpecialIdentifierAction)this.action) : new SpecialIdentifierActionControl(this);
                        break;
                    case ActionTypeEnum.File:
                        this.actionControl = (this.action != null) ? new FileActionControl(this, (FileAction)this.action) : new FileActionControl(this);
                        break;
                    case ActionTypeEnum.SongRequest:
                        this.actionControl = (this.action != null) ? new SongRequestActionControl(this, (SongRequestAction)this.action) : new SongRequestActionControl(this);
                        break;
                    case ActionTypeEnum.Spotify:
                        this.actionControl = (this.action != null) ? new SpotifyActionControl(this, (SpotifyAction)this.action) : new SpotifyActionControl(this);
                        break;
                    case ActionTypeEnum.Discord:
                        this.actionControl = (this.action != null) ? new DiscordActionControl(this, (DiscordAction)this.action) : new DiscordActionControl(this);
                        break;
                    case ActionTypeEnum.Translation:
                        this.actionControl = (this.action != null) ? new TranslationActionControl(this, (TranslationAction)this.action) : new TranslationActionControl(this);
                        break;
                    case ActionTypeEnum.Twitter:
                        this.actionControl = (this.action != null) ? new TwitterActionControl(this, (TwitterAction)this.action) : new TwitterActionControl(this);
                        break;
                    case ActionTypeEnum.Conditional:
                        this.actionControl = (this.action != null) ? new ConditionalActionControl(this, (ConditionalAction)this.action) : new ConditionalActionControl(this);
                        break;
                    case ActionTypeEnum.StreamingSoftware:
                        this.actionControl = (this.action != null) ? new StreamingSoftwareActionControl(this, (StreamingSoftwareAction)this.action) : new StreamingSoftwareActionControl(this);
                        break;
                    case ActionTypeEnum.Streamlabs:
                        this.actionControl = (this.action != null) ? new StreamlabsActionControl(this, (StreamlabsAction)this.action) : new StreamlabsActionControl(this);
                        break;
                    case ActionTypeEnum.MixerClips:
                        this.actionControl = (this.action != null) ? new MixerClipsActionControl(this, (MixerClipsAction)this.action) : new MixerClipsActionControl(this);
                        break;
                    case ActionTypeEnum.Command:
                        this.actionControl = (this.action != null) ? new CommandActionControl(this, (CommandAction)this.action) : new CommandActionControl(this);
                        break;
                    case ActionTypeEnum.Serial:
                        this.actionControl = (this.action != null) ? new SerialActionControl(this, (SerialAction)this.action) : new SerialActionControl(this);
                        break;
                    case ActionTypeEnum.Moderation:
                        this.actionControl = (this.action != null) ? new ModerationActionControl(this, (ModerationAction)this.action) : new ModerationActionControl(this);
                        break;
                    case ActionTypeEnum.OvrStream:
                        this.actionControl = (this.action != null) ? new OvrStreamActionControl(this, (OvrStreamAction)this.action) : new OvrStreamActionControl(this);
                        break;
                    case ActionTypeEnum.StreamingPlatform:
                        this.actionControl = (this.action != null) ? new StreamingPlatformActionControl(this, (StreamingPlatformAction)this.action) : new StreamingPlatformActionControl(this);
                        break;
                    case ActionTypeEnum.IFTTT:
                        this.actionControl = (this.action != null) ? new IFTTTActionControl(this, (IFTTTAction)this.action) : new IFTTTActionControl(this);
                        break;
                }
            }

            if (this.action != null && !string.IsNullOrEmpty(this.action.Label))
            {
                this.GroupBoxHeaderTextBox.Text = this.GroupBoxHeaderTextBlock.Text = this.action.Label;
            }

            if (this.actionControl != null)
            {
                this.ActionControlContentControl.Content = this.actionControl;
            }

            if (string.IsNullOrEmpty(this.GroupBoxHeaderTextBox.Text))
            {
                this.GroupBoxHeaderTextBox.Text = this.GroupBoxHeaderTextBlock.Text = EnumHelper.GetEnumName(this.type);
            }

            if (this.ActionContainer.IsMinimized)
            {
                this.AccordianGroupBoxControl_Minimized(this, new RoutedEventArgs());
            }
        }

        public ActionBase GetAction()
        {
            ActionBase action = null;
            if (this.actionControl != null)
            {
                action = this.actionControl.GetAction();
                if (action != null && !string.IsNullOrEmpty(this.GroupBoxHeaderTextBox.Text))
                {
                    action.Label = this.GroupBoxHeaderTextBox.Text;
                }
            }
            return action;
        }

        public async Task RunAsyncOperation(Func<Task> function) { await this.Window.RunAsyncOperation(function); }

        public void Minimize()
        {
            this.ActionContainer.Minimize();
        }

        private void AccordianGroupBoxControl_Maximized(object sender, RoutedEventArgs e)
        {
            if (this.GroupBoxHeaderTextBlock != null && this.GroupBoxHeaderTextBox != null)
            {
                this.GroupBoxHeaderTextBlock.Visibility = Visibility.Collapsed;
                this.GroupBoxHeaderTextBox.Visibility = Visibility.Visible;
            }
        }

        private void AccordianGroupBoxControl_Minimized(object sender, RoutedEventArgs e)
        {
            if (this.GroupBoxHeaderTextBlock != null && this.GroupBoxHeaderTextBox != null)
            {
                this.GroupBoxHeaderTextBlock.Visibility = Visibility.Visible;
                this.GroupBoxHeaderTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void GroupBoxHeaderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.GroupBoxHeaderTextBlock.Text = this.GroupBoxHeaderTextBox.Text;
        }

        private async void PlayActionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ActionBase action = this.GetAction();
                if (action != null)
                {
                    UserViewModel currentUser = await ChannelSession.GetCurrentUser();
                    await action.Perform(currentUser, new List<string>() { "@" + currentUser.UserName }, new Dictionary<string, string>());
                }
                else
                {
                    await MessageBoxHelper.ShowMessageDialog("Required action information is missing");
                }
            });
        }

        private void MoveUpActionButton_Click(object sender, RoutedEventArgs e) { this.EditorControl.MoveActionUp(this); }

        private void MoveDownActionButton_Click(object sender, RoutedEventArgs e) { this.EditorControl.MoveActionDown(this); }

        private void ActionHelpButton_Click(object sender, RoutedEventArgs e)
        {
            string actionName = EnumHelper.GetEnumName(this.type);
            actionName = actionName.ToLower();
            actionName = actionName.Replace(" ", "-");
            actionName = actionName.Replace("/", "");
            Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Actions#" + actionName);
        }

        private async void ActionDuplicateButton_Click(object sender, RoutedEventArgs e)
        {
            ActionBase action = this.GetAction();
            if (action == null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    await MessageBoxHelper.ShowMessageDialog("Required action information is missing");
                });
            }
            else
            {
                this.EditorControl.DuplicateAction(this);
            }
        }

        private void DeleteActionButton_Click(object sender, RoutedEventArgs e) { this.EditorControl.DeleteAction(this); }
    }
}