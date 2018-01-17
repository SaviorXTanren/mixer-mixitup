using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for CurrencyActionControl.xaml
    /// </summary>
    public partial class CurrencyActionControl : ActionControlBase
    {
        private CurrencyAction action;

        public CurrencyActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public CurrencyActionControl(ActionContainerControl containerControl, CurrencyAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.CurrencyTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values;
            if (this.action != null)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(this.action.CurrencyID))
                {
                    this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.Currencies[this.action.CurrencyID];
                    this.CurrencyAmountTextBox.IsEnabled = true;
                }
                this.CurrencyAmountTextBox.Text = this.action.Amount.ToString();
                this.CurrencyMessageTextBox.Text = this.action.ChatText;
                this.CurrencyWhisperToggleButton.IsChecked = this.action.IsWhisper;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            int currencyAmount;
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0 && int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount))
            {
                UserCurrencyViewModel currency = (UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem;
                return new CurrencyAction(currency, currencyAmount, this.CurrencyMessageTextBox.Text, this.CurrencyWhisperToggleButton.IsChecked.GetValueOrDefault());
            }
            return null;
        }

        private void CurrencyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0)
            {
                this.CurrencyAmountTextBox.IsEnabled = true;
            }
        }
    }
}
