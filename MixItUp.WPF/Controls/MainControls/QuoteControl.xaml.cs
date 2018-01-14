using MixItUp.Base;
using MixItUp.Base.Util;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private ObservableCollection<string> quotes = new ObservableCollection<string>();

        public QuoteControl()
        {
            InitializeComponent();

            this.QuotesDataGrid.ItemsSource = quotes;

            GlobalEvents.OnQuoteAdded += GlobalEvents_OnQuoteAdded;
        }

        protected override Task InitializeInternal()
        {
            this.EnableQuotesToggleButton.IsChecked = ChannelSession.Settings.QuotesEnabled;

            this.RefreshList();

            return base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void EnableQuotesToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.QuotesEnabled = this.EnableQuotesToggleButton.IsChecked.GetValueOrDefault();
        }

        private async void AddQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.AddQuoteTextBox.Text))
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    ChannelSession.Settings.Quotes.Add(this.AddQuoteTextBox.Text);
                    this.AddQuoteTextBox.Clear();
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                });
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.DataContext != null)
            {
                string quote = (string)button.DataContext;
                await this.Window.RunAsyncOperation(async () =>
                {
                    ChannelSession.Settings.Quotes.Remove(quote);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                });
            }
        }

        private void RefreshList()
        {
            this.quotes.Clear();
            foreach (string quote in ChannelSession.Settings.Quotes.OrderBy(q => q))
            {
                this.quotes.Add(quote);
            }
        }

        private void GlobalEvents_OnQuoteAdded(object sender, string quote)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.quotes.Add(quote);
            }));
        }
    }
}
