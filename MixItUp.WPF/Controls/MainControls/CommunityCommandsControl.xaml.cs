using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.CommunityCommands;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Controls.Dialogs.CommunityCommands;
using MixItUp.WPF.Windows.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CommunityCommandsControl.xaml
    /// </summary>
    public partial class CommunityCommandsControl : MainControlBase
    {
        private CommunityCommandsMainControlViewModel viewModel;

        public CommunityCommandsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new CommunityCommandsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private void CommandsList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }

        private void CommandsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is CommunityCommandViewModel)
            {
                this.viewModel.GetCommandDetailsCommand.Execute(((CommunityCommandViewModel)e.AddedItems[0]).ID);
                ((ListView)sender).SelectedItem = null;
            }
        }

        private void UserCommandsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is CommunityCommandViewModel)
            {
                this.viewModel.GetCommandDetailsCommand.Execute(((CommunityCommandViewModel)e.AddedItems[0]).ID);
                ((ListView)sender).SelectedItem = null;
            }
        }

        private void MyCommandsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is CommunityCommandViewModel)
            {
                this.viewModel.EditMyCommandCommand.Execute(((CommunityCommandViewModel)e.AddedItems[0]).ID);
                ((ListView)sender).SelectedItem = null;
            }
        }

        private async void DownloadCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (bool.Equals(await DialogHelper.ShowCustom(new CommandImporterDialogControl(this.viewModel.CommandDetails.PrimaryCommand)), true))
            {
                if (!this.viewModel.DownloadedCommandsCache.Contains(this.viewModel.CommandDetails.ID))
                {
                    this.viewModel.DownloadedCommandsCache.Add(this.viewModel.CommandDetails.ID);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await ChannelSession.Services.CommunityCommandsService.DownloadCommand(this.viewModel.CommandDetails.ID);
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        private async void ReviewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.CommandDetails.Username.Equals(ChannelSession.GetCurrentUser().Username, StringComparison.CurrentCultureIgnoreCase))
            {
                CommunityCommandsReviewCommandDialogControl dialogControl = new CommunityCommandsReviewCommandDialogControl();
                if (bool.Equals(await DialogHelper.ShowCustom(dialogControl), true) && !string.IsNullOrEmpty(dialogControl.Review))
                {
                    if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.CommunityCommandsReviewAgreement))
                    {
                        await this.viewModel.ReviewCommand(dialogControl.Rating, dialogControl.Review);
                    }
                }
            }
        }

        private async void ReportCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommunityCommandsReportCommandDialogControl dialogControl = new CommunityCommandsReportCommandDialogControl();
            if (bool.Equals(await DialogHelper.ShowCustom(dialogControl), true) && !string.IsNullOrEmpty(dialogControl.Report))
            {
                await this.viewModel.ReportCommand(dialogControl.Report);
            }
        }

        private void EditCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommunityCommandUploadWindow window = new CommunityCommandUploadWindow(this.viewModel.CommandDetails);
            window.Closed += (object s, System.EventArgs x) => { this.viewModel.EditMyCommandCommand.Execute(this.viewModel.CommandDetails.ID); };
            window.Show();
        }
    }
}
