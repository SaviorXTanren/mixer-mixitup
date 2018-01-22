using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for InteractiveActionControl.xaml
    /// </summary>
    public partial class InteractiveActionControl : ActionControlBase
    {
        private enum InteractiveTypeEnum
        {
            [Name("Move User To Group")]
            AddUserToGroup,
            [Name("Move Group To Scene")]
            MoveGroupToScene,
        }

        private InteractiveAction action;

        public InteractiveActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public InteractiveActionControl(ActionContainerControl containerControl, InteractiveAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.InteractiveTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveTypeEnum>();
            this.PermissionsAllowedComboBox.ItemsSource = ChatCommand.PermissionsAllowedValues;

            if (this.action != null)
            {
                this.InteractiveGroupNameGrid.Visibility = Visibility.Visible;
                this.InteractiveGroupNameTextBox.Text = this.action.GroupName;

                this.PermissionsAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.RoleRequirement);

                if (!string.IsNullOrEmpty(this.action.MoveGroupToScene))
                {
                    this.InteractiveTypeComboBox.SelectedItem = EnumHelper.GetEnumName(InteractiveTypeEnum.MoveGroupToScene);

                    this.InteractiveMoveToSceneGrid.Visibility = Visibility.Visible;
                    this.InteractiveMoveToSceneTextBox.Text = this.action.MoveGroupToScene;
                }
                else
                {
                    this.InteractiveTypeComboBox.SelectedItem = EnumHelper.GetEnumName(InteractiveTypeEnum.AddUserToGroup);
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.InteractiveTypeComboBox.SelectedIndex >= 0)
            {
                InteractiveTypeEnum interactiveType = EnumHelper.GetEnumValueFromString<InteractiveTypeEnum>((string)this.InteractiveTypeComboBox.SelectedItem);

                if (interactiveType == InteractiveTypeEnum.AddUserToGroup && !string.IsNullOrEmpty(this.InteractiveGroupNameTextBox.Text) && this.PermissionsAllowedComboBox.SelectedIndex >= 0)
                {
                    return new InteractiveAction(this.InteractiveGroupNameTextBox.Text, EnumHelper.GetEnumValueFromString<UserRole>((string)this.PermissionsAllowedComboBox.SelectedItem));
                }
                else if (interactiveType == InteractiveTypeEnum.MoveGroupToScene && !string.IsNullOrEmpty(this.InteractiveGroupNameTextBox.Text) &&
                    this.PermissionsAllowedComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.InteractiveMoveToSceneTextBox.Text))
                {
                    return new InteractiveAction(this.InteractiveGroupNameTextBox.Text, this.InteractiveMoveToSceneTextBox.Text,
                        EnumHelper.GetEnumValueFromString<UserRole>((string)this.PermissionsAllowedComboBox.SelectedItem));
                }
            }
            return null;
        }

        private void InteractiveTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.InteractiveGroupNameGrid.Visibility = Visibility.Hidden;
            this.InteractiveMoveToSceneGrid.Visibility = Visibility.Hidden;
            if (this.InteractiveTypeComboBox.SelectedIndex >= 0)
            {
                InteractiveTypeEnum interactiveType = EnumHelper.GetEnumValueFromString<InteractiveTypeEnum>((string)this.InteractiveTypeComboBox.SelectedItem);
                if (interactiveType == InteractiveTypeEnum.AddUserToGroup)
                {
                    this.InteractiveGroupNameGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveTypeEnum.MoveGroupToScene)
                {
                    this.InteractiveGroupNameGrid.Visibility = Visibility.Visible;
                    this.InteractiveMoveToSceneGrid.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
