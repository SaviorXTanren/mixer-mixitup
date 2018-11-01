using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for OverlayWidgetsControl.xaml
    /// </summary>
    public partial class OverlayWidgetsControl : MainControlBase
    {
        //private ObservableCollection<TimerCommand> timerCommands = new ObservableCollection<TimerCommand>();

        public OverlayWidgetsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            //this.OverlayWidgetsListView.ItemsSource = this.timerCommands;

            this.RefreshList();

            return base.InitializeInternal();
        }

        private void RefreshList()
        {
        //    this.TimerCommandsListView.SelectedIndex = -1;

        //    this.timerCommands.Clear();
        //    foreach (TimerCommand command in ChannelSession.Settings.TimerCommands.OrderBy(c => c.Name))
        //    {
        //        this.timerCommands.Add(command);
        //    }
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            //CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            //TimerCommand command = commandButtonsControl.GetCommandFromCommandButtons<TimerCommand>(sender);
            //if (command != null)
            //{
            //    CommandWindow window = new CommandWindow(new TimerCommandDetailsControl(command));
            //    window.Closed += Window_Closed;
            //    window.Show();
            //}
        }

        private void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            //await this.Window.RunAsyncOperation(async () =>
            //{
            //    CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            //    TimerCommand command = commandButtonsControl.GetCommandFromCommandButtons<TimerCommand>(sender);
            //    if (command != null)
            //    {
            //        ChannelSession.Settings.TimerCommands.Remove(command);
            //        await ChannelSession.SaveSettings();
            //        this.RefreshList();
            //    }
            //});
        }

        private void AddOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            //CommandWindow window = new CommandWindow(new TimerCommandDetailsControl());
            //window.Closed += Window_Closed;
            //window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }
    }
}
