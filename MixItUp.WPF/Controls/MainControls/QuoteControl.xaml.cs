using MixItUp.Base;
using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for QuoteControl.xaml
    /// </summary>
    public partial class QuoteControl : MainControlBase
    {
        public QuoteControl()
        {
            InitializeComponent();

            GlobalEvents.OnQuoteAdded += GlobalEvent_OnQuoteAdded;
        }

        protected override Task InitializeInternal()
        {
            this.EnableQuotesToggleButton.IsChecked = ChannelSession.Settings.QuotesEnabled;
            this.QuotesTextBox.Text = string.Join(Environment.NewLine, ChannelSession.Settings.Quotes);

            return base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private async void EnableQuotesToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.QuotesEnabled = this.EnableQuotesToggleButton.IsChecked.GetValueOrDefault();
                await ChannelSession.SaveSettings();
            });
        }

        private async void QuotesTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string quotes = this.QuotesTextBox.Text;
                if (string.IsNullOrEmpty(this.QuotesTextBox.Text))
                {
                    quotes = "";
                }

                ChannelSession.Settings.Quotes.Clear();
                foreach (string split in quotes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    ChannelSession.Settings.Quotes.Add(split);
                }

                await ChannelSession.SaveSettings();
            });
        }

        private void GlobalEvent_OnQuoteAdded(object sender, string e)
        {
            this.QuotesTextBox.Text = string.Join(Environment.NewLine, ChannelSession.Settings.Quotes);
        }
    }
}
