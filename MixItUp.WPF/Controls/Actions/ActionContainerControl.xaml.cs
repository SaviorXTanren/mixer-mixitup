using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
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
        private const int MinimizedGroupBoxHeight = 34;

        public CommandWindow Window { get; private set; }
        public CommandEditorControlBase EditorControl { get; private set; }

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
            this.GroupBoxHeaderTextBox.Text = EnumHelper.GetEnumName(this.type);

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
                    case ActionTypeEnum.OBSStudio:
                        this.actionControl = (this.action != null) ? new OBSStudioActionControl(this, (OBSStudioAction)this.action) : new OBSStudioActionControl(this);
                        break;
                    case ActionTypeEnum.XSplit:
                        this.actionControl = (this.action != null) ? new XSplitActionControl(this, (XSplitAction)this.action) : new XSplitActionControl(this);
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
                    case ActionTypeEnum.StreamlabsOBS:
                        this.actionControl = (this.action != null) ? new StreamlabsOBSActionControl(this, (StreamlabsOBSAction)this.action) : new StreamlabsOBSActionControl(this);
                        break;
                }
            }

            if (this.actionControl != null)
            {
                this.ActionControlContentControl.Content = this.actionControl;
            }
        }

        public ActionBase GetAction()
        {
            if (this.actionControl != null)
            {
                return this.actionControl.GetAction();
            }
            return null;
        }

        public async Task RunAsyncOperation(Func<Task> function) { await this.Window.RunAsyncOperation(function); }

        public void Minimize() { this.GroupBox.Height = MinimizedGroupBoxHeight; }

        public void OnWindowSizeChanged(Size size)
        {
            this.GroupBox.MaxWidth = size.Width - 38;
        }

        public void GroupBoxHeader_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.GroupBox.Height == MinimizedGroupBoxHeight)
            {
                this.GroupBox.Height = Double.NaN;
            }
            else
            {
                this.Minimize();
            }
        }

        private async void PlayActionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ActionBase action = this.GetAction();
                if (action != null)
                {
                    await action.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "@" + ChannelSession.GetCurrentUser().UserName });
                }
                else
                {
                    await MessageBoxHelper.ShowMessageDialog("Required action information is missing");
                }
            });
        }

        private void MoveUpActionButton_Click(object sender, RoutedEventArgs e) { this.EditorControl.MoveActionUp(this); }

        private void MoveDownActionButton_Click(object sender, RoutedEventArgs e) { this.EditorControl.MoveActionDown(this); }

        private void DeleteActionButton_Click(object sender, RoutedEventArgs e) { this.EditorControl.DeleteAction(this); }

        private void ActionHelpButton_Click(object sender, RoutedEventArgs e)
        {
            string actionName = EnumHelper.GetEnumName(this.type);
            actionName = actionName.ToLower();
            actionName = actionName.Replace(" ", "-");
            actionName = actionName.Replace("/", "");
            Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Actions#" + actionName);
        }
    }
}
