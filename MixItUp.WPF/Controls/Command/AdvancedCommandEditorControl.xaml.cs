using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using MixItUp.WPF.Windows.Store;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    public class ActionTypeEntry : IEquatable<ActionTypeEntry>, IComparable<ActionTypeEntry>
    {
        public string Name { get; set; }
        public ActionTypeEnum Type { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ActionTypeEntry)
            {
                return this.Equals((ActionTypeEntry)obj);
            }
            return false;
        }

        public bool Equals(ActionTypeEntry other) { return this.Type.Equals(other.Type); }

        public int CompareTo(ActionTypeEntry other) { return this.Type.CompareTo(other.Type); }

        public override int GetHashCode() { return this.Type.GetHashCode(); }
    }

    /// <summary>
    /// Interaction logic for AdvancedCommandEditorControl.xaml
    /// </summary>
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
                List<ActionTypeEntry> actionTypes = new List<ActionTypeEntry>();
                foreach (ActionTypeEnum actionType in EnumHelper.GetEnumList<ActionTypeEnum>())
                {
                    ActionTypeEntry entry = new ActionTypeEntry() { Name = EnumHelper.GetEnumName(actionType), Type = actionType };
                    switch (entry.Type)
                    {
                        case ActionTypeEnum.Custom:
                            continue;
                        case ActionTypeEnum.Chat:
                            entry.Name += " Message";
                            break;
                        case ActionTypeEnum.Counter:
                            entry.Name += " (Create & Update)";
                            break;
                        case ActionTypeEnum.Input:
                            entry.Name += " (Keyboard & Mouse)";
                            break;
                        case ActionTypeEnum.Overlay:
                            entry.Name += " (Images & Videos)";
                            break;
                        case ActionTypeEnum.File:
                            entry.Name += " (Read & Write)";
                            break;
                        default:
                            break;
                    }
                    actionTypes.Add(entry);
                }

                this.TypeComboBox.ItemsSource = actionTypes.OrderBy(at => at.Name);
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
                ActionTypeEntry type = (ActionTypeEntry)this.TypeComboBox.SelectedItem;
                ActionContainerControl actionControl = new ActionContainerControl(this.window, this, type.Type);
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
                        await SerializerHelper.SerializeToFile(fileName, command);
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

        private async void UploadToMixItUpStoreButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                CommandBase command = await this.GetNewCommand();
                if (command != null)
                {
                    UploadStoreListingWindow window = new UploadStoreListingWindow(command);
                    window.Show();
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
                    await MessageBoxHelper.ShowMessageDialog("Required action information is missing");
                    return new List<ActionBase>();
                }
                actions.Add(action);
            }
            return actions;
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
                this.newCommand.StoreID = this.storeListingID;
            }
            return this.newCommand;
        }
    }
}
