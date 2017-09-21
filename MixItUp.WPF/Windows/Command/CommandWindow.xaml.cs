using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Command
{
    /// <summary>
    /// Interaction logic for CommandWindow.xaml
    /// </summary>
    public partial class CommandWindow : LoadingWindowBase
    {
        private class TestCommand : CommandBase { }

        private CommandDetailsControlBase commandDetailsControl;

        private ObservableCollection<ActionControl> actionControls;

        public CommandWindow(CommandDetailsControlBase commandDetailsControl)
        {
            this.commandDetailsControl = commandDetailsControl;

            InitializeComponent();

            this.actionControls = new ObservableCollection<ActionControl>();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames(this.commandDetailsControl.GetAllowedActions()).OrderBy(s => s);

            this.ActionsListView.ItemsSource = this.actionControls;

            this.CommandDetailsGrid.Visibility = Visibility.Visible;
            this.CommandDetailsGrid.Children.Add(this.commandDetailsControl);
            await this.commandDetailsControl.Initialize();

            CommandBase command = this.commandDetailsControl.GetExistingCommand();

            if (command != null)
            {
                foreach (ActionBase action in command.Actions)
                {
                    ActionControl actionControl = new ActionControl(action);
                    actionControl.OnActionDelete += this.OnActionDeleted;
                    this.actionControls.Add(actionControl);
                }
            }

            await base.OnLoaded();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                ActionTypeEnum type = EnumHelper.GetEnumValueFromString<ActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                ActionControl actionControl = new ActionControl(type);
                actionControl.OnActionDelete += this.OnActionDeleted;
                this.actionControls.Add(actionControl);

                this.TypeComboBox.SelectedIndex = -1;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.commandDetailsControl.Validate())
            {
                return;
            }

            if (this.actionControls.Count == 0)
            {
                MessageBoxHelper.ShowDialog("At least one action must be created");
                return;
            }

            List<ActionBase> actions = this.GetActions();
            if (actions.Count == 0)
            {
                return;
            }

            CommandBase command = await this.RunAsyncOperation(async () => { return await this.commandDetailsControl.GetNewCommand(); });
            if (command != null)
            {
                command.Actions.Clear();
                command.Actions = actions;

                await this.RunAsyncOperation(async () => { await ChannelSession.Settings.Save(); });

                this.Close();
            }
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                TestCommand command = new TestCommand();
                command.Actions = this.GetActions();
                await command.Perform();
            });
        }

        private void OnActionDeleted(object sender, ActionBase e)
        {
            ActionControl control = (ActionControl)sender;
            this.actionControls.Remove(control);
        }

        private List<ActionBase> GetActions()
        {
            List<ActionBase> actions = new List<ActionBase>();
            foreach (ActionControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    MessageBoxHelper.ShowDialog("Required action information is missing");
                    return new List<ActionBase>();
                }
                actions.Add(action);
            }
            return actions;
        }
    }
}
