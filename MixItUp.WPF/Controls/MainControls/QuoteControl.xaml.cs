using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    public class QuoteListing
    {
        public int Index { get; set; }
        public UserQuoteViewModel Quote { get; set; }
    }

    /// <summary>
    /// Interaction logic for QuoteControl.xaml
    /// </summary>
    public partial class QuoteControl : MainControlBase
    {
        private ObservableCollection<QuoteListing> quotes = new ObservableCollection<QuoteListing>();

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
                    UserQuoteViewModel newQuote = new UserQuoteViewModel(this.AddQuoteTextBox.Text, DateTimeOffset.Now, ChannelSession.Channel.type);
                    ChannelSession.Settings.UserQuotes.Add(newQuote);
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
                QuoteListing quote = (QuoteListing)button.DataContext;
                await this.Window.RunAsyncOperation(async () =>
                {
                    ChannelSession.Settings.UserQuotes.Remove(quote.Quote);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                });
            }
        }

        private void RefreshList()
        {
            this.quotes.Clear();
            for (int i = 0; i < ChannelSession.Settings.UserQuotes.Count; i++)
            {
                this.quotes.Add(new QuoteListing() { Index = (i + 1), Quote = ChannelSession.Settings.UserQuotes[i] });
            }
        }

        private void GlobalEvents_OnQuoteAdded(object sender, UserQuoteViewModel quote)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.RefreshList();
            }));
        }

        private void QuoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            QuoteListing quote = (QuoteListing)textBox.DataContext;
            quote.Quote.Quote = textBox.Text;
        }

        private void QuoteGameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            QuoteListing quote = (QuoteListing)textBox.DataContext;
            quote.Quote.GameName = textBox.Text;
        }
    }
}
