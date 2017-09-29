using MixItUp.Base;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Util;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyControl.xaml
    /// </summary>
    public partial class CurrencyControl : MainControlBase
    {
        private ObservableCollection<UserDataViewModel> userCurrencyData;

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public CurrencyControl()
        {
            InitializeComponent();

            this.userCurrencyData = new ObservableCollection<UserDataViewModel>();
        }

        protected override Task InitializeInternal()
        {
            this.UserCurrencyListView.ItemsSource = this.userCurrencyData;

            this.Window.Closing += Window_Closing;
            if (ChannelSession.Settings.CurrencyEnabled)
            {
                this.CurrencyNameTextBox.Text = ChannelSession.Settings.CurrencyName;
                this.CurrencyAmountTextBox.Text = ChannelSession.Settings.CurrencyAcquireAmount.ToString();
                this.CurrencyTimeTextBox.Text = ChannelSession.Settings.CurrencyAcquireInterval.ToString();
            }
            this.CurrencyToggleSwitch.IsChecked = ChannelSession.Settings.CurrencyEnabled;

            return base.InitializeInternal();
        }

        private void CurrencyAcquireBackground()
        {
            while (!this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(1000 * 60 * ChannelSession.Settings.CurrencyAcquireInterval);

                    foreach (ChatUserViewModel chatUser in ChannelSession.ChatUsers.Values)
                    {
                        if (!ChannelSession.Settings.UserData.ContainsKey(chatUser.ID))
                        {
                            ChannelSession.Settings.UserData[chatUser.ID] = new UserDataViewModel(chatUser.ID, chatUser.UserName);
                        }
                        ChannelSession.Settings.UserData[chatUser.ID].CurrencyAmount += ChannelSession.Settings.CurrencyAcquireAmount;
                    }

                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        this.userCurrencyData.Clear();
                        foreach (ChatUserViewModel chatUser in ChannelSession.ChatUsers.Values)
                        {
                            this.userCurrencyData.Add(ChannelSession.Settings.UserData[chatUser.ID]);
                        }
                    });
                }
                catch (ThreadAbortException) { return; }
                catch (Exception ex) { }
            }

            this.backgroundThreadCancellationTokenSource.Token.ThrowIfCancellationRequested();
        }

        private async void CurrencyToggleSwitch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.CurrencyNameTextBox.Text))
            {
                MessageBoxHelper.ShowDialog("A currency name must be specified");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            int currencyAmount = 0;
            if (string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) || !int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount) || currencyAmount < 1)
            {
                MessageBoxHelper.ShowDialog("A valid currency amount must be specified");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            int currencyTime = 0;
            if (string.IsNullOrEmpty(this.CurrencyTimeTextBox.Text) || !int.TryParse(this.CurrencyTimeTextBox.Text, out currencyTime) || currencyTime < 1)
            {
                MessageBoxHelper.ShowDialog("A valid currency interval must be specified");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.CurrencyName = this.CurrencyNameTextBox.Text;
                ChannelSession.Settings.CurrencyAcquireAmount = currencyAmount;
                ChannelSession.Settings.CurrencyAcquireInterval = currencyTime;
                ChannelSession.Settings.CurrencyEnabled = true;

                await ChannelSession.Settings.Save();

                this.userCurrencyData.Clear();
                foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values)
                {
                    this.userCurrencyData.Add(userData);
                }
            });


            this.CurrencyNameTextBox.IsEnabled = false;
            this.CurrencyAmountTextBox.IsEnabled = false;
            this.CurrencyTimeTextBox.IsEnabled = false;
            this.UserCurrencyListView.IsEnabled = true;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => { this.CurrencyAcquireBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async void CurrencyToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.CurrencyEnabled = false;

                await ChannelSession.Settings.Save();
            });

            this.CurrencyNameTextBox.IsEnabled = true;
            this.CurrencyAmountTextBox.IsEnabled = true;
            this.CurrencyTimeTextBox.IsEnabled = true;
            this.UserCurrencyListView.IsEnabled = false;

            this.userCurrencyData.Clear();

            this.backgroundThreadCancellationTokenSource.Cancel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.backgroundThreadCancellationTokenSource.Cancel();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataViewModel userData = (UserDataViewModel)button.DataContext;
            userData.CurrencyAmount = 0;
        }
    }
}
