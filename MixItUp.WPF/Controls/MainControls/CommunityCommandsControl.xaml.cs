using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.CommunityCommands;
using MixItUp.Base.ViewModel.MainControls;
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

        private void CommandsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is CommunityCommandViewModel)
            {
                this.viewModel.DetailsCommand.Execute(((CommunityCommandViewModel)e.AddedItems[0]).ID);
                ((ListView)sender).SelectedItem = null;
            }
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
    }
}
