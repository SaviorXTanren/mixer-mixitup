using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows.Quotes;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for QuoteControl.xaml
    /// </summary>
    public partial class QuoteControl : MainControlBase
    {
        private QuotesMainControlViewModel viewModel;

        public QuoteControl()
        {
            InitializeComponent();

            UserQuoteModel.OnQuoteAdded += GlobalEvents_OnQuoteAdded;
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new QuotesMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnOpen();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private void ImportQuotesButton_Click(object sender, RoutedEventArgs e)
        {
            QuotesDataImportWindow window = new QuotesDataImportWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.DataContext != null)
            {
                await this.viewModel.RemoveQuote((UserQuoteViewModel)button.DataContext);
            }
        }

        private async void DateButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                if (button.DataContext != null)
                {
                    UserQuoteViewModel quote = (UserQuoteViewModel)button.DataContext;
                    CalendarDialogControl calendarControl = new CalendarDialogControl(quote.DateTime);
                    if (bool.Equals(await DialogHelper.ShowCustom(calendarControl), true))
                    {
                        quote.DateTime = calendarControl.SelectedDate.Date + quote.DateTime.TimeOfDay;
                    }
                }
            });
        }

        private async void TimeButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                if (button.DataContext != null)
                {
                    UserQuoteViewModel quote = (UserQuoteViewModel)button.DataContext;
                    ClockDialogControl calendarControl = new ClockDialogControl(quote.DateTime);
                    if (bool.Equals(await DialogHelper.ShowCustom(calendarControl), true))
                    {
                        quote.DateTime = quote.DateTime.Date + calendarControl.SelectedTime.TimeOfDay;
                    }
                }
            });
        }

        private void GlobalEvents_OnQuoteAdded(object sender, UserQuoteModel quote)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.viewModel.Refresh();
            }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.viewModel.Refresh();
        }
    }
}
