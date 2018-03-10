using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;
using System.Windows.Controls;

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
                    this.CurrencyUsernameTextBox.IsEnabled = true;
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
                return new CurrencyAction(currency, this.CurrencyUsernameTextBox.Text, currencyAmount, this.CurrencyMessageTextBox.Text, this.CurrencyWhisperToggleButton.IsChecked.GetValueOrDefault());
            }
            return null;
        }

        private void CurrencyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0)
            {
                this.CurrencyUsernameTextBox.IsEnabled = true;
                this.CurrencyAmountTextBox.IsEnabled = true;
            }
        }
    }
}
