using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Remote;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for RemoteControl.xaml
    /// </summary>
    public partial class RemoteControl : MainControlBase
    {
        public RemoteBoardModel CurrentBoard { get; private set; }
        public RemoteBoardGroupModel CurrentGroup { get; private set; }
        public RemoteCommand CurrentlySelectedCommand { get;set;}

        private ObservableCollection<RemoteCommand> remoteCommands = new ObservableCollection<RemoteCommand>();
        private ObservableCollection<RemoteBoardModel> boards = new ObservableCollection<RemoteBoardModel>();
        private ObservableCollection<RemoteBoardGroupModel> groups = new ObservableCollection<RemoteBoardGroupModel>();

        public RemoteControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.RemoteCommandsListView.ItemsSource = this.remoteCommands;
            this.BoardNameComboBox.ItemsSource = this.boards;
            this.GroupNameComboBox.ItemsSource = this.groups;

            this.Button00.Initialize(this, 0, 0);
            this.Button10.Initialize(this, 1, 0);
            this.Button20.Initialize(this, 2, 0);
            this.Button30.Initialize(this, 3, 0);
            this.Button01.Initialize(this, 0, 1);
            this.Button11.Initialize(this, 1, 1);
            this.Button21.Initialize(this, 2, 1);
            this.Button31.Initialize(this, 3, 1);

            this.RefreshCommandsList();

            this.RefreshBoardsView();

            return base.InitializeInternal();
        }

        private void RefreshCommandsList()
        {
            this.RemoteCommandsListView.SelectedIndex = -1;

            this.remoteCommands.Clear();
            foreach (RemoteCommand command in ChannelSession.Settings.RemoteCommands.OrderBy(c => c.Name))
            {
                this.remoteCommands.Add(command);
            }
        }

        public void ResetListSelectedIndex()
        {
            this.RemoteCommandsListView.SelectedIndex = -1;
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            RemoteCommand command = commandButtonsControl.GetCommandFromCommandButtons<RemoteCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new RemoteCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                RemoteCommand command = commandButtonsControl.GetCommandFromCommandButtons<RemoteCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.RemoteCommands.Remove(command);
                    await ChannelSession.SaveSettings();

                    this.RefreshCommandsList();
                    this.UpdateRemoteItemsAndBoard();
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new RemoteCommandDetailsControl());
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void AddReferenceCommandButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                RemoteReferenceCommandDialogControl dialogControl = new RemoteReferenceCommandDialogControl();
                string result = await MessageBoxHelper.ShowCustomDialog(dialogControl);
                if (!string.IsNullOrEmpty(result) && result.Equals("Save") && dialogControl.ReferenceCommand != null)
                {
                    ChannelSession.Settings.RemoteCommands.Add(new RemoteCommand(dialogControl.ReferenceCommand.Name, dialogControl.ReferenceCommand));
                    await ChannelSession.SaveSettings();

                    this.RefreshCommandsList();
                    this.UpdateRemoteItemsAndBoard();
                }
            });
        }

        private void UpdateRemoteItemsAndBoard()
        {
            foreach (RemoteBoardModel board in ChannelSession.Settings.RemoteBoards)
            {
                foreach (RemoteBoardGroupModel group in board.Groups)
                {
                    foreach (RemoteBoardItemModelBase item in group.Items.Where(i => i.Command == null).ToList())
                    {
                        group.Items.Remove(item);
                    }

                    foreach (RemoteBoardItemModelBase item in group.Items)
                    {
                        item.SetValuesFromCommand();
                    }
                }
            }
            this.RefreshBoardItemsView();
        }

        private void RemoteCommandsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.RemoteCommandsListView.SelectedItem != null)
            {
                RemoteCommand command = (RemoteCommand)this.RemoteCommandsListView.SelectedItem;
                this.CurrentlySelectedCommand = command;
            }
        }

        private void DataGridRow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            CommandButtonsControl commandButtonsControl = VisualTreeHelpers.GetVisualChild<CommandButtonsControl>(row);
            if (commandButtonsControl != null)
            {
                RemoteCommand command = commandButtonsControl.GetCommandFromCommandButtons<RemoteCommand>();
                if (command != null)
                {
                    this.CurrentlySelectedCommand = command;
                }
            }
        }

        private void BoardNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.BoardNameComboBox.SelectedIndex >= 0)
            {
                this.CurrentBoard = (RemoteBoardModel)this.BoardNameComboBox.SelectedItem;
                this.RefreshBoardGroupsView();
            }
        }

        private async void BoardAddButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string name = await MessageBoxHelper.ShowTextEntryDialog("Board Name");
                if (!string.IsNullOrEmpty(name))
                {
                    if (ChannelSession.Settings.RemoteBoards.Any(b => b.Name.Equals(name)))
                    {
                        await MessageBoxHelper.ShowMessageDialog("There already exists a board with this name");
                        return;
                    }

                    ChannelSession.Settings.RemoteBoards.Add(new RemoteBoardModel(name));
                    await ChannelSession.SaveSettings();

                    this.RefreshBoardsView();
                }
            });
        }

        private async void BoardEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.BoardNameComboBox.SelectedIndex >= 0)
            {
                RemoteBoardModel board = (RemoteBoardModel)this.BoardNameComboBox.SelectedItem;
                await this.Window.RunAsyncOperation(async () =>
                {
                    string name = await MessageBoxHelper.ShowTextEntryDialog("Board Name", board.Name);
                    if (!string.IsNullOrEmpty(name))
                    {
                        board.Name = name;
                        await ChannelSession.SaveSettings();

                        this.RefreshBoardsView();
                    }
                });
            }
        }

        private async void BoardDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.BoardNameComboBox.SelectedIndex >= 0)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you wish to delete this board?"))
                    {
                        RemoteBoardModel board = (RemoteBoardModel)this.BoardNameComboBox.SelectedItem;
                        ChannelSession.Settings.RemoteBoards.Remove(board);
                        await ChannelSession.SaveSettings();

                        this.RefreshBoardsView();
                    }
                });
            }
        }

        private void RefreshBoardsView()
        {
            this.CurrentBoard = null;
            this.boards.Clear();
            foreach (RemoteBoardModel board in ChannelSession.Settings.RemoteBoards)
            {
                this.boards.Add(board);
            }

            if (this.boards.Count > 0)
            {
                this.BoardNameComboBox.SelectedIndex = 0;
            }
            else
            {
                this.RefreshBoardGroupsView();
            }
        }

        private void GroupNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.GroupNameComboBox.SelectedIndex >= 0)
            {
                this.CurrentGroup = (RemoteBoardGroupModel)this.GroupNameComboBox.SelectedItem;
                this.RefreshBoardItemsView();
            }
        }

        private async void GroupAddButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string name = await MessageBoxHelper.ShowTextEntryDialog("Group Name");
                if (!string.IsNullOrEmpty(name))
                {
                    if (this.CurrentBoard.Groups.Any(b => b.Name.Equals(name)))
                    {
                        await MessageBoxHelper.ShowMessageDialog("There already exists a group for this board with this name");
                        return;
                    }

                    this.CurrentBoard.Groups.Add(new RemoteBoardGroupModel(name));
                    await ChannelSession.SaveSettings();

                    this.RefreshBoardsView();
                }
            });
        }

        private async void GroupEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.GroupNameComboBox.SelectedIndex >= 0)
            {
                RemoteBoardGroupModel group = (RemoteBoardGroupModel)this.GroupNameComboBox.SelectedItem;
                await this.Window.RunAsyncOperation(async () =>
                {
                    string name = await MessageBoxHelper.ShowTextEntryDialog("Group Name", group.Name);
                    if (!string.IsNullOrEmpty(name))
                    {
                        group.Name = name;
                        await ChannelSession.SaveSettings();

                        this.RefreshBoardsView();
                    }
                });
            }
        }

        private async void GroupDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.GroupNameComboBox.SelectedIndex >= 0)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you wish to delete this group?"))
                    {
                        RemoteBoardGroupModel group = (RemoteBoardGroupModel)this.BoardNameComboBox.SelectedItem;
                        this.CurrentBoard.Groups.Remove(group);
                        await ChannelSession.SaveSettings();

                        this.RefreshBoardsView();
                    }
                });
            }
        }

        private void RefreshBoardGroupsView()
        {
            this.CurrentGroup = null;
            this.groups.Clear();
            if (this.CurrentBoard != null)
            {
                this.GroupNameComboBox.IsEnabled = true;
                foreach (RemoteBoardGroupModel group in this.CurrentBoard.Groups)
                {
                    this.groups.Add(group);
                }
            }
            else
            {
                this.GroupNameComboBox.IsEnabled = false;
            }

            if (this.groups.Count > 0)
            {
                this.GroupNameComboBox.SelectedIndex = 0;
            }
            else
            {
                this.RefreshBoardItemsView();
            }
        }

        private void RefreshBoardItemsView()
        {
            this.Button00.SetRemoteItem(null);
            this.Button10.SetRemoteItem(null);
            this.Button20.SetRemoteItem(null);
            this.Button30.SetRemoteItem(null);
            this.Button01.SetRemoteItem(null);
            this.Button11.SetRemoteItem(null);
            this.Button21.SetRemoteItem(null);
            this.Button31.SetRemoteItem(null);

            if (this.CurrentGroup != null)
            {
                foreach (RemoteBoardItemModelBase item in this.CurrentGroup.Items)
                {
                    if (item.YPosition == 0)
                    {
                        if (item.XPosition == 0)
                        {
                            this.Button00.SetRemoteItem(item);
                        }
                        else if (item.XPosition == 1)
                        {
                            this.Button10.SetRemoteItem(item);
                        }
                        else if (item.XPosition == 2)
                        {
                            this.Button20.SetRemoteItem(item);
                        }
                        else if (item.XPosition == 3)
                        {
                            this.Button30.SetRemoteItem(item);
                        }
                    }
                    else if (item.YPosition == 1)
                    {
                        if (item.XPosition == 0)
                        {
                            this.Button01.SetRemoteItem(item);
                        }
                        else if (item.XPosition == 1)
                        {
                            this.Button11.SetRemoteItem(item);
                        }
                        else if (item.XPosition == 2)
                        {
                            this.Button21.SetRemoteItem(item);
                        }
                        else if (item.XPosition == 3)
                        {
                            this.Button31.SetRemoteItem(item);
                        }
                    }
                }
            }
        }

        private async void ConnectRemoteButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.SaveSettings();


            });
        }




        private void GetCodeButton_Click(object sender, RoutedEventArgs e)
        {
            //string code = "1234";   // Add code to query web service for connection code

            // Normal Connection logic

            //this.RemoteCodeTextBlock.Text = "Code: " + code;
            //this.RemoteCodeTextBlock.Visibility = Visibility.Visible;
            //this.RemoteCodeTimeLeftTextBlock.Visibility = Visibility.Visible;
            //this.GetCodeButton.Visibility = Visibility.Collapsed;

            //for (int i = 30; i > 0; i--)
            //{
            //    this.RemoteCodeTimeLeftTextBlock.Text = i + " Seconds Left...";
            //    await Task.Delay(1000);
            //}

            //this.RemoteCodeTextBlock.Visibility = Visibility.Collapsed;
            //this.RemoteCodeTimeLeftTextBlock.Visibility = Visibility.Collapsed;
            //this.GetCodeButton.Visibility = Visibility.Visible;

            //this.LogInGrid.Visibility = Visibility.Collapsed;
            //this.RemoteSetupGrid.Visibility = Visibility.Visible;
        }

        private void DisconnectRemoteButton_Click(object sender, RoutedEventArgs e)
        {
            //this.RemoteCodeTextBlock.Visibility = Visibility.Collapsed;
            //this.RemoteCodeTimeLeftTextBlock.Visibility = Visibility.Collapsed;
            //this.GetCodeButton.Visibility = Visibility.Visible;

            //this.LogInGrid.Visibility = Visibility.Visible;
            //this.RemoteSetupGrid.Visibility = Visibility.Collapsed;
        }



        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshCommandsList();
            this.UpdateRemoteItemsAndBoard();
        }
    }
}
