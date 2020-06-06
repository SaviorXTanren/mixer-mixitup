using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Window.Currency;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows.Command;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyWindow.xaml
    /// </summary>
    public partial class CurrencyWindow : LoadingWindowBase
    {
        private CurrencyWindowViewModel viewModel;

        public CurrencyWindow()
        {
            this.viewModel = new CurrencyWindowViewModel();

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public CurrencyWindow(CurrencyModel currency)
        {
            this.viewModel = new CurrencyWindowViewModel(currency);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
            await base.OnLoaded();
        }

        private void DeleteRankButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            RankModel rank = (RankModel)button.DataContext;
            this.viewModel.Ranks.Remove(rank);
        }

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(MixItUp.Base.Resources.UserRankChanged)));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
                if (command != null)
                {
                    this.viewModel.RankChangedCommand = null;
                    await ChannelSession.SaveSettings();
                    this.UpdateRankChangedCommand();
                }
            });
        }

        private async void DateButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                DateTimeOffset resetStart = (this.viewModel.IsNew || this.viewModel.Currency.ResetStartCadence == DateTimeOffset.MinValue) ? DateTimeOffset.Now : this.viewModel.Currency.ResetStartCadence;

                string identifier = "";
                if (this.viewModel.AutomaticResetRate == CurrencyResetRateEnum.Weekly) { identifier = "Week"; }
                else if (this.viewModel.AutomaticResetRate == CurrencyResetRateEnum.Monthly) { identifier = "Month"; }
                else if (this.viewModel.AutomaticResetRate == CurrencyResetRateEnum.Yearly) { identifier = "Year"; }

                CalendarDialogControl calendarControl = new CalendarDialogControl(resetStart, string.Format("Please select the Day of the {0} that this should be reset on:", identifier));
                if (bool.Equals(await DialogHelper.ShowCustom(calendarControl), true))
                {
                    this.viewModel.AutomaticResetStartTime = calendarControl.SelectedDate.Date;
                }
            });
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

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.RankChangedCommand = (CustomCommand)e;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.UpdateRankChangedCommand();
        }

        private void UpdateRankChangedCommand()
        {
            if (this.viewModel.RankChangedCommand != null)
            {
                this.NewCommandButton.Visibility = Visibility.Collapsed;
                this.CommandButtons.Visibility = Visibility.Visible;
                this.CommandButtons.DataContext = this.viewModel.RankChangedCommand;
            }
            else
            {
                this.NewCommandButton.Visibility = Visibility.Visible;
                this.CommandButtons.Visibility = Visibility.Collapsed;
            }
        }
    }
}
