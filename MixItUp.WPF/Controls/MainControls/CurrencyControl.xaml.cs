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

            this.CurrencyToggleSwitch.IsChecked = ChannelSession.Settings.CurrencyAcquisition.Enabled;
            this.CurrencyNameTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.Name;
            this.CurrencyAmountTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.AcquireAmount.ToString();
            this.CurrencyTimeTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.AcquireInterval.ToString();
            this.CurrencyFollowBonusTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.FollowBonus.ToString();
            this.CurrencyHostBonusTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.HostBonus.ToString();
            this.CurrencySubscribeBonusTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.SubscribeBonus.ToString();
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
            if (string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) || !int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount) || currencyAmount < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The currency rate must be 0 or greater");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            int currencyTime = 0;
            if (string.IsNullOrEmpty(this.CurrencyTimeTextBox.Text) || !int.TryParse(this.CurrencyTimeTextBox.Text, out currencyTime) || currencyTime < 1)
            {
                await MessageBoxHelper.ShowMessageDialog("The currency interval be greater than 1");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            int followBonus = 0;
            if (string.IsNullOrEmpty(this.CurrencyFollowBonusTextBox.Text) || !int.TryParse(this.CurrencyFollowBonusTextBox.Text, out followBonus) || followBonus < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Follow bonus must be 0 or greater");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            int hostBonus = 0;
            if (string.IsNullOrEmpty(this.CurrencyHostBonusTextBox.Text) || !int.TryParse(this.CurrencyHostBonusTextBox.Text, out hostBonus) || hostBonus < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Host bonus must be 0 or greater");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            int subscribeBonus = 0;
            if (string.IsNullOrEmpty(this.CurrencySubscribeBonusTextBox.Text) || !int.TryParse(this.CurrencyHostBonusTextBox.Text, out subscribeBonus) || subscribeBonus < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Subscribe bonus must be 0 or greater");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.CurrencyAcquisition.Name = this.CurrencyNameTextBox.Text;
                ChannelSession.Settings.CurrencyAcquisition.AcquireAmount = currencyAmount;
                ChannelSession.Settings.CurrencyAcquisition.AcquireInterval = currencyTime;
                ChannelSession.Settings.CurrencyAcquisition.FollowBonus = followBonus;
                ChannelSession.Settings.CurrencyAcquisition.HostBonus = hostBonus;
                ChannelSession.Settings.CurrencyAcquisition.SubscribeBonus = subscribeBonus;
                ChannelSession.Settings.CurrencyAcquisition.ResetInterval = (string)this.ResetCurrencyComboBox.SelectedItem;
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

        private async void ResetCurrencyManuallyButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("Do you want to reset all currency?"))
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
