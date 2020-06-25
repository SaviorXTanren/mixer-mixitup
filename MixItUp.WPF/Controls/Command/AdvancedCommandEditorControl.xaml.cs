using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Windows.Command;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    public partial class AdvancedCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private CommandDetailsControlBase commandDetailsControl;

        private Guid storeListingID;
        private IEnumerable<ActionBase> initialActions;

        private ObservableCollection<ActionContainerControl> actionControls = new ObservableCollection<ActionContainerControl>();

        private CommandBase newCommand = null;
        private bool hasLoaded = false;

        public AdvancedCommandEditorControl(CommandWindow window, CommandDetailsControlBase commandDetailsControl, Guid storeListingID, IEnumerable<ActionBase> initialActions)
            : this(window, commandDetailsControl)
        {
            this.storeListingID = storeListingID;
            this.initialActions = initialActions;
        }

        public AdvancedCommandEditorControl(CommandWindow window, CommandDetailsControlBase commandDetailsControl)
        {
            this.window = window;
            this.commandDetailsControl = commandDetailsControl;

            InitializeComponent();
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

        public override void DuplicateAction(ActionContainerControl actionContainer)
        {
            List<ActionBase> actions = new List<ActionBase>() { actionContainer.GetAction() };
            actions = JSONSerializerHelper.Clone<List<ActionBase>>(actions);
            this.AddActionControlContainer(new ActionContainerControl(this.window, this, actions.First()));
        }

        public override void DeleteAction(ActionContainerControl control)
        {
            this.actionControls.Remove(control);
        }

        protected override async Task OnLoaded()
        {
            if (!hasLoaded)
            {
                var actionTypes = System.Enum.GetValues(typeof(ActionTypeEnum))
                    .Cast<ActionTypeEnum>()
                    .Where(v => !EnumHelper.IsObsolete(v) && v != ActionTypeEnum.Custom);

                this.TypeComboBox.ItemsSource = actionTypes.OrderBy(at => EnumLocalizationHelper.GetLocalizedName(at));
                this.ActionsItemsControl.ItemsSource = this.actionControls;

                this.CommandDetailsGrid.Visibility = Visibility.Visible;
                this.CommandDetailsGrid.Children.Add(this.commandDetailsControl);
                await this.commandDetailsControl.Initialize();

                List<ActionBase> actions = new List<ActionBase>();

                CommandBase command = this.commandDetailsControl.GetExistingCommand();
                if (command != null && command.Actions.Count > 0)
                {
                    actions.AddRange(command.Actions);
                }
                else if (initialActions != null)
                {
                    actions.AddRange(initialActions);
                }

                // Remove all deprecated actions
                actions.RemoveAll(a => a is SongRequestAction || a is SpotifyAction);

                foreach (ActionBase action in actions)
                {
                    ActionContainerControl actionControl = new ActionContainerControl(this.window, this, action);
                    actionControl.Minimize();
                    this.actionControls.Add(actionControl);
                }
            }

            hasLoaded = true;
            await base.OnLoaded();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                ActionTypeEnum type = (ActionTypeEnum)this.TypeComboBox.SelectedItem;
                ActionContainerControl actionControl = new ActionContainerControl(this.window, this, type);
                this.AddActionControlContainer(actionControl);
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

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                CommandBase command = await this.GetNewCommand();
                if (command != null)
                {
                    string fileName = ChannelSession.Services.FileService.ShowSaveFileDialog(command.Name + ".mixitupc");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        await FileSerializerHelper.SerializeToFile(fileName, command);
                    }
                }
            });
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
                        ActionListContainer actionContainer = await FileSerializerHelper.DeserializeFromFile<ActionListContainer>(fileName);
                        if (actionContainer != null && actionContainer.Actions != null)
                        {
                            foreach (ActionBase action in actionContainer.Actions)
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

        private async Task<List<ActionBase>> GetActions()
        {
            List<ActionBase> actions = new List<ActionBase>();
            foreach (ActionContainerControl control in this.actionControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    await DialogHelper.ShowMessage("Required action information is missing");
                    return new List<ActionBase>();
                }
                actions.Add(action);
            }
            return actions;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.PlayButton.Visibility = Visibility.Collapsed;
            this.StopButton.Visibility = Visibility.Visible;

            this.newCommand = await this.GetNewCommand();
            if (this.newCommand != null)
            {
                await CommandButtonsControl.TestCommand(this.newCommand);
            }
            this.SwitchToPlay();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.newCommand != null)
            {
                this.newCommand.StopCurrentRun();
            }
            this.SwitchToPlay();
        }

        public void SwitchToPlay()
        {
            this.PlayButton.Visibility = Visibility.Visible;
            this.StopButton.Visibility = Visibility.Collapsed;
        }

        private void AddActionControlContainer(ActionContainerControl actionControl)
        {
            foreach (ActionContainerControl control in this.actionControls)
            {
                control.Minimize();
            }
            this.actionControls.Add(actionControl);
        }

        private async Task<CommandBase> GetNewCommand()
        {
            if (!await this.commandDetailsControl.Validate())
            {
                return null;
            }

            if (this.actionControls.Count == 0)
            {
                await DialogHelper.ShowMessage("At least one action must be created");
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
                this.newCommand.StoreID = this.storeListingID;
            }
            return this.newCommand;
        }
    }
}
