using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Currency;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows.Commands;
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
            await this.viewModel.OnOpen();
            await base.OnLoaded();
        }

        private void DeleteRankButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            RankModel rank = (RankModel)button.DataContext;
            this.viewModel.Ranks.Remove(rank);
        }

        private void RankUpNewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, MixItUp.Base.Resources.UserRankChanged);
            window.CommandSaved += RankUpWindow_CommandSaved;
            window.ForceShow();
        }

        private void RankUpCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CustomCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<CustomCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(command);
                window.CommandSaved += RankUpWindow_CommandSaved;
                window.ForceShow();
            }
        }

        private async void RankUpCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CustomCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<CustomCommandModel>();
                if (command != null)
                {
                    this.viewModel.RankChangedCommand = null;
                    ChannelSession.Settings.RemoveCommand(command);
                    await ChannelSession.SaveSettings();
                }
            });
        }

        private void RankDownNewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, MixItUp.Base.Resources.UserRankDown);
            window.CommandSaved += RankDownWindow_CommandSaved;
            window.ForceShow();
        }

        private void RankDownCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CustomCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<CustomCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(command);
                window.CommandSaved += RankDownWindow_CommandSaved;
                window.ForceShow();
            }
        }

        private async void RankDownCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CustomCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<CustomCommandModel>();
                if (command != null)
                {
                    this.viewModel.RankDownCommand = null;
                    ChannelSession.Settings.RemoveCommand(command);
                    await ChannelSession.SaveSettings();
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

        private void RankUpWindow_CommandSaved(object sender, CommandModelBase command)
        {
            this.viewModel.RankChangedCommand = (CustomCommandModel)command;
        }

        private void RankDownWindow_CommandSaved(object sender, CommandModelBase command)
        {
            this.viewModel.RankDownCommand = (CustomCommandModel)command;
        }
    }
}
