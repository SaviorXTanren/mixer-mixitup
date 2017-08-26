using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Timers
{
    /// <summary>
    /// Interaction logic for TimerCommandWindow.xaml
    /// </summary>
    public partial class TimerCommandWindow : LoadingWindowBase
    {
        private TimerCommand command;

        private ObservableCollection<ActionControl> actionControls;

        private List<ActionTypeEnum> allowedActions = new List<ActionTypeEnum>()
        {
            ActionTypeEnum.Chat, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram,
            ActionTypeEnum.Input, ActionTypeEnum.Overlay, ActionTypeEnum.Sound, ActionTypeEnum.Wait
        };

        public TimerCommandWindow() : this(null) { }

        public TimerCommandWindow(TimerCommand command)
        {
            this.command = command;

            InitializeComponent();

            this.actionControls = new ObservableCollection<ActionControl>();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.ActionsListView.ItemsSource = this.actionControls;

            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                MessageBoxHelper.ShowError("Required command information is missing");
                return;
            }

            if (this.actionControls.Count == 0)
            {
                MessageBoxHelper.ShowError("At least one action must be created");
                return;
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
                this.command = new TimerCommand(this.NameTextBox.Text);
                ChannelSession.Settings.TimerCommands.Add(this.command);
            }
            else
            {
                this.command.Name = this.NameTextBox.Text;
                this.command.Actions.Clear();
            }

            this.command.Actions = newActions;

            this.Close();
        }
    }
}
