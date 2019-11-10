using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
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

            this.Requirements.HideCurrencyRequirement();
            this.Requirements.HideThresholdRequirement();

            IEnumerable<UserCurrencyViewModel> currencies = ChannelSession.Settings.Currencies.Values;
            this.IsEnabled = (currencies.Count() > 0);
            this.CurrencyTypeComboBox.ItemsSource = currencies;
            this.CurrencyTypeComboBox.SelectedIndex = 0;

            this.CurrencyRequirementComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyRequirementTypeEnum>();
            this.CurrencyRequirementComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyRequirementTypeEnum.MinimumAndMaximum);
        }

        public string GameName { get { return this.NameTextBox.Text; } }
        public IEnumerable<string> ChatTriggers { get { return this.ChatCommandTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries); } }
        public UserCurrencyViewModel Currency { get { return (UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem; } }
        public CurrencyRequirementTypeEnum CurrencyUsage
        {
            get
            {
                return (this.CurrencyRequirementComboBox.SelectedIndex >= 0) ? EnumHelper.GetEnumValueFromString<CurrencyRequirementTypeEnum>((string)this.CurrencyRequirementComboBox.SelectedItem)
                    : CurrencyRequirementTypeEnum.MinimumAndMaximum;
            }
        }
        public int RequiredAmount { get { return int.Parse(this.RequiredAmountTextBox.Text); } }
        public int MinimumAmount { get { return int.Parse(this.MinimumAmountTextBox.Text); } }
        public int MaximumAmount { get { return int.Parse(this.MaximumAmountTextBox.Text); } }

        public void SetAsMinimumOnly()
        {
            this.CurrencyRequirementComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyRequirementTypeEnum.MinimumOnly);
            this.CurrencyRequirementComboBox.IsEnabled = false;
        }

        public void SetAsNoCostOnly()
        {
            this.CurrencyTypeComboBox.IsEnabled = false;
            this.CurrencyRequirementComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyRequirementTypeEnum.NoCurrencyCost);
            this.CurrencyRequirementComboBox.IsEnabled = false;
        }

        public void SetAsRequiredAmount()
        {
            this.RequiredAmountTextBox.Visibility = Visibility.Visible;
            this.MinimumAmountTextBox.Visibility = Visibility.Collapsed;
            this.MaximumAmountTextBox.Visibility = Visibility.Collapsed;
        }

        public RequirementViewModel GetRequirements()
        {
            RequirementViewModel requirements = this.Requirements.GetRequirements();

            UserCurrencyViewModel currency = (UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem;
            CurrencyRequirementTypeEnum requirement = this.CurrencyUsage;
            if (requirement == CurrencyRequirementTypeEnum.NoCurrencyCost)
            {
                requirements.Currency = new CurrencyRequirementViewModel(currency, requirement, 0);
            }
            else if (requirement == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                int.TryParse(this.RequiredAmountTextBox.Text, out int required);
                requirements.Currency = new CurrencyRequirementViewModel(currency, requirement, required);
            }
            else if (requirement == CurrencyRequirementTypeEnum.MinimumOnly)
            {
                int.TryParse(this.MinimumAmountTextBox.Text, out int minimum);
                requirements.Currency = new CurrencyRequirementViewModel(currency, requirement, minimum);
            }
            else if (requirement == CurrencyRequirementTypeEnum.MinimumAndMaximum)
            {
                int.TryParse(this.MinimumAmountTextBox.Text, out int minimum);
                int.TryParse(this.MaximumAmountTextBox.Text, out int maximum);
                requirements.Currency = new CurrencyRequirementViewModel(currency, minimum, maximum);
            }

            return requirements;
        }

        public void SetDefaultValues(string name, string triggers, CurrencyRequirementTypeEnum currencyRequirement, int minimum = 0, int maximum = 0)
        {
            this.NameTextBox.Text = name;
            this.ChatCommandTextBox.Text = triggers;
            this.CurrencyRequirementComboBox.SelectedItem = EnumHelper.GetEnumName(currencyRequirement);
            this.RequiredAmountTextBox.Text = minimum.ToString();
            this.MinimumAmountTextBox.Text = minimum.ToString();
            this.MaximumAmountTextBox.Text = (maximum > 0) ? maximum.ToString() : string.Empty;
        }

        public void SetDefaultValues(GameCommandBase command)
        {
            this.existingCommand = command;
            this.SetDefaultValues(command.Name, command.CommandsString, command.Requirements.Currency.RequirementType, command.Requirements.Currency.RequiredAmount, command.Requirements.Currency.MaximumAmount);
            this.CurrencyTypeComboBox.SelectedItem = command.Requirements.Currency.GetCurrency();
            this.Requirements.RoleRequirement.SetRoleRequirement(this.existingCommand.Requirements.Role);
            this.Requirements.CooldownRequirement.SetCooldownRequirement(this.existingCommand.Requirements.Cooldown);
            this.Requirements.CurrencyRankInventoryRequirement.RankRequirement.SetCurrencyRequirement(this.existingCommand.Requirements.Rank);
            this.Requirements.CurrencyRankInventoryRequirement.InventoryRequirement.SetInventoryRequirement(this.existingCommand.Requirements.Inventory);
            this.Requirements.SettingsRequirement.SetSettingsRequirement(this.existingCommand.Requirements.Settings);
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                await DialogHelper.ShowMessage("A Game Name is required");
                return false;
            }

            if (string.IsNullOrEmpty(this.ChatCommandTextBox.Text))
            {
                await DialogHelper.ShowMessage("At least 1 chat trigger must be specified");
                return false;
            }

            if (!CommandBase.IsValidCommandString(this.ChatCommandTextBox.Text))
            {
                await DialogHelper.ShowMessage("The chat triggers contain an invalid character");
                return false;
            }

            IEnumerable<string> commandStrings = this.ChatTriggers;
            if (commandStrings.GroupBy(c => c).Where(g => g.Count() > 1).Count() > 0)
            {
                await DialogHelper.ShowMessage("Each chat trigger must be unique");
                return false;
            }

            if (!await this.Requirements.Validate())
            {
                return false;
            }

            if (this.CurrencyTypeComboBox.SelectedIndex < 0)
            {
                await DialogHelper.ShowMessage("A currency must be selected");
                return false;
            }

            if (this.CurrencyRequirementComboBox.SelectedIndex < 0)
            {
                await DialogHelper.ShowMessage("A currency usage must be selected");
                return false;
            }
            CurrencyRequirementTypeEnum requirement = this.CurrencyUsage;

            int minimum = 0;
            int maximum = 0;

            if (requirement == CurrencyRequirementTypeEnum.MinimumOnly || requirement == CurrencyRequirementTypeEnum.MinimumAndMaximum)
            {
                if (string.IsNullOrEmpty(this.MinimumAmountTextBox.Text) || !int.TryParse(this.MinimumAmountTextBox.Text, out minimum) || minimum <= 0)
                {
                    await DialogHelper.ShowMessage("A valid minimum amount must be specified");
                    return false;
                }
            }
            
            if (requirement == CurrencyRequirementTypeEnum.MinimumAndMaximum)
            {
                if (string.IsNullOrEmpty(this.MaximumAmountTextBox.Text) || !int.TryParse(this.MaximumAmountTextBox.Text, out maximum) || maximum <= 0)
                {
                    await DialogHelper.ShowMessage("A valid maximum amount must be specified");
                    return false;
                }

                if (maximum > 0 && maximum < minimum)
                {
                    await DialogHelper.ShowMessage("Maximum amount must be greater than or equal to minimum amount");
                    return false;
                }
            }

            if (requirement == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                if (string.IsNullOrEmpty(this.RequiredAmountTextBox.Text) || !int.TryParse(this.RequiredAmountTextBox.Text, out minimum) || minimum <= 0)
                {
                    await DialogHelper.ShowMessage("A valid required amount must be specified");
                    return false;
                }
            }

            foreach (PermissionsCommandBase command in ChannelSession.AllChatCommands)
            {
                if (command.IsEnabled && this.existingCommand != command)
                {
                    if (commandStrings.Any(c => command.Commands.Contains(c, StringComparer.InvariantCultureIgnoreCase)))
                    {
                        await DialogHelper.ShowMessage("There already exists a command that uses one of the chat triggers you have specified");
                        return false;
                    }
                }
            }

            return true;
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.NameTextBox.Text = this.existingCommand.Name;
                this.ChatCommandTextBox.Text = this.existingCommand.CommandsString;
                this.CurrencyTypeComboBox.SelectedItem = this.existingCommand.Requirements.Currency.GetCurrency();
                this.CurrencyRequirementComboBox.SelectedItem = EnumHelper.GetEnumName(this.existingCommand.Requirements.Currency.RequirementType);
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

        private void CurrencyRequirementComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.CurrencyRequirementComboBox.SelectedIndex >= 0)
            {
                CurrencyRequirementTypeEnum requirement = this.CurrencyUsage;
                this.RequiredAmountTextBox.Visibility = (requirement == CurrencyRequirementTypeEnum.RequiredAmount) ? Visibility.Visible : Visibility.Collapsed;
                this.MinimumAmountTextBox.Visibility = (requirement == CurrencyRequirementTypeEnum.MinimumOnly || requirement == CurrencyRequirementTypeEnum.MinimumAndMaximum) ? Visibility.Visible : Visibility.Collapsed;
                this.MaximumAmountTextBox.Visibility = (requirement == CurrencyRequirementTypeEnum.MinimumAndMaximum) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
