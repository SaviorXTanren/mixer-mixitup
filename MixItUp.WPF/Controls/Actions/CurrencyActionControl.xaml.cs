using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    public enum CurrencyGiveToTypeEnum
    {
        [Name("Current User")]
        CurrentUser,
        [Name("Specific User")]
        SpecificUser,
        [Name("All Chat Users")]
        AllChatUsers
    }

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
            this.GiveToComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyGiveToTypeEnum>();

            if (this.action != null)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(this.action.CurrencyID))
                {
                    this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.Currencies[this.action.CurrencyID];
                }

                this.CurrencyAmountTextBox.Text = this.action.Amount.ToString();
                if (!string.IsNullOrEmpty(this.action.Username))
                {
                    this.GiveToComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyGiveToTypeEnum.SpecificUser);
                }
                else if (this.action.GiveToAllUsers)
                {
                    this.GiveToComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyGiveToTypeEnum.AllChatUsers);
                }
                else
                {
                    this.GiveToComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyGiveToTypeEnum.CurrentUser);
                }

                this.CurrencyUsernameTextBox.Text = this.action.Username;

                this.DeductFromUserToggleButton.IsChecked = this.action.DeductFromUser;
                this.CurrencyWhisperToggleButton.IsChecked = this.action.IsWhisper;
                this.CurrencyMessageTextBox.Text = this.action.ChatText;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) && this.GiveToComboBox.SelectedIndex >= 0)
            {
                CurrencyGiveToTypeEnum giveTo = EnumHelper.GetEnumValueFromString<CurrencyGiveToTypeEnum>((string)this.GiveToComboBox.SelectedItem);
                if (giveTo == CurrencyGiveToTypeEnum.SpecificUser)
                {
                    if (string.IsNullOrEmpty(this.CurrencyUsernameTextBox.Text))
                    {
                        return null;
                    }
                }
                else
                {
                    this.CurrencyUsernameTextBox.Text = null;
                }

                UserCurrencyViewModel currency = (UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem;
                return new CurrencyAction(currency, this.CurrencyUsernameTextBox.Text, (giveTo == CurrencyGiveToTypeEnum.AllChatUsers), this.CurrencyAmountTextBox.Text,
                    this.DeductFromUserToggleButton.IsChecked.GetValueOrDefault(), this.CurrencyMessageTextBox.Text, this.CurrencyWhisperToggleButton.IsChecked.GetValueOrDefault());
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

        private void GiveToComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.GiveToComboBox.SelectedIndex >= 0)
            {
                CurrencyGiveToTypeEnum giveTo = EnumHelper.GetEnumValueFromString<CurrencyGiveToTypeEnum>((string)this.GiveToComboBox.SelectedItem);
                this.CurrencyUsernameTextBox.IsEnabled = (giveTo == CurrencyGiveToTypeEnum.SpecificUser);
            }
        }
    }
}
