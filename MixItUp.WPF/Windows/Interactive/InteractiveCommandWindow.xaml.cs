using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Interactive
{
    /// <summary>
    /// Interaction logic for InteractiveCommandWindow.xaml
    /// </summary>
    public partial class InteractiveCommandWindow : LoadingWindowBase
    {
        private InteractiveGameListingModel game;
        private InteractiveGameVersionModel version;
        private InteractiveSceneModel scene;
        private InteractiveControlModel control;

        private InteractiveCommand command;

        private ObservableCollection<ActionControl> actionControls;

        private List<ActionTypeEnum> allowedActions = new List<ActionTypeEnum>()
        {
            ActionTypeEnum.Chat, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram, ActionTypeEnum.Input, ActionTypeEnum.Overlay, ActionTypeEnum.Sound, ActionTypeEnum.Wait
        };

        public InteractiveCommandWindow(InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene, InteractiveControlModel control)
        {
            this.game = game;
            this.version = version;
            this.scene = scene;
            this.control = control;

            InitializeComponent();

            this.actionControls = new ObservableCollection<ActionControl>();

            this.Initialize(this.StatusBar);
        }

        public InteractiveCommandWindow(InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene, InteractiveCommand command)
            : this(game, version, scene, command.Control)
        {
            this.command = command;
        }

        protected override Task OnLoaded()
        {
            this.ActionsListView.ItemsSource = this.actionControls;

            this.ButtonTriggerComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveButtonCommandTriggerType>();

            if (this.control != null && this.control is InteractiveButtonControlModel)
            {
                this.ButtonTriggerComboBox.IsEnabled = true;
                this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(InteractiveButtonCommandTriggerType.MouseDown);
                this.SparkCostTextBox.IsEnabled = true;
                this.SparkCostTextBox.Text = ((InteractiveButtonControlModel)this.control).cost.ToString();
                this.CooldownTextBox.IsEnabled = true;
            }

            if (this.command != null)
            {
                if (this.command.Button != null)
                {
                    this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Trigger);                  
                    this.CooldownTextBox.Text = this.command.Cooldown.ToString();
                }

                foreach (ActionBase action in this.command.Actions)
                {
                    this.actionControls.Add(new ActionControl(allowedActions, action));
                }
            }

            return base.OnLoaded();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.actionControls.Add(new ActionControl(allowedActions));
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            InteractiveButtonCommandTriggerType trigger = InteractiveButtonCommandTriggerType.MouseDown;
            int sparkCost = 0;
            int cooldown = 0;

            if (this.control is InteractiveButtonControlModel)
            {
                if (this.ButtonTriggerComboBox.SelectedIndex < 0)
                {
                    MessageBoxHelper.ShowError("An trigger type must be selected");
                    return;
                }

                if (!int.TryParse(this.SparkCostTextBox.Text, out sparkCost) || sparkCost <= 0)
                {
                    MessageBoxHelper.ShowError("A valid spark cost must be entered");
                    return;
                }

                if (!int.TryParse(this.CooldownTextBox.Text, out cooldown) || cooldown <= 0)
                {
                    MessageBoxHelper.ShowError("A valid cooldown must be entered");
                    return;
                }
            }

            List<ActionBase> newActions = new List<ActionBase>();
            foreach (ActionControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    MessageBoxHelper.ShowError("Required action information is missing");
                    return;
                }
                newActions.Add(action);
            }

            if (this.command == null)
            {
                if (this.control is InteractiveButtonControlModel)
                {
                    this.command = new InteractiveCommand(this.game, this.scene, (InteractiveButtonControlModel)this.control, trigger);
                }
                else
                {
                    this.command = new InteractiveCommand(this.game, this.scene, (InteractiveJoystickControlModel)this.control);
                }
                ChannelSession.Settings.InteractiveControls.Add(this.command);
            }

            if (this.control is InteractiveButtonControlModel)
            {
                this.command.Button.cost = sparkCost;
                this.command.Cooldown = cooldown;
            }

            this.command.Actions.Clear();
            this.command.Actions = newActions;

            await this.RunAsyncOperation(async () => { await ChannelSession.MixerConnection.Interactive.UpdateInteractiveGameVersion(this.version); });

            this.Close();
        }
    }
}
