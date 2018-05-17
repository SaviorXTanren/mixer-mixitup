using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for InteractiveJoystickCommandDetailsControl.xaml
    /// </summary>
    public partial class InteractiveJoystickCommandDetailsControl : CommandDetailsControlBase
    {
        public InteractiveGameListingModel Game { get; private set; }
        public InteractiveGameVersionModel Version { get; private set; }
        public InteractiveSceneModel Scene { get; private set; }
        public InteractiveJoystickControlModel Control { get; private set; }

        private InteractiveJoystickCommand command;

        public InteractiveJoystickCommandDetailsControl(InteractiveJoystickCommand command)
        {
            this.command = command;
            this.Control = command.Joystick;

            InitializeComponent();
        }

        public InteractiveJoystickCommandDetailsControl(InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene, InteractiveJoystickControlModel control)
        {
            this.Game = game;
            this.Version = version;
            this.Scene = scene;
            this.Control = control;

            InitializeComponent();
        }

        public override async Task Initialize()
        {
            this.JoystickSetupComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveJoystickSetupType>();
            this.UpKeyComboBox.ItemsSource = this.RightKeyComboBox.ItemsSource = this.DownKeyComboBox.ItemsSource = this.LeftKeyComboBox.ItemsSource = EnumHelper.GetEnumNames<InputKeyEnum>().OrderBy(s => s);

            this.JoystickDeadZoneTextBox.Text = "20";
            this.MouseMovementMultiplierTextBox.Text = "1.0";

            this.Requirements.HideCooldownRequirement();

            if (this.command != null)
            {
                this.JoystickSetupComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.SetupType);
                this.JoystickDeadZoneTextBox.Text = (this.command.DeadZone * 100).ToString();
                this.MouseMovementMultiplierTextBox.Text = this.command.MouseMovementMultiplier.ToString();

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
            if (this.JoystickSetupComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A Joystick Setup must be selected");
                return false;
            }
            InteractiveJoystickSetupType setup = EnumHelper.GetEnumValueFromString<InteractiveJoystickSetupType>((string)this.JoystickSetupComboBox.SelectedItem);

            if (!int.TryParse(this.JoystickDeadZoneTextBox.Text, out int deadzone) || deadzone < 0 || deadzone > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid Joystick Dead Zone must be entered between 0 & 100");
                return false;
            }

            if (setup == InteractiveJoystickSetupType.MouseMovement)
            {
                if (!double.TryParse(this.MouseMovementMultiplierTextBox.Text, out double mouseMultiplier) || mouseMultiplier < 1.0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A valid Movement Multiplier must be entered that is 1.0 or greater");
                    return false;
                }
            }

            if (!await this.Requirements.Validate())
            {
                return false;
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                InteractiveJoystickSetupType setup = EnumHelper.GetEnumValueFromString<InteractiveJoystickSetupType>((string)this.JoystickSetupComboBox.SelectedItem);

                RequirementViewModel requirements = this.Requirements.GetRequirements();

                if (this.command == null)
                {
                    this.command = new InteractiveJoystickCommand(this.Game, this.Scene, this.Control, requirements);
                    ChannelSession.Settings.InteractiveCommands.Add(this.command);
                }
                this.command.InitializeAction();

                this.command.SetupType = setup;
                this.command.DeadZone = (double.Parse(this.JoystickDeadZoneTextBox.Text) / 100.0);
                this.command.MappedKeys.Clear();

                if (setup == InteractiveJoystickSetupType.MouseMovement)
                {
                    this.command.MouseMovementMultiplier = double.Parse(this.MouseMovementMultiplierTextBox.Text);
                }
                else if (setup == InteractiveJoystickSetupType.MapToIndividualKeys)
                {
                    if (this.UpKeyComboBox.SelectedIndex >= 0) { this.command.MappedKeys.Add(EnumHelper.GetEnumValueFromString<InputKeyEnum>((string)this.UpKeyComboBox.SelectedItem)); } else { this.command.MappedKeys.Add(null); }
                    if (this.RightKeyComboBox.SelectedIndex >= 0) { this.command.MappedKeys.Add(EnumHelper.GetEnumValueFromString<InputKeyEnum>((string)this.RightKeyComboBox.SelectedItem)); } else { this.command.MappedKeys.Add(null); }
                    if (this.DownKeyComboBox.SelectedIndex >= 0) { this.command.MappedKeys.Add(EnumHelper.GetEnumValueFromString<InputKeyEnum>((string)this.DownKeyComboBox.SelectedItem)); } else { this.command.MappedKeys.Add(null); }
                    if (this.LeftKeyComboBox.SelectedIndex >= 0) { this.command.MappedKeys.Add(EnumHelper.GetEnumValueFromString<InputKeyEnum>((string)this.LeftKeyComboBox.SelectedItem)); } else { this.command.MappedKeys.Add(null); }
                }
                this.command.Requirements = requirements;
                this.command.Unlocked = this.UnlockedControl.Unlocked;

                await ChannelSession.Connection.UpdateInteractiveGameVersion(this.Version);
                return this.command;
            }
            return null;
        }

        private void JoystickSetupComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.JoystickSetupComboBox.SelectedIndex >= 0)
            {
                InteractiveJoystickSetupType setup = EnumHelper.GetEnumValueFromString<InteractiveJoystickSetupType>((string)this.JoystickSetupComboBox.SelectedItem);
                if (setup == InteractiveJoystickSetupType.MapToIndividualKeys)
                {
                    this.UpKeyComboBox.IsEnabled = this.RightKeyComboBox.IsEnabled = this.DownKeyComboBox.IsEnabled = this.LeftKeyComboBox.IsEnabled = true;
                    this.UpKeyComboBox.SelectedIndex = this.RightKeyComboBox.SelectedIndex = this.DownKeyComboBox.SelectedIndex = this.LeftKeyComboBox.SelectedIndex = -1;
                }
                else
                {
                    this.UpKeyComboBox.IsEnabled = this.RightKeyComboBox.IsEnabled = this.DownKeyComboBox.IsEnabled = this.LeftKeyComboBox.IsEnabled = false;
                    if (setup == InteractiveJoystickSetupType.WASD)
                    {
                        this.UpKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.W);
                        this.RightKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.D);
                        this.DownKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.S);
                        this.LeftKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.A);
                    }
                    else if (setup == InteractiveJoystickSetupType.DirectionalArrows)
                    {
                        this.UpKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Up);
                        this.RightKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Right);
                        this.DownKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Down);
                        this.LeftKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Left);
                    }
                    else if (setup == InteractiveJoystickSetupType.MouseMovement)
                    {
                        this.UpKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Y);
                        this.RightKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.X);
                        this.DownKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Y);
                        this.LeftKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.X);
                    }
                }

                this.MouseMovementMultiplierTextBox.Visibility = (setup == InteractiveJoystickSetupType.MouseMovement) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void TestSetupButton_Click(object sender, RoutedEventArgs e)
        {
            CommandBase command = await this.GetNewCommand();
            if (command != null)
            {
                await MessageBoxHelper.ShowTimedCustomDialog(new InteractiveJoystickSetupTestDialogControl((InteractiveJoystickCommand)command), 15000);
            }
        }
    }
}