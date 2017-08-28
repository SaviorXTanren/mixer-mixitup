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
        private InteractiveCommand command;

        private ObservableCollection<ActionControl> actionControls;

        private List<ActionTypeEnum> allowedActions = new List<ActionTypeEnum>()
        {
            ActionTypeEnum.Chat, ActionTypeEnum.Cooldown, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram,
            ActionTypeEnum.Input, ActionTypeEnum.Overlay, ActionTypeEnum.Sound, ActionTypeEnum.Wait
        };

        public InteractiveCommandWindow() : this(null) { }

        public InteractiveCommandWindow(InteractiveCommand command)
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
            //if (this.EventTypeComboBox.SelectedIndex < 0)
            //{
            //    MessageBoxHelper.ShowError("An event type must be selected");
            //    return;
            //}

            //if (this.EventIDTextBox.IsEnabled && string.IsNullOrEmpty(this.EventIDTextBox.Text))
            //{
            //    MessageBoxHelper.ShowError("A name must be specified for this event type");
            //    return;
            //}

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
                this.command = new InteractiveCommand();
                ChannelSession.Settings.InteractiveControls.Add(this.command);
            }
            else
            {
                this.command.Actions.Clear();
            }

            this.command.Actions.Clear();
            this.command.Actions = newActions;

            this.Close();
        }
    }
}
