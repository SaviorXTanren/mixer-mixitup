using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CurrencyControl.xaml
    /// </summary>
    public partial class CurrencyControl : MainControlBase
    {
        public CurrencyControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.CurrencyNameTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.Name;
            this.CurrencyAmountTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.AcquireAmount.ToString();
            this.CurrencyTimeTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.AcquireInterval.ToString();
            this.CurrencyToggleSwitch.IsChecked = ChannelSession.Settings.CurrencyAcquisition.Enabled;
            this.CurrencyGrid.IsEnabled = !ChannelSession.Settings.CurrencyAcquisition.Enabled;

            return base.InitializeInternal();
        }

        private async void CurrencyToggleSwitch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.CurrencyNameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A currency name must be specified");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            int currencyAmount = 0;
            if (string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) || !int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount) || currencyAmount < 1)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid currency amount must be specified");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            int currencyTime = 0;
            if (string.IsNullOrEmpty(this.CurrencyTimeTextBox.Text) || !int.TryParse(this.CurrencyTimeTextBox.Text, out currencyTime) || currencyTime < 1)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid currency interval must be specified");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.CurrencyAcquisition.Name = this.CurrencyNameTextBox.Text;
                ChannelSession.Settings.CurrencyAcquisition.AcquireAmount = currencyAmount;
                ChannelSession.Settings.CurrencyAcquisition.AcquireInterval = currencyTime;
                ChannelSession.Settings.CurrencyAcquisition.Enabled = true;

                await ChannelSession.SaveSettings();
            });

            this.CurrencyGrid.IsEnabled = false;
        }

        private async void CurrencyToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.CurrencyAcquisition.Enabled = false;

                await ChannelSession.SaveSettings();
            });

            this.CurrencyGrid.IsEnabled = true;
        }
    }
}
