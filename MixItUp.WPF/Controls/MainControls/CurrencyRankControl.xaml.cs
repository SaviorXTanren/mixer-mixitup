using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CurrencyRankControl.xaml
    /// </summary>
    public partial class CurrencyRankControl : MainControlBase
    {
        private ObservableCollection<UserRankViewModel> ranks = new ObservableCollection<UserRankViewModel>();

        public CurrencyRankControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.RanksListView.ItemsSource = this.ranks;
            this.ranks.Clear();
            foreach (UserRankViewModel rank in ChannelSession.Settings.Ranks.OrderBy(r => r.MinimumPoints))
            {
                this.ranks.Add(rank);
            }

            this.CurrencyNameTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.Name;
            this.CurrencyAmountTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.AcquireAmount.ToString();
            this.CurrencyTimeTextBox.Text = ChannelSession.Settings.CurrencyAcquisition.AcquireInterval.ToString();
            this.CurrencyToggleSwitch.IsChecked = ChannelSession.Settings.CurrencyAcquisition.Enabled;
            this.CurrencyGrid.IsEnabled = !ChannelSession.Settings.CurrencyAcquisition.Enabled;

            this.RankPointsNameTextBox.Text = ChannelSession.Settings.RankAcquisition.Name;
            this.RankPointsAmountTextBox.Text = ChannelSession.Settings.RankAcquisition.AcquireAmount.ToString();
            this.RankPointsTimeTextBox.Text = ChannelSession.Settings.RankAcquisition.AcquireInterval.ToString();
            this.RankToggleSwitch.IsChecked = ChannelSession.Settings.RankAcquisition.Enabled;
            this.RankGrid.IsEnabled = !ChannelSession.Settings.RankAcquisition.Enabled;

            CustomCommand rankChangeCommand = ChannelSession.Settings.RankChanged; 
            if (rankChangeCommand == null)
            {
                rankChangeCommand = new CustomCommand("RankChanged");
                ChannelSession.Settings.RankChanged = rankChangeCommand;
            }
            this.RankChangedCommandControl.Initialize(this.Window, ChannelSession.Settings.RankChanged);
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

        private async void RankToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.RankPointsNameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A points name must be specified");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int rankAmount = 0;
            if (string.IsNullOrEmpty(this.RankPointsAmountTextBox.Text) || !int.TryParse(this.RankPointsAmountTextBox.Text, out rankAmount) || rankAmount < 1)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid points amount must be specified");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int rankTime = 0;
            if (string.IsNullOrEmpty(this.RankPointsTimeTextBox.Text) || !int.TryParse(this.RankPointsTimeTextBox.Text, out rankTime) || rankTime < 1)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid points interval must be specified");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            if (this.ranks.Count() < 1)
            {
                await MessageBoxHelper.ShowMessageDialog("At least one rank must be created");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.RankAcquisition.Name = this.RankPointsNameTextBox.Text;
                ChannelSession.Settings.RankAcquisition.AcquireAmount = rankAmount;
                ChannelSession.Settings.RankAcquisition.AcquireInterval = rankTime;
                ChannelSession.Settings.RankAcquisition.Enabled = true;

                await ChannelSession.SaveSettings();
            });

            this.RankGrid.IsEnabled = false;
        }

        private async void RankToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.RankAcquisition.Enabled = false;

                await ChannelSession.SaveSettings();
            });

            this.RankGrid.IsEnabled = true;
        }

        private void DeleteRankButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserRankViewModel rank = (UserRankViewModel)button.DataContext;
            ChannelSession.Settings.Ranks.Remove(rank);
            this.ranks.Remove(rank);
        }

        private async void AddRankButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.RankNameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A rank name must be specified");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int rankAmount = 0;
            if (string.IsNullOrEmpty(this.RankAmountTextBox.Text) || !int.TryParse(this.RankAmountTextBox.Text, out rankAmount) || rankAmount < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A rank amount must be specified");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            if (this.ranks.Any(r => r.Name.Equals(this.RankNameTextBox.Text) || r.MinimumPoints == rankAmount))
            {
                await MessageBoxHelper.ShowMessageDialog("Every rank must have a unique name and minimum amount");
                this.CurrencyToggleSwitch.IsChecked = false;
                return;
            }

            UserRankViewModel newRank = new UserRankViewModel(this.RankNameTextBox.Text, rankAmount);
            ChannelSession.Settings.Ranks.Add(newRank);

            this.ranks.Clear();
            foreach (UserRankViewModel rank in ChannelSession.Settings.Ranks.OrderBy(r => r.MinimumPoints))
            {
                this.ranks.Add(rank);
            }

            ChannelSession.Settings.Ranks = this.ranks.ToList();

            this.RankNameTextBox.Clear();
            this.RankAmountTextBox.Clear();
        }
    }
}
