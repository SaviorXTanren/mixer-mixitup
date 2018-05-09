using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Requirement;
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
        private InteractiveAction action;

        public InteractiveActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public InteractiveActionControl(ActionContainerControl containerControl, InteractiveAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.InteractiveTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveActionTypeEnum>();
            this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.InteractiveMoveUserToScenePermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;

            if (this.action != null)
            {
                this.InteractiveTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.InteractiveType);
                if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveUserToGroup)
                {
                    this.InteractiveMoveUserToGroupGroupNameTextBox.Text = this.action.GroupName;
                    this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.RoleRequirement);
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveGroupToScene)
                {
                    this.InteractiveMoveGroupToSceneGroupNameTextBox.Text = this.action.GroupName;
                    this.InteractiveMoveGroupToSceneSceneIDTextBox.Text = this.action.SceneID;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveUserToScene)
                {
                    this.InteractiveMoveUserToScenePermissionsAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.RoleRequirement);
                    this.InteractiveMoveUserToSceneSceneIDTextBox.Text = this.action.SceneID;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.CooldownButton || this.action.InteractiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    this.action.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    this.InteractiveCooldownNameTextBox.Text = this.action.CooldownID;
                    this.InteractiveCooldownAmountTextBox.Text = this.action.CooldownAmount.ToString();
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.InteractiveTypeComboBox.SelectedIndex >= 0)
            {
                InteractiveActionTypeEnum interactiveType = EnumHelper.GetEnumValueFromString<InteractiveActionTypeEnum>((string)this.InteractiveTypeComboBox.SelectedItem);

                if (interactiveType == InteractiveActionTypeEnum.MoveUserToGroup && !string.IsNullOrEmpty(this.InteractiveMoveUserToGroupGroupNameTextBox.Text) &&
                    this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.SelectedIndex >= 0)
                {
                    return new InteractiveAction(interactiveType, this.InteractiveMoveUserToGroupGroupNameTextBox.Text, null,
                        EnumHelper.GetEnumValueFromString<UserRole>((string)this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.SelectedItem));
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveGroupToScene && !string.IsNullOrEmpty(this.InteractiveMoveGroupToSceneGroupNameTextBox.Text) &&
                    !string.IsNullOrEmpty(this.InteractiveMoveGroupToSceneSceneIDTextBox.Text))
                {
                    return new InteractiveAction(interactiveType, this.InteractiveMoveGroupToSceneGroupNameTextBox.Text, this.InteractiveMoveGroupToSceneSceneIDTextBox.Text);
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveUserToScene && this.InteractiveMoveUserToScenePermissionsAllowedComboBox.SelectedIndex >= 0 &&
                    !string.IsNullOrEmpty(this.InteractiveMoveUserToSceneSceneIDTextBox.Text))
                {
                    return new InteractiveAction(interactiveType, this.InteractiveMoveUserToSceneSceneIDTextBox.Text, this.InteractiveMoveUserToSceneSceneIDTextBox.Text,
                        EnumHelper.GetEnumValueFromString<UserRole>((string)this.InteractiveMoveUserToScenePermissionsAllowedComboBox.SelectedItem));
                }
                else if (interactiveType == InteractiveActionTypeEnum.CooldownButton || interactiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    interactiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    if (!string.IsNullOrEmpty(this.InteractiveCooldownNameTextBox.Text) && int.TryParse(this.InteractiveCooldownAmountTextBox.Text, out int cooldownAmount) && cooldownAmount > 0)
                    {
                        return new InteractiveAction(interactiveType, this.InteractiveCooldownNameTextBox.Text, cooldownAmount);
                    }
                }
            }
            return null;
        }

        private void InteractiveTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.InteractiveMoveUserToGroupGrid.Visibility = Visibility.Hidden;
            this.InteractiveMoveGroupToSceneGrid.Visibility = Visibility.Hidden;
            this.InteractiveMoveUserToSceneGrid.Visibility = Visibility.Hidden;
            this.InteractiveCooldownGrid.Visibility = Visibility.Hidden;
            if (this.InteractiveTypeComboBox.SelectedIndex >= 0)
            {
                InteractiveActionTypeEnum interactiveType = EnumHelper.GetEnumValueFromString<InteractiveActionTypeEnum>((string)this.InteractiveTypeComboBox.SelectedItem);
                if (interactiveType == InteractiveActionTypeEnum.MoveUserToGroup)
                {
                    this.InteractiveMoveUserToGroupGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveGroupToScene)
                {
                    this.InteractiveMoveGroupToSceneGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveUserToScene)
                {
                    this.InteractiveMoveUserToSceneGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.CooldownButton || interactiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    interactiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    this.InteractiveCooldownGrid.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
