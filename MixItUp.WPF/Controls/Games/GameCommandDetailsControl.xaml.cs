using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for GameCommandDetailsControl.xaml
    /// </summary>
    public partial class GameCommandDetailsControl : LoadingControlBase
    {
        private GameCommandBase existingCommand;

        public GameCommandDetailsControl()
        {
            InitializeComponent();
        }

        public GameCommandDetailsControl(GameCommandBase existingCommand)
            : this()
        {
            this.existingCommand = existingCommand;
        }

        public string GameName { get { return this.NameTextBox.Text; } }
        public IEnumerable<string> ChatTriggers { get { return this.ChatCommandTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(t => "!" + t); } }
        public UserCurrencyViewModel Currency { get { return (UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem; } }
        public int RequiredAmount { get { return int.Parse(this.RequiredAmountTextBox.Text); } }
        public int MinimumAmount { get { return int.Parse(this.MinimumAmountTextBox.Text); } }
        public int MaximumAmount { get { return int.Parse(this.MaximumAmountTextBox.Text); } }

        public void SetAsRequiredAmount()
        {
            this.RequiredAmountTextBox.Visibility = Visibility.Visible;
            this.MinimumAmountTextBox.Visibility = Visibility.Collapsed;
            this.MaximumAmountTextBox.Visibility = Visibility.Collapsed;
        }

        public void SetDefaultValues(string name, string triggers, int minimum, int maximum = 0)
        {
            this.NameTextBox.Text = name;
            this.ChatCommandTextBox.Text = triggers;
            this.CurrencyTypeComboBox.SelectedIndex = 0;
            this.RequiredAmountTextBox.Text = minimum.ToString();
            this.MinimumAmountTextBox.Text = minimum.ToString();
            this.MaximumAmountTextBox.Text = (maximum > 0) ? maximum.ToString() : string.Empty;
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A Game Name is required");
                return false;
            }

            if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("At least 1 chat trigger must be specified");
                return false;
            }

            if (!CommandBase.IsValidCommandString(this.ChatCommandTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("The chat triggers contain an invalid character");
                return false;
            }

            IEnumerable<string> commandStrings = this.GetChatTriggers();
            if (commandStrings.GroupBy(c => c).Where(g => g.Count() > 1).Count() > 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Each chat trigger must be unique");
                return false;
            }

            if (!await this.Requirements.Validate())
            {
                return false;
            }

            if (this.CurrencyTypeComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A currency must be selected");
                return false;
            }

            if (this.RequiredAmountTextBox.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(this.RequiredAmountTextBox.Text) || !int.TryParse(this.RequiredAmountTextBox.Text, out int amount) || amount <= 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A valid required amount must be specified");
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(this.MinimumAmountTextBox.Text) || !int.TryParse(this.MinimumAmountTextBox.Text, out int minimum) || minimum <= 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A valid minimum amount must be specified");
                    return false;
                }

                int maximum = 0;
                if (!string.IsNullOrEmpty(this.MaximumAmountTextBox.Text))
                {
                    if (!int.TryParse(this.MaximumAmountTextBox.Text, out maximum) || maximum <= 0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("A valid maximum amount must be specified");
                        return false;
                    }
                }

                if (maximum > 0 && maximum < minimum)
                {
                    await MessageBoxHelper.ShowMessageDialog("Maximum amount must be greater than or equal to minimum amount");
                    return false;
                }
            }

            foreach (PermissionsCommandBase command in ChannelSession.AllChatCommands)
            {
                if (command.IsEnabled && this.existingCommand != command)
                {
                    if (commandStrings.Any(c => command.Commands.Contains(c)))
                    {
                        await MessageBoxHelper.ShowMessageDialog("There already exists a command that uses one of the chat triggers you have specified");
                        return false;
                    }
                }
            }

            return true;
        }

        protected override Task OnLoaded()
        {
            this.Requirements.HideCurrencyRequirement();
            this.Requirements.HideThresholdRequirement();

            IEnumerable<UserCurrencyViewModel> currencies = ChannelSession.Settings.Currencies.Values;
            this.IsEnabled = (currencies.Count() > 0);
            this.CurrencyTypeComboBox.ItemsSource = currencies;

            if (this.existingCommand != null)
            {
                this.NameTextBox.Text = this.existingCommand.Name;
                this.ChatCommandTextBox.Text = this.existingCommand.CommandsString.Replace("!", "");
                this.CurrencyTypeComboBox.SelectedItem = this.existingCommand.Requirements.Currency.GetCurrency();
                this.RequiredAmountTextBox.Text = this.existingCommand.Requirements.Currency.RequiredAmount.ToString();
                this.MinimumAmountTextBox.Text = this.existingCommand.Requirements.Currency.RequiredAmount.ToString();
                this.MaximumAmountTextBox.Text = (this.existingCommand.Requirements.Currency.MaximumAmount > 0) ? this.existingCommand.Requirements.Currency.MaximumAmount.ToString() : string.Empty;

                if (this.existingCommand.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
                {
                    this.SetAsRequiredAmount();
                }
            }

            return base.OnLoaded();
        }

        private IEnumerable<string> GetChatTriggers()
        {
            return ;
        }
    }
}
