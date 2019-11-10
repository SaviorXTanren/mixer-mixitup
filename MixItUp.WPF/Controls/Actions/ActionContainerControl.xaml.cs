using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using StreamingClient.Base.Util;
using System.Collections.Generic;
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
        private ActionContentContainerControl ActionContentContainerControl;

        private ActionBase action;
        private ActionTypeEnum type;

        public ActionContainerControl(CommandWindow window, CommandEditorControlBase editorControl, ActionTypeEnum type)
        {
            this.Window = window;
            this.EditorControl = editorControl;
            this.type = type;

            InitializeComponent();

            this.ActionContentContainerControl = (ActionContentContainerControl)this.GetByUid("ActionContentContainerControl");
            this.ActionContentContainerControl.AssignAction(this.type);

            this.Loaded += ActionContainerControl_Loaded;
        }

        public ActionContainerControl(CommandWindow window, CommandEditorControlBase editorControl, ActionBase action)
            : this(window, editorControl, action.Type)
        {
            this.action = action;
            this.ActionContentContainerControl.AssignAction(this.action);
        }

        private void ActionContainerControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.GroupBoxHeaderTextBlock = (TextBlock)this.GetByUid("GroupBoxHeaderTextBlock");
            this.GroupBoxHeaderTextBox = (TextBox)this.GetByUid("GroupBoxHeaderTextBox");

            if (this.action != null && !string.IsNullOrEmpty(this.action.Label))
            {
                this.GroupBoxHeaderTextBox.Text = this.GroupBoxHeaderTextBlock.Text = this.action.Label;
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
            ActionBase action = this.ActionContentContainerControl.GetAction();
            if (action != null && !string.IsNullOrEmpty(this.GroupBoxHeaderTextBox.Text))
            {
                action.Label = this.GroupBoxHeaderTextBox.Text;
            }
            return action;
        }

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
                    await DialogHelper.ShowMessage("Required action information is missing");
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
            ProcessHelper.LaunchLink("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Actions#" + actionName);
        }

        private async void ActionDuplicateButton_Click(object sender, RoutedEventArgs e)
        {
            ActionBase action = this.GetAction();
            if (action == null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    await DialogHelper.ShowMessage("Required action information is missing");
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