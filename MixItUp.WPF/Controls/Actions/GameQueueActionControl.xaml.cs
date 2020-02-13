using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for GameQueueActionControl.xaml
    /// </summary>
    public partial class GameQueueActionControl : ActionControlBase
    {
        private GameQueueAction action;

        public GameQueueActionControl() : base() { InitializeComponent(); }

        public GameQueueActionControl(GameQueueAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.GameQueueActionTypeComboBox.ItemsSource = Enum.GetValues(typeof(GameQueueActionType))
                .Cast<GameQueueActionType>()
                .OrderBy(s => EnumLocalizationHelper.GetLocalizedName(s));
            if (this.action != null)
            {
                this.GameQueueActionTypeComboBox.SelectedItem = this.action.GameQueueType;
                this.RoleRequirement.SetRoleRequirement(this.action.RoleRequirement);
                this.TargetUsernameTextBox.Text = this.action.TargetUsername;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.GameQueueActionTypeComboBox.SelectedIndex >= 0)
            {
                GameQueueActionType gameQueueType = (GameQueueActionType)this.GameQueueActionTypeComboBox.SelectedItem;
                if (gameQueueType == GameQueueActionType.SelectFirstType)
                {
                    if (this.RoleRequirement.GetRoleRequirement() == null)
                    {
                        return null;
                    }
                    return new GameQueueAction(gameQueueType, this.RoleRequirement.GetRoleRequirement());
                }
                return new GameQueueAction(gameQueueType, targetUsername: this.TargetUsernameTextBox.Text);
            }
            return null;
        }

        private void GameQueueActionTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.GameQueueActionTypeComboBox.SelectedIndex >= 0)
            {
                GameQueueActionType gameQueueType = (GameQueueActionType)this.GameQueueActionTypeComboBox.SelectedItem;
                if (gameQueueType == GameQueueActionType.SelectFirstType)
                {
                    this.RoleRequirement.Visibility = Visibility.Visible;
                }
                else
                {
                    this.RoleRequirement.Visibility = Visibility.Collapsed;
                }

                if (gameQueueType == GameQueueActionType.JoinFrontOfQueue || gameQueueType == GameQueueActionType.JoinQueue ||
                    gameQueueType == GameQueueActionType.LeaveQueue || gameQueueType == GameQueueActionType.QueuePosition)
                {
                    this.TargetUsernameTextBox.Visibility = Visibility.Visible;
                }
                else
                {
                    this.TargetUsernameTextBox.Visibility = Visibility.Collapsed;
                    this.TargetUsernameTextBox.Text = string.Empty;
                }
            }
        }
    }
}
