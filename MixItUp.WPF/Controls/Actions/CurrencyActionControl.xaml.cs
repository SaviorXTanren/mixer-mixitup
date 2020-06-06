using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for CurrencyActionControl.xaml
    /// </summary>
    public partial class CurrencyActionControl : ActionControlBase
    {
        private CurrencyAction action;

        public CurrencyActionControl() : base() { InitializeComponent(); }

        public CurrencyActionControl(CurrencyAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            List<object> currencyInventoryList = new List<object>();
            currencyInventoryList.AddRange(ChannelSession.Settings.Currency.Values);
            currencyInventoryList.AddRange(ChannelSession.Settings.Inventory.Values);
            currencyInventoryList.AddRange(ChannelSession.Settings.StreamPass.Values);
            this.CurrencyTypeComboBox.ItemsSource = currencyInventoryList;
            this.CurrencyActionTypeComboBox.ItemsSource = Enum.GetValues(typeof(CurrencyActionTypeEnum))
                .Cast<CurrencyActionTypeEnum>()
                .OrderBy(s => EnumLocalizationHelper.GetLocalizedName(s));
            this.CurrencyPermissionsAllowedComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;

            this.CurrencyPermissionsAllowedComboBox.SelectedIndex = 0;

            if (this.action != null)
            {
                if (this.action.CurrencyID != Guid.Empty && ChannelSession.Settings.Currency.ContainsKey(this.action.CurrencyID))
                {
                    this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.Currency[this.action.CurrencyID];
                }
                else if (this.action.InventoryID != Guid.Empty && ChannelSession.Settings.Inventory.ContainsKey(this.action.InventoryID))
                {
                    this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.Inventory[this.action.InventoryID];
                }
                else if (this.action.StreamPassID != Guid.Empty && ChannelSession.Settings.StreamPass.ContainsKey(this.action.StreamPassID))
                {
                    this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.StreamPass[this.action.StreamPassID];
                }
                this.CurrencyActionTypeComboBox.SelectedItem = this.action.CurrencyActionType;
                this.InventoryItemNameComboBox.Text = this.action.ItemName;
                this.CurrencyAmountTextBox.Text = this.action.Amount;
                this.CurrencyUsernameTextBox.Text = this.action.Username;
                this.CurrencyPermissionsAllowedComboBox.SelectedItem = this.action.RoleRequirement;
                this.DeductFromUserToggleButton.IsChecked = this.action.DeductFromUser;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0 && this.CurrencyActionTypeComboBox.SelectedIndex >= 0)
            {
                CurrencyModel currency = this.GetSelectedCurrency();
                InventoryModel inventory = this.GetSelectedInventory();
                StreamPassModel streamPass = this.GetSelectedStreamPass();
                CurrencyActionTypeEnum actionType = (CurrencyActionTypeEnum)this.CurrencyActionTypeComboBox.SelectedItem;

                if (actionType == CurrencyActionTypeEnum.ResetForAllUsers || actionType == CurrencyActionTypeEnum.ResetForUser || !string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text))
                {
                    if (actionType == CurrencyActionTypeEnum.AddToSpecificUser)
                    {
                        if (string.IsNullOrEmpty(this.CurrencyUsernameTextBox.Text))
                        {
                            return null;
                        }
                    }

                    UserRoleEnum roleRequirement = UserRoleEnum.User;
                    if (actionType == CurrencyActionTypeEnum.AddToAllChatUsers || actionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                    {
                        if (this.CurrencyPermissionsAllowedComboBox.SelectedIndex < 0)
                        {
                            return null;
                        }
                        roleRequirement = (UserRoleEnum)this.CurrencyPermissionsAllowedComboBox.SelectedItem;
                    }

                    if (currency != null)
                    {
                        return new CurrencyAction(currency, actionType, this.CurrencyAmountTextBox.Text, username: this.CurrencyUsernameTextBox.Text,
                            roleRequirement: roleRequirement, deductFromUser: this.DeductFromUserToggleButton.IsChecked.GetValueOrDefault());
                    }
                    else if (inventory != null)
                    {
                        if (actionType == CurrencyActionTypeEnum.ResetForAllUsers || actionType == CurrencyActionTypeEnum.ResetForUser)
                        {
                            return new CurrencyAction(inventory, actionType);
                        }
                        else if (!string.IsNullOrEmpty(this.InventoryItemNameComboBox.Text))
                        {
                            return new CurrencyAction(inventory, actionType, this.InventoryItemNameComboBox.Text, this.CurrencyAmountTextBox.Text,
                                username: this.CurrencyUsernameTextBox.Text, roleRequirement: roleRequirement, deductFromUser: this.DeductFromUserToggleButton.IsChecked.GetValueOrDefault());
                        }
                    }
                    else if (streamPass != null)
                    {
                        return new CurrencyAction(streamPass, actionType, this.CurrencyAmountTextBox.Text, username: this.CurrencyUsernameTextBox.Text,
                            roleRequirement: roleRequirement, deductFromUser: this.DeductFromUserToggleButton.IsChecked.GetValueOrDefault());
                    }
                }
            }
            return null;
        }

        private void CurrencyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0)
            {
                this.CurrencyActionTypeComboBox.IsEnabled = this.CurrencyUsernameTextBox.IsEnabled = this.CurrencyAmountTextBox.IsEnabled =
                    this.DeductFromUserTextBlock.IsEnabled = this.DeductFromUserToggleButton.IsEnabled = true;

                if (this.GetSelectedInventory() != null)
                {
                    this.InventoryItemNameComboBox.Visibility = Visibility.Visible;
                    this.InventoryItemNameComboBox.ItemsSource = this.GetSelectedInventory().Items.Values.Select(i => i.Name);
                }
                else
                {
                    this.InventoryItemNameComboBox.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CurrencyActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CurrencyActionTypeComboBox.SelectedIndex >= 0)
            {
                CurrencyActionTypeEnum actionType = (CurrencyActionTypeEnum)this.CurrencyActionTypeComboBox.SelectedItem;
                this.GiveToGrid.Visibility = (actionType == CurrencyActionTypeEnum.AddToSpecificUser || actionType == CurrencyActionTypeEnum.AddToAllChatUsers ||
                    actionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || actionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers) ?
                    Visibility.Visible : Visibility.Collapsed;

                this.CurrencyUsernameTextBox.Visibility = (actionType == CurrencyActionTypeEnum.AddToSpecificUser || actionType == CurrencyActionTypeEnum.SubtractFromSpecificUser) ?
                    Visibility.Visible : Visibility.Collapsed;

                this.CurrencyPermissionsAllowedComboBox.Visibility = (actionType == CurrencyActionTypeEnum.AddToAllChatUsers || actionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers) ?
                    Visibility.Visible : Visibility.Collapsed;

                this.DeductFromUserTextBlock.IsEnabled = this.DeductFromUserToggleButton.IsEnabled =
                    (actionType == CurrencyActionTypeEnum.AddToSpecificUser || actionType == CurrencyActionTypeEnum.AddToAllChatUsers) ? true : false;

                this.InventoryItemNameComboBox.IsEnabled = (actionType != CurrencyActionTypeEnum.ResetForAllUsers && actionType != CurrencyActionTypeEnum.ResetForUser);

                this.CurrencyAmountTextBox.IsEnabled = (actionType != CurrencyActionTypeEnum.ResetForAllUsers && actionType != CurrencyActionTypeEnum.ResetForUser);
            }
        }

        private CurrencyModel GetSelectedCurrency()
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0 && this.CurrencyTypeComboBox.SelectedItem is CurrencyModel)
            {
                return (CurrencyModel)this.CurrencyTypeComboBox.SelectedItem;
            }
            return null;
        }

        private InventoryModel GetSelectedInventory()
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0 && this.CurrencyTypeComboBox.SelectedItem is InventoryModel)
            {
                return (InventoryModel)this.CurrencyTypeComboBox.SelectedItem;
            }
            return null;
        }

        private StreamPassModel GetSelectedStreamPass()
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0 && this.CurrencyTypeComboBox.SelectedItem is StreamPassModel)
            {
                return (StreamPassModel)this.CurrencyTypeComboBox.SelectedItem;
            }
            return null;
        }
    }
}
