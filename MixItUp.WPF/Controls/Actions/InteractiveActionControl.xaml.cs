using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    public class CustomMetadataPair
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Interaction logic for InteractiveActionControl.xaml
    /// </summary>
    public partial class InteractiveActionControl : ActionControlBase
    {
        private InteractiveAction action;

        private ObservableCollection<MixPlayGameModel> games = new ObservableCollection<MixPlayGameModel>();

        private ObservableCollection<CustomMetadataPair> customMetadataPairs = new ObservableCollection<CustomMetadataPair>();

        public InteractiveActionControl() : base() { InitializeComponent(); }

        public InteractiveActionControl(InteractiveAction action) : this() { this.action = action; }

        public override async Task OnLoaded()
        {
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveActionTypeEnum>().OrderBy(s => s);
            this.MoveUserToGroupPermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.MoveUserToScenePermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.MoveAllUsersToGroupPermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.MoveAllUsersToScenePermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.UpdateControlTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveActionUpdateControlTypeEnum>().OrderBy(s => s);

            this.MoveUserToGroupPermissionsAllowedComboBox.SelectedIndex = 0;
            this.MoveUserToScenePermissionsAllowedComboBox.SelectedIndex = 0;

            this.GameComboBox.ItemsSource = games;

            this.CustomMetadataItemsControl.ItemsSource = this.customMetadataPairs;
            this.customMetadataPairs.Add(new CustomMetadataPair());

            foreach (MixPlayGameModel game in await ChannelSession.Interactive.GetAllConnectableGames())
            {
                this.games.Add(game);
            }

            if (this.action != null)
            {
                this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.InteractiveType);
                if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveUserToGroup)
                {
                    this.MoveUserToGroupGroupNameTextBox.Text = this.action.GroupName;
                    this.MoveUserToGroupPermissionsAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.RoleRequirement);
                    this.MoveUserToGroupUserNameTextBox.Text = this.action.OptionalUserName;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveGroupToScene)
                {
                    this.MoveGroupToSceneGroupNameTextBox.Text = this.action.GroupName;
                    this.MoveGroupToSceneSceneIDTextBox.Text = this.action.SceneID;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveUserToScene)
                {
                    this.MoveUserToScenePermissionsAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.RoleRequirement);
                    this.MoveUserToSceneSceneIDTextBox.Text = this.action.SceneID;
                    this.MoveUserToSceneUserNameTextBox.Text = this.action.OptionalUserName;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveAllUsersToGroup)
                {
                    this.MoveAllUsersToGroupPermissionsAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.RoleRequirement);
                    this.MoveAllUsersToGroupGroupNameTextBox.Text = this.action.GroupName;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveAllUsersToScene)
                {
                    this.MoveAllUsersToScenePermissionsAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.RoleRequirement);
                    this.MoveAllUsersToSceneSceneNameTextBox.Text = this.action.SceneID;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.CooldownButton || this.action.InteractiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    this.action.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    this.CooldownNameTextBox.Text = this.action.CooldownID;
                    this.CooldownAmountTextBox.Text = this.action.CooldownAmount.ToString();
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.Connect)
                {
                    this.GameComboBox.SelectedItem = this.games.FirstOrDefault(g => g.id.Equals(this.action.InteractiveGameID));
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.UpdateControl)
                {
                    this.UpdateControlNameTextBox.Text = this.action.ControlID;
                    this.UpdateControlTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.UpdateControlType);
                    this.UpdateControlValueTextBox.Text = this.action.UpdateValue;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.SetCustomMetadata)
                {
                    this.CustomMetadataControlIDTextBox.Text = this.action.ControlID;
                    this.customMetadataPairs.Clear();
                    foreach (var kvp in this.action.CustomMetadata)
                    {
                        this.customMetadataPairs.Add(new CustomMetadataPair() { Name = kvp.Key, Value = kvp.Value });
                    }
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.EnableDisableControl)
                {
                    this.EnableDisableControlNameTextBox.Text = this.action.ControlID;
                    this.EnableDisableControlToggleButton.IsChecked = this.action.EnableDisableControl;
                }
            }
        }

        public override ActionBase GetAction()
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                InteractiveActionTypeEnum interactiveType = EnumHelper.GetEnumValueFromString<InteractiveActionTypeEnum>((string)this.TypeComboBox.SelectedItem);

                if (interactiveType == InteractiveActionTypeEnum.MoveUserToGroup && !string.IsNullOrEmpty(this.MoveUserToGroupGroupNameTextBox.Text) &&
                    this.MoveUserToGroupPermissionsAllowedComboBox.SelectedIndex >= 0)
                {
                    return InteractiveAction.CreateMoveUserToGroupAction(this.MoveUserToGroupGroupNameTextBox.Text,
                        EnumHelper.GetEnumValueFromString<UserRoleEnum>((string)this.MoveUserToGroupPermissionsAllowedComboBox.SelectedItem),
                        this.MoveUserToGroupUserNameTextBox.Text);
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveUserToScene && this.MoveUserToScenePermissionsAllowedComboBox.SelectedIndex >= 0 &&
                    !string.IsNullOrEmpty(this.MoveUserToSceneSceneIDTextBox.Text))
                {
                    return InteractiveAction.CreateMoveUserToSceneAction(this.MoveUserToSceneSceneIDTextBox.Text,
                        EnumHelper.GetEnumValueFromString<UserRoleEnum>((string)this.MoveUserToScenePermissionsAllowedComboBox.SelectedItem),
                        this.MoveUserToSceneUserNameTextBox.Text);
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveGroupToScene && !string.IsNullOrEmpty(this.MoveGroupToSceneGroupNameTextBox.Text) &&
                    !string.IsNullOrEmpty(this.MoveGroupToSceneSceneIDTextBox.Text))
                {
                    return InteractiveAction.CreateMoveGroupToSceneAction(this.MoveGroupToSceneGroupNameTextBox.Text, this.MoveGroupToSceneSceneIDTextBox.Text);
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveAllUsersToGroup && !string.IsNullOrEmpty(this.MoveAllUsersToGroupGroupNameTextBox.Text))
                {
                    return InteractiveAction.CreateMoveAllUsersToGroupAction(this.MoveAllUsersToGroupGroupNameTextBox.Text,
                        EnumHelper.GetEnumValueFromString<UserRoleEnum>((string)this.MoveAllUsersToGroupPermissionsAllowedComboBox.SelectedItem));
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveAllUsersToScene && !string.IsNullOrEmpty(this.MoveAllUsersToSceneSceneNameTextBox.Text))
                {
                    return InteractiveAction.CreateMoveAllUsersToSceneAction(this.MoveAllUsersToSceneSceneNameTextBox.Text,
                        EnumHelper.GetEnumValueFromString<UserRoleEnum>((string)this.MoveAllUsersToScenePermissionsAllowedComboBox.SelectedItem));
                }
                else if (interactiveType == InteractiveActionTypeEnum.CooldownButton || interactiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    interactiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    if (!string.IsNullOrEmpty(this.CooldownNameTextBox.Text) && int.TryParse(this.CooldownAmountTextBox.Text, out int cooldownAmount) && cooldownAmount > 0)
                    {
                        return InteractiveAction.CreateCooldownAction(interactiveType, this.CooldownNameTextBox.Text, cooldownAmount);
                    }
                }
                else if (interactiveType == InteractiveActionTypeEnum.Connect)
                {
                    if (this.GameComboBox.SelectedIndex >= 0)
                    {
                        MixPlayGameModel game = (MixPlayGameModel)this.GameComboBox.SelectedItem;
                        return InteractiveAction.CreateConnectAction(game);
                    }
                }
                else if (interactiveType == InteractiveActionTypeEnum.Disconnect)
                {
                    return new InteractiveAction(interactiveType);
                }
                else if (interactiveType == InteractiveActionTypeEnum.UpdateControl)
                {
                    if (!string.IsNullOrEmpty(this.UpdateControlNameTextBox.Text) && this.UpdateControlTypeComboBox.SelectedIndex >= 0 &&
                        !string.IsNullOrEmpty(this.UpdateControlValueTextBox.Text))
                    {
                        return InteractiveAction.CreateUpdateControlAction(
                            EnumHelper.GetEnumValueFromString<InteractiveActionUpdateControlTypeEnum>((string)this.UpdateControlTypeComboBox.SelectedItem),
                            this.UpdateControlNameTextBox.Text, this.UpdateControlValueTextBox.Text);
                    }
                }
                else if (interactiveType == InteractiveActionTypeEnum.SetCustomMetadata)
                {
                    if (!string.IsNullOrEmpty(this.CustomMetadataControlIDTextBox.Text) && this.customMetadataPairs.Count > 0)
                    {
                        foreach (CustomMetadataPair pair in this.customMetadataPairs)
                        {
                            if (string.IsNullOrEmpty(pair.Name) || string.IsNullOrEmpty(pair.Value))
                            {
                                return null;
                            }
                        }
                        return InteractiveAction.CreateSetCustomMetadataAction(this.CustomMetadataControlIDTextBox.Text, this.customMetadataPairs.ToDictionary(p => p.Name, p => p.Value));
                    }
                }
                else if (interactiveType == InteractiveActionTypeEnum.EnableDisableControl)
                {
                    if (!string.IsNullOrEmpty(this.EnableDisableControlNameTextBox.Text))
                    {
                        return InteractiveAction.CreateEnableDisableControlAction(this.EnableDisableControlNameTextBox.Text, this.EnableDisableControlToggleButton.IsChecked.GetValueOrDefault());
                    }
                }
            }
            return null;
        }

        private void InteractiveTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MoveUserToGroupGrid.Visibility = Visibility.Collapsed;
            this.MoveGroupToSceneGrid.Visibility = Visibility.Collapsed;
            this.MoveUserToSceneGrid.Visibility = Visibility.Collapsed;
            this.MoveAllUsersToGroupGrid.Visibility = Visibility.Collapsed;
            this.MoveAllUsersToSceneGrid.Visibility = Visibility.Collapsed;
            this.CooldownGrid.Visibility = Visibility.Collapsed;
            this.ConnectGrid.Visibility = Visibility.Collapsed;
            this.UpdateControlGrid.Visibility = Visibility.Collapsed;
            this.SetCustomMetadataGrid.Visibility = Visibility.Collapsed;
            this.EnableDisableControlGrid.Visibility = Visibility.Collapsed;
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                InteractiveActionTypeEnum interactiveType = EnumHelper.GetEnumValueFromString<InteractiveActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (interactiveType == InteractiveActionTypeEnum.MoveUserToGroup)
                {
                    this.MoveUserToGroupGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveGroupToScene)
                {
                    this.MoveGroupToSceneGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveUserToScene)
                {
                    this.MoveUserToSceneGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveAllUsersToGroup)
                {
                    this.MoveAllUsersToGroupGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveAllUsersToScene)
                {
                    this.MoveAllUsersToSceneGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.CooldownButton || interactiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    interactiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    this.CooldownGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.Connect)
                {
                    this.ConnectGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.UpdateControl)
                {
                    this.UpdateControlGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.SetCustomMetadata)
                {
                    this.SetCustomMetadataGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.EnableDisableControl)
                {
                    this.EnableDisableControlGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void AddCustomMetadataButton_Click(object sender, RoutedEventArgs e)
        {
            this.customMetadataPairs.Add(new CustomMetadataPair());
        }

        private void DeleteCustomMetadataButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CustomMetadataPair pair = (CustomMetadataPair)button.DataContext;
            this.customMetadataPairs.Remove(pair);
        }
    }
}
