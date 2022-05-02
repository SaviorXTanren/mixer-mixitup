using MixItUp.Base.ViewModel.MainControls;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Windows.Dashboard;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System.Collections.Generic;
using System;
using MixItUp.Base.Util;
using System.Linq;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : MainControlBase
    {
        private ChatMainControlViewModel viewModel;

        private List<object> defaultContextMenuItems = new List<object>();

        public ChatControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            await this.ChatList.Initialize(this.Window);

            this.viewModel = new ChatMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);

            this.viewModel.ContextMenuCommandsChanged += ViewModel_ContextMenuCommandsChanged;

            await this.viewModel.OnOpen();
            this.DataContext = this.viewModel;

            foreach (object item in this.UserList.ContextMenu.Items)
            {
                this.defaultContextMenuItems.Add(item);
            }
            this.ViewModel_ContextMenuCommandsChanged(this, new EventArgs());
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
            await base.OnVisibilityChanged();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow window = new DashboardWindow();
            window.Show();
        }

        private async void ViewModel_ContextMenuCommandsChanged(object sender, EventArgs e)
        {
            await DispatcherHelper.Dispatcher.InvokeAsync(() =>
            {
                this.UserList.ContextMenu.Items.Clear();
                foreach (var item in this.defaultContextMenuItems)
                {
                    this.UserList.ContextMenu.Items.Add(item);
                }

                if (viewModel.ContextMenuChatCommands.Count() > 0)
                {
                    this.UserList.ContextMenu.Items.Add(new Separator());
                    foreach (CommandModelBase command in viewModel.ContextMenuChatCommands)
                    {
                        MenuItem menuItem = new MenuItem();
                        menuItem.Header = command.Name;
                        menuItem.DataContext = command;
                        menuItem.Click += this.ContextMenuChatCommand_Click;
                        this.UserList.ContextMenu.Items.Add(menuItem);
                    }
                }

                return Task.CompletedTask;
            });
        }

        private async void UserInformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.UserList.SelectedItem != null && this.UserList.SelectedItem is UserV2ViewModel)
            {
                UserV2ViewModel user = (UserV2ViewModel)this.UserList.SelectedItem;
                if (user != null)
                {
                    await ChatUserDialogControl.ShowUserDialog(user);
                }
            }
        }

        private async void ContextMenuChatCommand_Click(object sender, RoutedEventArgs e)
        {
            if (this.UserList.SelectedItem != null && this.UserList.SelectedItem is UserV2ViewModel)
            {
                UserV2ViewModel user = (UserV2ViewModel)this.UserList.SelectedItem;
                if (user != null)
                {
                    if (e.Source is MenuItem)
                    {
                        MenuItem menuItem = (MenuItem)e.Source;
                        if (menuItem.DataContext != null && menuItem.DataContext is CommandModelBase)
                        {
                            CommandModelBase command = (CommandModelBase)menuItem.DataContext;
                            await ServiceManager.Get<CommandService>().Queue(command, new CommandParametersModel(platform: user.Platform, arguments: new List<string>() { user.Username }) { TargetUser = user });
                        }
                    }
                }
            }
        }
    }
}
