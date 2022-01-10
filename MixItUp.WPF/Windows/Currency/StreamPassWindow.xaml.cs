using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Currency;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows.Commands;
using System.Threading.Tasks;

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
            await this.viewModel.OnOpen();
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
                CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, string.Format("{0} - {1}", MixItUp.Base.Resources.LevelUp, this.viewModel.CustomLevelUpNumber));
                window.CommandSaved += CustomLevelUpWindow_CommandSaved;
                window.ForceShow();
            }
        }

        private void LevelCommandButtons_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(((CommandListingButtonsControl)sender).GetCommandFromCommandButtons());
            window.ForceShow();
        }

        private void LevelCommandButtons_DeleteClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandListingButtonsControl button = (CommandListingButtonsControl)sender;
            StreamPassCustomLevelUpCommandViewModel command = (StreamPassCustomLevelUpCommandViewModel)button.DataContext;
            this.viewModel.DeleteCustomLevelUpCommand(command);
        }

        private void DefaultLevelUpCommandButtons_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(this.viewModel.DefaultLevelUpCommand);
            window.CommandSaved += Window_CommandSaved;
            window.ForceShow();
        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                bool isNew = this.viewModel.IsNew;
                if (await this.viewModel.Validate())
                {
                    await this.viewModel.Save();

                    if (isNew)
                    {
                        NewAutoChatCommandsDialogControl customDialogControl = new NewAutoChatCommandsDialogControl(this.viewModel.GetNewAutoChatCommands());
                        if (bool.Equals(await DialogHelper.ShowCustom(customDialogControl), true))
                        {
                            customDialogControl.AddSelectedCommands();
                        }
                    }

                    this.Close();
                }
            });
        }

        private void CustomLevelUpWindow_CommandSaved(object sender, CommandModelBase command)
        {
            this.viewModel.AddCustomLevelUpCommand(command);
            this.viewModel.CustomLevelUpNumber = 0;
        }

        private void Window_CommandSaved(object sender, CommandModelBase command)
        {
            this.viewModel.DefaultLevelUpCommand = command;
        }
    }
}
