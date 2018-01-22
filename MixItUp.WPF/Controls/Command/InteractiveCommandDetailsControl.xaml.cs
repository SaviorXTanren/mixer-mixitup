using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for InteractiveCommandDetailsControl.xaml
    /// </summary>
    public partial class InteractiveCommandDetailsControl : CommandDetailsControlBase
    {
        public InteractiveGameListingModel Game { get; private set; }
        public InteractiveGameVersionModel Version { get; private set; }
        public InteractiveSceneModel Scene { get; private set; }
        public InteractiveControlModel Control { get; private set; }

        private InteractiveCommand command;

        public InteractiveCommandDetailsControl(InteractiveCommand command)
        {
            this.command = command;
            this.Control = command.Control;

            InitializeComponent();
        }

        public InteractiveCommandDetailsControl(InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene, InteractiveControlModel control)
        {
            this.Game = game;
            this.Version = version;
            this.Scene = scene;
            this.Control = control;

            InitializeComponent();
        }

        public override async Task Initialize()
        {
            foreach (var cooldownGroup in ChannelSession.Settings.InteractiveCooldownGroups.Keys.ToList())
            {
                if (!ChannelSession.Settings.InteractiveCommands.Any(c => cooldownGroup.Equals(c.CooldownGroup)))
                {
                    ChannelSession.Settings.InteractiveCooldownGroups.Remove(cooldownGroup);
                    continue;
                }
            }

            this.ButtonTriggerComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveButtonCommandTriggerType>();
            this.CooldownTypeComboBox.ItemsSource = new List<string>() { "Individual", "Group" };
            this.CooldownGroupsComboBox.ItemsSource = ChannelSession.Settings.InteractiveCooldownGroups.Keys;

            if (this.Control != null && this.Control is InteractiveButtonControlModel)
            {
                this.ButtonTriggerComboBox.IsEnabled = true;
                this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(InteractiveButtonCommandTriggerType.MouseDown);
                this.SparkCostTextBox.IsEnabled = true;
                this.SparkCostTextBox.Text = ((InteractiveButtonControlModel)this.Control).cost.ToString();
                this.CooldownGroupsComboBox.IsEnabled = true;
                this.CooldownTypeComboBox.IsEnabled = true;
                this.CooldownTextBox.IsEnabled = true;
            }

            if (this.command != null)
            {
                if (this.command.Button != null)
                {
                    this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Trigger);
                    if (!string.IsNullOrEmpty(this.command.CooldownGroup))
                    {
                        this.CooldownTypeComboBox.SelectedItem = "Group";
                        this.CooldownGroupsComboBox.SelectedItem = this.command.CooldownGroup;
                        this.CooldownTextBox.Text = ChannelSession.Settings.InteractiveCooldownGroups[this.command.CooldownGroup].ToString();
                    }
                    else
                    {
                        this.CooldownTypeComboBox.SelectedItem = "Individual";
                        this.CooldownTextBox.Text = this.command.IndividualCooldown.ToString();
                    }
                }

                IEnumerable<InteractiveGameListingModel> games = await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel);
                this.Game = games.FirstOrDefault(g => g.id.Equals(this.command.GameID));
                if (this.Game != null)
                {
                    this.Version = this.Game.versions.First();
                    this.Version = await ChannelSession.Connection.GetInteractiveGameVersion(this.Version);
                    this.Scene = this.Version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(this.command.SceneID));
                }
            }
        }

        public override async Task<bool> Validate()
        {
            int sparkCost = 0;
            int cooldown = 0;

            if (this.Control is InteractiveButtonControlModel)
            {
                if (this.ButtonTriggerComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("An trigger type must be selected");
                    return false;
                }

                if (!int.TryParse(this.SparkCostTextBox.Text, out sparkCost) || sparkCost < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A valid spark cost must be entered");
                    return false;
                }

                if (this.CooldownTypeComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A cooldown type must be selected");
                    return false;
                }

                if (this.CooldownTypeComboBox.SelectedItem.Equals("Group") && this.CooldownGroupsComboBox.SelectedIndex < 0 && string.IsNullOrEmpty(this.CooldownGroupsComboBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("A cooldown group must be selected or entered");
                    return false;
                }

                if (!string.IsNullOrEmpty(this.CooldownTextBox.Text))
                {
                    if (!int.TryParse(this.CooldownTextBox.Text, out cooldown) || cooldown < 0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("Cooldown must be 0 or greater");
                        return false;
                    }
                }
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                InteractiveButtonCommandTriggerType trigger = EnumHelper.GetEnumValueFromString<InteractiveButtonCommandTriggerType>((string)this.ButtonTriggerComboBox.SelectedItem);
                if (this.command == null)
                {
                    if (this.Control is InteractiveButtonControlModel)
                    {                
                        this.command = new InteractiveCommand(this.Game, this.Scene, (InteractiveButtonControlModel)this.Control, trigger);
                    }
                    else
                    {
                        this.command = new InteractiveCommand(this.Game, this.Scene, (InteractiveJoystickControlModel)this.Control);
                    }
                    ChannelSession.Settings.InteractiveCommands.Add(this.command);
                }

                if (this.Control is InteractiveButtonControlModel)
                {
                    this.command.Trigger = trigger;
                    this.command.Button.cost = int.Parse(this.SparkCostTextBox.Text);
                    if (this.CooldownTypeComboBox.SelectedItem.Equals("Group"))
                    {
                        string cooldownGroup = this.CooldownGroupsComboBox.Text;
                        this.command.CooldownGroup = cooldownGroup;
                        ChannelSession.Settings.InteractiveCooldownGroups[cooldownGroup] = int.Parse(this.CooldownTextBox.Text);

                        this.command.IndividualCooldown = 0;
                    }
                    else
                    {
                        int cooldown = 0;
                        if (!string.IsNullOrEmpty(this.CooldownTextBox.Text))
                        {
                            cooldown = int.Parse(this.CooldownTextBox.Text);
                        }
                        this.command.IndividualCooldown = cooldown;

                        this.command.CooldownGroup = null;
                    }

                    await ChannelSession.Connection.UpdateInteractiveGameVersion(this.Version);
                }
                return this.command;
            }
            return null;
        }

        private void CooldownTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.CooldownTypeComboBox.SelectedIndex >= 0)
            {
                string selection = (string)this.CooldownTypeComboBox.SelectedItem;
                if (selection.Equals("Group"))
                {
                    this.CooldownGroupsComboBox.Visibility = Visibility.Visible;
                }
                else if (selection.Equals("Individual"))
                {
                    this.CooldownGroupsComboBox.Visibility = Visibility.Collapsed;
                }
                this.CooldownGroupsComboBox.SelectedIndex = -1;
                this.CooldownTextBox.Clear();
            }
        }

        private void CooldownGroupsComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string selection = (string)e.AddedItems[0];
                if (ChannelSession.Settings.InteractiveCooldownGroups.ContainsKey(selection))
                {
                    this.CooldownTextBox.Text = ChannelSession.Settings.InteractiveCooldownGroups[selection].ToString();
                }
                else
                {
                    this.CooldownTextBox.Text = string.Empty;
                }
            }
        }
    }
}
