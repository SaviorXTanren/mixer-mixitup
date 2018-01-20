using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for AdvancedCommandEditorControl.xaml
    /// </summary>
    public partial class AdvancedCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private CommandDetailsControlBase commandDetailsControl;
        private ObservableCollection<ActionContainerControl> actionControls;

        private CommandBase newCommand = null;

        public AdvancedCommandEditorControl(CommandWindow window, CommandDetailsControlBase commandDetailsControl)
        {
            this.window = window;
            this.commandDetailsControl = commandDetailsControl;

            InitializeComponent();

            this.actionControls = new ObservableCollection<ActionContainerControl>();
        }

        public override void OnWindowSizeChanged(Size size)
        {
            foreach (ActionContainerControl controlContainer in this.actionControls)
            {
                controlContainer.OnWindowSizeChanged(size);
            }
        }

        public override CommandBase GetExistingCommand() { return this.commandDetailsControl.GetExistingCommand(); }

        public override void MoveActionUp(ActionContainerControl control)
        {
            int index = this.actionControls.IndexOf(control);
            index = MathHelper.Clamp((index - 1), 0, this.actionControls.Count() - 1);

            this.actionControls.Remove(control);
            this.actionControls.Insert(index, control);
        }

        public override void MoveActionDown(ActionContainerControl control)
        {
            int index = this.actionControls.IndexOf(control);
            index = MathHelper.Clamp((index + 1), 0, this.actionControls.Count() - 1);

            this.actionControls.Remove(control);
            this.actionControls.Insert(index, control);
        }

        public override void DeleteAction(ActionContainerControl control)
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
                    ActionContainerControl actionControl = new ActionContainerControl(this.window, this, action);
                    actionControl.Minimize();
                    this.actionControls.Add(actionControl);
                    actionControl.OnWindowSizeChanged(this.window.RenderSize);
                }
            }

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
                ActionContainerControl actionControl = new ActionContainerControl(this.window, this, type);
                this.actionControls.Add(actionControl);
                actionControl.OnWindowSizeChanged(this.window.RenderSize);

                this.TypeComboBox.SelectedIndex = -1;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.newCommand = await this.GetNewCommand();
            if (this.newCommand != null)
            {
                this.CommandSavedSuccessfully(this.newCommand);

                await this.window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

                this.window.Close();
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

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            CommandBase command = await this.GetNewCommand();
            if (command != null)
            {
                await this.window.RunAsyncOperation(async () =>
                {
                    string fileName = ChannelSession.Services.FileService.ShowSaveFileDialog(command.Name + ".mixitupc");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        await SerializerHelper.SerializeToFile(fileName, command);
                    }
                });
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                string fileName = ChannelSession.Services.FileService.ShowOpenFileDialog("Mix It Up Command (*.mixitupc)|*.mixitupc|All files (*.*)|*.*");
                if (!string.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        CustomCommand command = await SerializerHelper.DeserializeFromFile<CustomCommand>(fileName);
                        if (command != null && command.Actions != null)
                        {
                            foreach (ActionBase action in command.Actions)
                            {
                                ActionContainerControl actionControl = new ActionContainerControl(this.window, this, action);
                                actionControl.Minimize();
                                this.actionControls.Add(actionControl);
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }
            });
        }

        private async Task<CommandBase> GetNewCommand()
        {
            if (!await this.commandDetailsControl.Validate())
            {
                return null;
            }

            if (this.actionControls.Count == 0)
            {
                await MessageBoxHelper.ShowMessageDialog("At least one action must be created");
                return null;
            }

            List<ActionBase> actions = await this.GetActions();
            if (actions.Count == 0)
            {
                return null;
            }

            this.newCommand = await this.window.RunAsyncOperation(async () => { return await this.commandDetailsControl.GetNewCommand(); });
            if (this.newCommand != null)
            {
                this.newCommand.Actions.Clear();
                this.newCommand.Actions = actions;
            }
            return this.newCommand;
        }
    }
}
