using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
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
        public MixPlayGameModel Game { get; private set; }
        public MixPlayGameVersionModel Version { get; private set; }
        public MixPlaySceneModel Scene { get; private set; }
        public MixPlayJoystickControlModel Control { get; private set; }

        private InteractiveJoystickCommand command;

        public InteractiveJoystickCommandDetailsControl(MixPlayGameModel game, MixPlayGameVersionModel version, InteractiveJoystickCommand command)
        {
            this.Game = game;
            this.Version = version;
            this.command = command;
            this.Control = command.Joystick;

            InitializeComponent();
        }

        public InteractiveJoystickCommandDetailsControl(MixPlayGameModel game, MixPlayGameVersionModel version, MixPlaySceneModel scene, MixPlayJoystickControlModel control)
        {
            this.Game = game;
            this.Version = version;
            this.Scene = scene;
            this.Control = control;

            InitializeComponent();
        }

        public override Task Initialize()
        {
            this.Requirements.HideSettingsRequirement();

            this.JoystickSetupComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveJoystickSetupType>();
            this.UpKeyComboBox.ItemsSource = this.RightKeyComboBox.ItemsSource = this.DownKeyComboBox.ItemsSource = this.LeftKeyComboBox.ItemsSource = EnumHelper.GetEnumNames<InputKeyEnum>().OrderBy(s => s);

            this.JoystickDeadZoneTextBox.Text = "20";
            this.MouseMovementMultiplierTextBox.Text = "1.0";

            this.Requirements.HideCooldownRequirement();

            if (this.Control != null)
            {
                this.NameTextBox.Text = this.Control.controlID;
            }

            if (this.Scene != null)
            {
                this.SceneTextBox.Text = this.Scene.sceneID;
            }

            if (this.command != null)
            {
                this.SceneTextBox.Text = this.command.SceneID;
                this.JoystickSetupComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.SetupType);
                this.JoystickDeadZoneTextBox.Text = (this.command.DeadZone * 100).ToString();
                this.MouseMovementMultiplierTextBox.Text = this.command.MouseMovementMultiplier.ToString();

                if (this.Game != null)
                {
                    this.Scene = this.Version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(this.command.SceneID));
                }
            }

            return Task.FromResult(0);
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

                this.command.Unlocked = this.UnlockedControl.Unlocked;
                this.command.Requirements = requirements;

                return this.command;
            }
            return null;
        }

        private string GetSelectedIndex(InputKeyEnum? inputKey)
        {
            if (inputKey.HasValue)
            {
                return EnumHelper.GetEnumName(inputKey.Value);
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
                    this.UpKeyComboBox.SelectedItem = GetSelectedIndex(this.command?.MappedKeys.ElementAtOrDefault(0));
                    this.RightKeyComboBox.SelectedItem = GetSelectedIndex(this.command?.MappedKeys.ElementAtOrDefault(1));
                    this.DownKeyComboBox.SelectedItem = GetSelectedIndex(this.command?.MappedKeys.ElementAtOrDefault(2));
                    this.LeftKeyComboBox.SelectedItem = GetSelectedIndex(this.command?.MappedKeys.ElementAtOrDefault(3));
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
                    else if (setup == InteractiveJoystickSetupType.DirectionalArrows || setup == InteractiveJoystickSetupType.MouseMovement)
                    {
                        this.UpKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Up);
                        this.RightKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Right);
                        this.DownKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Down);
                        this.LeftKeyComboBox.SelectedItem = EnumHelper.GetEnumName(InputKeyEnum.Left);
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
                await MessageBoxHelper.ShowTimedCustomDialog(new InteractiveJoystickSetupTestDialogControl((InteractiveJoystickCommand)command), 16000);
            }
        }
    }
}