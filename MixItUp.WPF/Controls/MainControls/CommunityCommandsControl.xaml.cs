using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.CommunityCommands;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Controls.Dialogs.CommunityCommands;
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
                this.viewModel.DetailsCommand.Execute(((CommunityCommandViewModel)e.AddedItems[0]).ID);
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
            await ChannelSession.Services.CommunityCommandsService.DownloadCommand(this.viewModel.CommandDetails.ID);

            await DialogHelper.ShowCustom(new CommandImporterDialogControl(this.viewModel.CommandDetails.PrimaryCommand));
        }

        private async void ReviewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommunityCommandsReviewCommandDialogControl dialogControl = new CommunityCommandsReviewCommandDialogControl();
            if (bool.Equals(await DialogHelper.ShowCustom(dialogControl), true) && !string.IsNullOrEmpty(dialogControl.Review))
            {
                await this.viewModel.ReviewCommand(dialogControl.Rating, dialogControl.Review);
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

        private async void EditCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommunityCommandsReviewCommandDialogControl dialogControl = new CommunityCommandsReviewCommandDialogControl();
            if (bool.Equals(await DialogHelper.ShowCustom(dialogControl), true) && !string.IsNullOrEmpty(dialogControl.Review))
            {
                await this.viewModel.ReviewCommand(dialogControl.Rating, dialogControl.Review);
            }
        }
    }
}
