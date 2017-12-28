using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using System;
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
        public event EventHandler<CommandBase> CommandSaveSuccessfully;

        private CommandDetailsControlBase commandDetailsControl;

        private ObservableCollection<ActionContainerControl> actionControls;

        private bool allowActionChanges;

        private CommandBase newCommand = null;

        public CommandWindow(CommandDetailsControlBase commandDetailsControl, bool allowActionChanges = true) 
        {
            this.commandDetailsControl = commandDetailsControl;
            this.allowActionChanges = allowActionChanges;

            InitializeComponent();

            CommandBase command = this.commandDetailsControl.GetExistingCommand();
            if (command != null)
            {
                this.Title = this.Title + " - " + command.Name;
            }

            this.actionControls = new ObservableCollection<ActionContainerControl>();

            this.Initialize(this.StatusBar);
        }

        public void MoveActionUp(ActionContainerControl control)
        {
            int index = this.actionControls.IndexOf(control);
            index = MathHelper.Clamp((index - 1), 0, this.actionControls.Count() - 1);

            this.actionControls.Remove(control);
            this.actionControls.Insert(index, control);
        }

        public void MoveActionDown(ActionContainerControl control)
        {
            int index = this.actionControls.IndexOf(control);
            index = MathHelper.Clamp((index + 1), 0, this.actionControls.Count() - 1);

            this.actionControls.Remove(control);
            this.actionControls.Insert(index, control);
        }

        public void DeleteAction(ActionContainerControl control)
        {
            this.actionControls.Remove(control);
        }

        protected override async Task OnLoaded()
        {
            List<string> actionTypes = EnumHelper.GetEnumNames<ActionTypeEnum>().ToList();
            actionTypes.Remove(EnumHelper.GetEnumName(ActionTypeEnum.Custom));
            this.TypeComboBox.ItemsSource = actionTypes.OrderBy(s => s);

            this.ActionsListView.ItemsSource = this.actionControls;

            this.CommandDetailsGrid.Visibility = Visibility.Visible;
            this.CommandDetailsGrid.Children.Add(this.commandDetailsControl);
            await this.commandDetailsControl.Initialize();

            CommandBase command = this.commandDetailsControl.GetExistingCommand();
            if (command != null)
            {
                foreach (ActionBase action in command.Actions)
                {
                    ActionContainerControl actionControl = new ActionContainerControl(this, action);
                    actionControl.Minimize();
                    this.actionControls.Add(actionControl);
                }
            }

            this.ActionsListView.IsEnabled = this.allowActionChanges;

            await base.OnLoaded();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                foreach (ActionContainerControl control in this.actionControls)
                {
                    control.Minimize();
                }

                ActionTypeEnum type = EnumHelper.GetEnumValueFromString<ActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                ActionContainerControl actionControl = new ActionContainerControl(this, type);
                this.actionControls.Add(actionControl);

                this.TypeComboBox.SelectedIndex = -1;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await this.commandDetailsControl.Validate())
            {
                return;
            }

            if (this.actionControls.Count == 0)
            {
                await MessageBoxHelper.ShowMessageDialog("At least one action must be created");
                return;
            }

            List<ActionBase> actions = await this.GetActions();
            if (actions.Count == 0)
            {
                return;
            }

            this.newCommand = await this.RunAsyncOperation(async () => { return await this.commandDetailsControl.GetNewCommand(); });
            if (this.newCommand != null)
            {
                this.newCommand.Actions.Clear();
                this.newCommand.Actions = actions;

                if (this.CommandSaveSuccessfully != null)
                {
                    this.CommandSaveSuccessfully(this, this.newCommand);
                }

                await this.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

                this.Close();
            }
        }

        private async Task<List<ActionBase>> GetActions()
        {
            List<ActionBase> actions = new List<ActionBase>();
            foreach (ActionContainerControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("Required action information is missing");
                    return new List<ActionBase>();
                }
                actions.Add(action);
            }
            return actions;
        }
    }
}
