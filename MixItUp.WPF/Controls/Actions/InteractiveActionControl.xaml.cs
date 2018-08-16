using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
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

        private ObservableCollection<InteractiveGameModel> games = new ObservableCollection<InteractiveGameModel>();

        private ObservableCollection<CustomMetadataPair> customMetadataPairs = new ObservableCollection<CustomMetadataPair>();

        public InteractiveActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public InteractiveActionControl(ActionContainerControl containerControl, InteractiveAction action) : this(containerControl) { this.action = action; }

        public override async Task OnLoaded()
        {
            this.InteractiveTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveActionTypeEnum>().OrderBy(s => s);
            this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.InteractiveMoveUserToScenePermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
            this.InteractiveUpdateControlTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveActionUpdateControlTypeEnum>().OrderBy(s => s);

            this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.SelectedIndex = 0;
            this.InteractiveMoveUserToScenePermissionsAllowedComboBox.SelectedIndex = 0;

            this.InteractiveGameComboBox.ItemsSource = games;

            this.CustomMetadataItemsControl.ItemsSource = this.customMetadataPairs;
            this.customMetadataPairs.Add(new CustomMetadataPair());

            foreach (InteractiveGameModel game in await ChannelSession.Interactive.GetAllConnectableGames())
            {
                this.games.Add(game);
            }

            if (this.action != null)
            {
                this.InteractiveTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.InteractiveType);
                if (this.action.InteractiveType == InteractiveActionTypeEnum.MoveUserToGroup)
                {
                    this.InteractiveMoveUserToGroupGroupNameTextBox.Text = this.action.GroupName;
                    this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.RoleRequirement);
                    this.InteractiveMoveUserToGroupUserNameTextBox.Text = this.action.OptionalUserName;
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
                    this.InteractiveMoveUserToSceneUserNameTextBox.Text = this.action.OptionalUserName;
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.CooldownButton || this.action.InteractiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    this.action.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    this.InteractiveCooldownNameTextBox.Text = this.action.CooldownID;
                    this.InteractiveCooldownAmountTextBox.Text = this.action.CooldownAmount.ToString();
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.Connect)
                {
                    this.InteractiveGameComboBox.SelectedItem = this.games.FirstOrDefault(g => g.id.Equals(this.action.InteractiveGameID));
                }
                else if (this.action.InteractiveType == InteractiveActionTypeEnum.UpdateControl)
                {
                    this.InteractiveUpdateControlNameTextBox.Text = this.action.ControlID;
                    this.InteractiveUpdateControlTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.UpdateControlType);
                    this.InteractiveUpdateControlValueTextBox.Text = this.action.UpdateValue;
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
            }
        }

        public override ActionBase GetAction()
        {
            if (this.InteractiveTypeComboBox.SelectedIndex >= 0)
            {
                InteractiveActionTypeEnum interactiveType = EnumHelper.GetEnumValueFromString<InteractiveActionTypeEnum>((string)this.InteractiveTypeComboBox.SelectedItem);

                if (interactiveType == InteractiveActionTypeEnum.MoveUserToGroup && !string.IsNullOrEmpty(this.InteractiveMoveUserToGroupGroupNameTextBox.Text) &&
                    this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.SelectedIndex >= 0)
                {
                    return InteractiveAction.CreateMoveUserToGroupAction(this.InteractiveMoveUserToGroupGroupNameTextBox.Text,
                        EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.InteractiveMoveUserToGroupPermissionsAllowedComboBox.SelectedItem),
                        this.InteractiveMoveUserToGroupUserNameTextBox.Text);
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveUserToScene && this.InteractiveMoveUserToScenePermissionsAllowedComboBox.SelectedIndex >= 0 &&
                    !string.IsNullOrEmpty(this.InteractiveMoveUserToSceneSceneIDTextBox.Text))
                {
                    return InteractiveAction.CreateMoveUserToSceneAction(this.InteractiveMoveUserToSceneSceneIDTextBox.Text,
                        EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.InteractiveMoveUserToScenePermissionsAllowedComboBox.SelectedItem),
                        this.InteractiveMoveUserToSceneUserNameTextBox.Text);
                }
                else if (interactiveType == InteractiveActionTypeEnum.MoveGroupToScene && !string.IsNullOrEmpty(this.InteractiveMoveGroupToSceneGroupNameTextBox.Text) &&
                    !string.IsNullOrEmpty(this.InteractiveMoveGroupToSceneSceneIDTextBox.Text))
                {
                    return InteractiveAction.CreateMoveGroupToSceneAction(this.InteractiveMoveGroupToSceneGroupNameTextBox.Text, this.InteractiveMoveGroupToSceneSceneIDTextBox.Text);
                }
                else if (interactiveType == InteractiveActionTypeEnum.CooldownButton || interactiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    interactiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    if (!string.IsNullOrEmpty(this.InteractiveCooldownNameTextBox.Text) && int.TryParse(this.InteractiveCooldownAmountTextBox.Text, out int cooldownAmount) && cooldownAmount > 0)
                    {
                        return InteractiveAction.CreateCooldownAction(interactiveType, this.InteractiveCooldownNameTextBox.Text, cooldownAmount);
                    }
                }
                else if (interactiveType == InteractiveActionTypeEnum.Connect)
                {
                    if (this.InteractiveGameComboBox.SelectedIndex >= 0)
                    {
                        InteractiveGameModel game = (InteractiveGameModel)this.InteractiveGameComboBox.SelectedItem;
                        return InteractiveAction.CreateConnectAction(game);
                    }
                }
                else if (interactiveType == InteractiveActionTypeEnum.Disconnect)
                {
                    return new InteractiveAction(interactiveType);
                }
                else if (interactiveType == InteractiveActionTypeEnum.UpdateControl)
                {
                    if (!string.IsNullOrEmpty(this.InteractiveUpdateControlNameTextBox.Text) && this.InteractiveUpdateControlTypeComboBox.SelectedIndex >= 0 &&
                        !string.IsNullOrEmpty(this.InteractiveUpdateControlValueTextBox.Text))
                    {
                        return InteractiveAction.CreateUpdateControlAction(
                            EnumHelper.GetEnumValueFromString<InteractiveActionUpdateControlTypeEnum>((string)this.InteractiveUpdateControlTypeComboBox.SelectedItem),
                            this.InteractiveUpdateControlNameTextBox.Text, this.InteractiveUpdateControlValueTextBox.Text);
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
            }
            return null;
        }

        private void InteractiveTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.InteractiveMoveUserToGroupGrid.Visibility = Visibility.Collapsed;
            this.InteractiveMoveGroupToSceneGrid.Visibility = Visibility.Collapsed;
            this.InteractiveMoveUserToSceneGrid.Visibility = Visibility.Collapsed;
            this.InteractiveCooldownGrid.Visibility = Visibility.Collapsed;
            this.InteractiveConnectGrid.Visibility = Visibility.Collapsed;
            this.InteractiveUpdateControlGrid.Visibility = Visibility.Collapsed;
            this.InteractiveSetCustomMetadataGrid.Visibility = Visibility.Collapsed;
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
                else if (interactiveType == InteractiveActionTypeEnum.Connect)
                {
                    this.InteractiveConnectGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.UpdateControl)
                {
                    this.InteractiveUpdateControlGrid.Visibility = Visibility.Visible;
                }
                else if (interactiveType == InteractiveActionTypeEnum.SetCustomMetadata)
                {
                    this.InteractiveSetCustomMetadataGrid.Visibility = Visibility.Visible;
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
