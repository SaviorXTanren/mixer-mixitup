using MixItUp.Base;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
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
            this.ResetCurrencyComboBox.ItemsSource = new List<string>() { "Never", "Yearly", "Monthly", "Weekly", "Daily" };

            this.CurrencyNameTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.Name;
            this.CurrencyAmountTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.AcquireAmount.ToString();
            this.CurrencyTimeTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.AcquireInterval.ToString();
            this.CurrencyToggleSwitch.IsChecked = ChannelSession.Settings.CurrencyAcquisition.Enabled;
            this.ResetCurrencyComboBox.SelectedItem = ChannelSession.Settings.CurrencyAcquisition.ResetInterval;
            this.CurrencyGrid.IsEnabled = !ChannelSession.Settings.CurrencyAcquisition.Enabled;

            if (ChannelSession.Settings.CurrencyAcquisition.ShouldBeReset())
            {
                this.ResetCurrency();
            }

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

        private void ResetCurrencyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ResetCurrencyComboBox.SelectedIndex >= 0)
            {
                ChannelSession.Settings.CurrencyAcquisition.ResetInterval = (string)this.ResetCurrencyComboBox.SelectedItem;
            }
        }

        private async void ResetCurrencyManuallyButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("This will reset the currency for all users. Are you sure?"))
                {
                    this.ResetCurrency();
                }
            });
        }

        private void ResetCurrency()
        {
            foreach (var kvp in ChannelSession.Settings.UserData)
            {
                kvp.Value.ResetCurrency();
            }
            ChannelSession.Settings.CurrencyAcquisition.LastReset = new DateTimeOffset(DateTimeOffset.Now.Date);
        }
    }
}
