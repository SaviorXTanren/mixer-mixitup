using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Dashboard;
using MixItUp.WPF.Controls.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for NotificationsDashboardControl.xaml
    /// </summary>
    public partial class AlertsDashboardControl : DashboardControlBase
    {
        private AlertsDashboardControlViewModel viewModel;

        private ScrollViewer scrollViewer;

        private List<object> defaultContextMenuItems = new List<object>();

        public AlertsDashboardControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new AlertsDashboardControlViewModel(this.Window.ViewModel);

            this.viewModel.ContextMenuCommandsChanged += ViewModel_ContextMenuCommandsChanged;

            await this.viewModel.OnOpen();

            await base.InitializeInternal();

            foreach (object item in this.AlertsListView.ContextMenu.Items)
            {
                this.defaultContextMenuItems.Add(item);
            }
            this.ViewModel_ContextMenuCommandsChanged(this, new EventArgs());
        }

        private void NotificationsListView_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (this.scrollViewer == null)
            {
                this.scrollViewer = (ScrollViewer)e.OriginalSource;
            }

            if (this.scrollViewer != null)
            {
                if (ChannelSession.Settings.LatestChatAtTop)
                {
                    this.scrollViewer.ScrollToTop();
                }
                else
                {
                    this.scrollViewer.ScrollToBottom();
                }
            }
        }

        private async void ViewModel_ContextMenuCommandsChanged(object sender, EventArgs e)
        {
            await DispatcherHelper.Dispatcher.InvokeAsync(() =>
            {
                this.AlertsListView.ContextMenu.Items.Clear();
                foreach (var item in this.defaultContextMenuItems)
                {
                    this.AlertsListView.ContextMenu.Items.Add(item);
                }

                if (viewModel.ContextMenuChatCommands.Count() > 0)
                {
                    this.AlertsListView.ContextMenu.Items.Add(new Separator());
                    foreach (CommandModelBase command in viewModel.ContextMenuChatCommands)
                    {
                        MenuItem menuItem = new MenuItem();
                        menuItem.Header = command.Name;
                        menuItem.DataContext = command;
                        menuItem.Click += this.ContextMenuChatCommand_Click;
                        this.AlertsListView.ContextMenu.Items.Add(menuItem);
                    }
                }

                return Task.CompletedTask;
            });
        }

        private async void UserInformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.AlertsListView.SelectedItem != null && this.AlertsListView.SelectedItem is ChatMessageViewModel)
            {
                ChatMessageViewModel message = (ChatMessageViewModel)this.AlertsListView.SelectedItem;
                if (message.User != null)
                {
                    await ChatUserDialogControl.ShowUserDialog(message.User);
                }
            }
        }

        private async void ContextMenuChatCommand_Click(object sender, RoutedEventArgs e)
        {
            if (this.AlertsListView.SelectedItem != null && this.AlertsListView.SelectedItem is ChatMessageViewModel)
            {
                ChatMessageViewModel message = (ChatMessageViewModel)this.AlertsListView.SelectedItem;
                if (message.User != null)
                {
                    if (e.Source is MenuItem)
                    {
                        MenuItem menuItem = (MenuItem)e.Source;
                        if (menuItem.DataContext != null && menuItem.DataContext is ChatCommandModel)
                        {
                            ChatCommandModel command = (ChatCommandModel)menuItem.DataContext;
                            await ServiceManager.Get<CommandService>().Queue(command, new CommandParametersModel(platform: message.Platform, arguments: new List<string>() { message.User.Username }) { TargetUser = message.User });
                        }
                    }
                }
            }
        }
    }
}
