using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for WebhooksControl.xaml
    /// </summary>
    public partial class WebhooksControl : MainControlBase
    {
        private WebhooksMainControlViewModel viewModel;

        public WebhooksControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new WebhooksMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnOpen();

            await base.InitializeInternal();
        }

        private async void CopyURLCommandButton_Click(object sender, RoutedEventArgs e)
        {
            WebhookCommandItemViewModel commandItem = ((Button)sender).DataContext as WebhookCommandItemViewModel;
            await UIHelpers.CopyToClipboard($"{MixItUpService.MixItUpAPIEndpoint}webhook/{commandItem.Webhook.Id}?secret={commandItem.Webhook.Secret}");
        }

        private void NewWebhookCommandButton_Click(object sender, RoutedEventArgs e)
        {
            WebhookCommandItemViewModel commandItem = ((Button)sender).DataContext as WebhookCommandItemViewModel;
            CommandEditorWindow window = new CommandEditorWindow(commandItem.Webhook);
            window.Closed += Window_Closed;
            window.ForceShow();
        }

        private async void DeleteWebhookCommandButton_Click(object sender, RoutedEventArgs e)
        {
            WebhookCommandItemViewModel commandItem = ((Button)sender).DataContext as WebhookCommandItemViewModel;
            await ServiceManager.Get<MixItUpService>().DeleteWebhook(commandItem.Webhook.Id);
            await this.viewModel.RefreshCommands();
            await ChannelSession.SaveSettings();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            WebhookCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<WebhookCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(command);
                window.Closed += Window_Closed;
                window.ForceShow();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                try
                {
                    WebhookCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<WebhookCommandModel>();
                    if (command != null)
                    {
                        ServiceManager.Get<CommandService>().WebhookCommands.Remove(command);
                        ChannelSession.Settings.RemoveCommand(command);
                        await ServiceManager.Get<MixItUpService>().DeleteWebhook(command.ID);
                        await this.viewModel.RefreshCommands();
                        await ChannelSession.SaveSettings();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }

        private async void AddWebhookButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                try
                {
                    await ServiceManager.Get<MixItUpService>().CreateWebhook();
                    await this.viewModel.RefreshCommands();
                    await ChannelSession.SaveSettings();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            await this.viewModel.RefreshCommands();
        }
    }
}