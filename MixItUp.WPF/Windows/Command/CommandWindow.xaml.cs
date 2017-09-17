using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Command
{
    /// <summary>
    /// Interaction logic for CommandWindow.xaml
    /// </summary>
    public partial class CommandWindow : LoadingWindowBase
    {
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
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames(this.commandDetailsControl.GetAllowedActions());

            this.ActionsListView.ItemsSource = this.actionControls;

            this.CommandDetailsGrid.Visibility = Visibility.Visible;
            this.CommandDetailsGrid.Children.Add(this.commandDetailsControl);
            await this.commandDetailsControl.Initialize();

            CommandBase command = this.commandDetailsControl.GetExistingCommand();

            if (command != null)
            {
                foreach (ActionBase action in command.Actions)
                {
                    this.actionControls.Add(new ActionControl(action));
                }
            }

            await base.OnLoaded();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                ActionTypeEnum type = EnumHelper.GetEnumValueFromString<ActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                this.actionControls.Add(new ActionControl(type));
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
                MessageBoxHelper.ShowError("At least one action must be created");
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

        private async void TestCommandButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                TestCommand command = new TestCommand();
                command.Actions = this.GetActions();
                await command.Perform();
            });
        }

        private List<ActionBase> GetActions()
        {
            List<ActionBase> actions = new List<ActionBase>();
            foreach (ActionControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    MessageBoxHelper.ShowError("Required action information is missing");
                    return new List<ActionBase>();
                }
                actions.Add(action);
            }
            return actions;
        }
    }
}
