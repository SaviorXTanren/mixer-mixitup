using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Window.Currency;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for StreamPassWindow.xaml
    /// </summary>
    public partial class StreamPassWindow : LoadingWindowBase
    {
        private StreamPassWindowViewModel viewModel;

        public StreamPassWindow()
        {
            this.viewModel = new StreamPassWindowViewModel();

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public StreamPassWindow(StreamPassModel seasonPass)
        {
            this.viewModel = new StreamPassWindowViewModel(seasonPass);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
            await base.OnLoaded();
        }

        private async void StartDateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CalendarDialogControl calendarControl = new CalendarDialogControl(this.viewModel.StartDate);
                if (bool.Equals(await DialogHelper.ShowCustom(calendarControl), true))
                {
                    this.viewModel.StartDate = calendarControl.SelectedDate.Date;
                }
            });
        }

        private async void EndDateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CalendarDialogControl calendarControl = new CalendarDialogControl(this.viewModel.EndDate);
                if (bool.Equals(await DialogHelper.ShowCustom(calendarControl), true))
                {
                    this.viewModel.EndDate = calendarControl.SelectedDate.Date;
                }
            });
        }

        private async void AddCustomLevelUpButtom_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (await this.viewModel.ValidateAddingCustomLevelUpCommand())
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(string.Format("{0} - {1}", MixItUp.Base.Resources.LevelUp, this.viewModel.CustomLevelUpNumber))));
                window.CommandSaveSuccessfully += CustomLevelUpWindow_CommandSaveSuccessfully;
                window.Show();
            }
        }

        private void LevelCommandButtons_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StreamPassCustomLevelUpCommandViewModel command = (StreamPassCustomLevelUpCommandViewModel)button.DataContext;
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command.Command));
            window.Show();
        }

        private async void DeleteLevelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StreamPassCustomLevelUpCommandViewModel command = (StreamPassCustomLevelUpCommandViewModel)button.DataContext;
            await this.viewModel.DeleteCustomLevelUpCommand(command);
        }

        private void DefaultLevelUpNewCommandButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(MixItUp.Base.Resources.LevelUp)));
            window.CommandSaveSuccessfully += DefaultLevelUpWindow_CommandSaveSuccessfully;
            window.Show();
        }

        private void DefaultLevelUpCommandButtons_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(this.viewModel.DefaultLevelUpCommand));
            window.Show();
        }

        private void DefaultLevelUpCommandButtons_DeleteClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.viewModel.DefaultLevelUpCommand = null;
        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await this.viewModel.Validate())
                {
                    await this.viewModel.Save();
                    this.Close();
                }
            });
        }

        private void CustomLevelUpWindow_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.AddCustomLevelUpCommand((CustomCommand)e);
            this.viewModel.CustomLevelUpNumber = 0;
        }

        private void DefaultLevelUpWindow_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.DefaultLevelUpCommand = (CustomCommand)e;
        }
    }
}
