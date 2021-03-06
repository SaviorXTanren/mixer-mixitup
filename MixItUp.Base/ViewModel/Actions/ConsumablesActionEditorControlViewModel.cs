using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class ConsumableViewModel : UIViewModelBase
    {
        public Guid ID
        {
            get
            {
                if (this.Currency != null) { return this.Currency.ID; }
                else if (this.Inventory != null) { return this.Inventory.ID; }
                else if (this.StreamPass != null) { return this.StreamPass.ID; }
                return Guid.Empty;
            }
        }

        public string Name
        {
            get
            {
                if (this.Currency != null) { return this.Currency.Name; }
                else if (this.Inventory != null) { return this.Inventory.Name; }
                else if (this.StreamPass != null) { return this.StreamPass.Name; }
                return null;
            }
        }

        public CurrencyModel Currency { get; set; }

        public InventoryModel Inventory { get; set; }

        public StreamPassModel StreamPass { get; set; }

        public ConsumableViewModel(CurrencyModel currency) { this.Currency = currency; }

        public ConsumableViewModel(InventoryModel inventory) { this.Inventory = inventory; }

        public ConsumableViewModel(StreamPassModel streamPass) { this.StreamPass = streamPass; }
    }

    public class ConsumablesActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Consumables; } }

        public ThreadSafeObservableCollection<ConsumableViewModel> Consumables { get; set; } = new ThreadSafeObservableCollection<ConsumableViewModel>();

        public ConsumableViewModel SelectedConsumable
        {
            get { return this.selectedConsumable; }
            set
            {
                this.selectedConsumable = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowInventoryItems");
                this.NotifyPropertyChanged("InventoryItems");
                this.NotifyPropertyChanged("InventoryItemName");
            }
        }
        private ConsumableViewModel selectedConsumable;

        public IEnumerable<ConsumablesActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<ConsumablesActionTypeEnum>(); } }

        public ConsumablesActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowInventoryItems");
                this.NotifyPropertyChanged("CanEnterAmount");
                this.NotifyPropertyChanged("ShowExtraOptions");
                this.NotifyPropertyChanged("CanDeductFromUser");
                this.NotifyPropertyChanged("CanSelectUsersToApplyTo");
                this.NotifyPropertyChanged("CanEnterTargetUsername");
            }
        }
        private ConsumablesActionTypeEnum selectedActionType;

        public bool ShowInventoryItems { get { return this.SelectedConsumable != null && this.SelectedConsumable.Inventory != null && this.CanEnterAmount; } }

        public IEnumerable<InventoryItemModel> InventoryItems
        {
            get
            {
                if (this.SelectedConsumable != null && this.SelectedConsumable.Inventory != null)
                {
                    return this.SelectedConsumable.Inventory.Items.Values.ToList();
                }
                return null;
            }
        }

        public string InventoryItemName
        {
            get { return this.inventoryItemName; }
            set
            {
                this.inventoryItemName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string inventoryItemName;

        public string Amount
        {
            get { return this.amount; }
            set
            {
                this.amount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string amount;

        public bool CanEnterAmount { get { return this.SelectedActionType != ConsumablesActionTypeEnum.ResetForAllUsers && this.SelectedActionType != ConsumablesActionTypeEnum.ResetForUser; } }

        public bool ShowExtraOptions
        {
            get
            {
                return this.SelectedActionType == ConsumablesActionTypeEnum.AddToSpecificUser || this.SelectedActionType == ConsumablesActionTypeEnum.AddToAllChatUsers ||
                    this.SelectedActionType == ConsumablesActionTypeEnum.SubtractFromSpecificUser || this.SelectedActionType == ConsumablesActionTypeEnum.SubtractFromAllChatUsers;
            }
        }

        public bool DeductFromUser
        {
            get { return this.deductFromUser; }
            set
            {
                this.deductFromUser = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool deductFromUser;

        public bool CanDeductFromUser { get { return this.SelectedActionType == ConsumablesActionTypeEnum.AddToSpecificUser || this.SelectedActionType == ConsumablesActionTypeEnum.AddToAllChatUsers; } }

        public IEnumerable<UserRoleEnum> UsersToApplyTo { get { return UserDataModel.GetSelectableUserRoles(); } }

        public UserRoleEnum SelectedUsersToApplyTo
        {
            get { return this.selectedUsersToApplyTo; }
            set
            {
                this.selectedUsersToApplyTo = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum selectedUsersToApplyTo;

        public bool CanSelectUsersToApplyTo { get { return this.SelectedActionType == ConsumablesActionTypeEnum.AddToAllChatUsers || this.SelectedActionType == ConsumablesActionTypeEnum.SubtractFromAllChatUsers; } }

        public string TargetUsername
        {
            get { return this.targetUsername; }
            set
            {
                this.targetUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string targetUsername;

        public bool CanEnterTargetUsername { get { return this.SelectedActionType == ConsumablesActionTypeEnum.AddToSpecificUser || this.SelectedActionType == ConsumablesActionTypeEnum.SubtractFromSpecificUser; } }

        public ConsumablesActionEditorControlViewModel(ConsumablesActionModel action)
            : base(action)
        {
            this.LoadConsumables();
            if (action.CurrencyID != Guid.Empty)
            {
                this.SelectedConsumable = this.Consumables.FirstOrDefault(c => c.ID.Equals(action.CurrencyID));
            }
            else if (action.InventoryID != Guid.Empty)
            {
                this.SelectedConsumable = this.Consumables.FirstOrDefault(c => c.ID.Equals(action.InventoryID));
                this.InventoryItemName = action.ItemName;
            }
            else if (action.StreamPassID != Guid.Empty)
            {
                this.SelectedConsumable = this.Consumables.FirstOrDefault(c => c.ID.Equals(action.StreamPassID));
            }
            this.SelectedActionType = action.ActionType;
            this.Amount = action.Amount;

            this.DeductFromUser = action.DeductFromUser;
            this.SelectedUsersToApplyTo = action.UsersToApplyTo;
            this.TargetUsername = action.Username;
        }

        public ConsumablesActionEditorControlViewModel() : base() { this.LoadConsumables(); }

        public override Task<Result> Validate()
        {
            if (this.SelectedConsumable == null || this.SelectedConsumable.ID == Guid.Empty)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ConsumableActionSelectConsumable));
            }

            if (this.SelectedConsumable.Inventory != null)
            {
                if (this.SelectedActionType != ConsumablesActionTypeEnum.ResetForAllUsers && this.SelectedActionType != ConsumablesActionTypeEnum.ResetForUser)
                {
                    if (string.IsNullOrEmpty(this.InventoryItemName))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.ConsumableActionSelectInventoryItem));
                    }
                }
            }

            if (this.CanEnterAmount && string.IsNullOrEmpty(this.Amount))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ConsumableActionMissingAmount));
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.SelectedConsumable.Currency != null)
            {
                return Task.FromResult<ActionModelBase>(new ConsumablesActionModel(this.SelectedConsumable.Currency, this.SelectedActionType, this.Amount, this.TargetUsername, this.SelectedUsersToApplyTo,
                    this.DeductFromUser));
            }
            else if (this.SelectedConsumable.Inventory != null)
            {
                return Task.FromResult<ActionModelBase>(new ConsumablesActionModel(this.SelectedConsumable.Inventory, this.SelectedActionType, this.InventoryItemName, this.Amount, this.TargetUsername,
                    this.SelectedUsersToApplyTo, this.DeductFromUser));
            }
            else if (this.SelectedConsumable.StreamPass != null)
            {
                return Task.FromResult<ActionModelBase>(new ConsumablesActionModel(this.SelectedConsumable.StreamPass, this.SelectedActionType, this.Amount, this.TargetUsername, this.SelectedUsersToApplyTo,
                    this.DeductFromUser));
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        private void LoadConsumables()
        {
            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                this.Consumables.Add(new ConsumableViewModel(currency));
            }
            foreach (InventoryModel inventory in ChannelSession.Settings.Inventory.Values)
            {
                this.Consumables.Add(new ConsumableViewModel(inventory));
            }
            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            {
                this.Consumables.Add(new ConsumableViewModel(streamPass));
            }
        }
    }
}
