using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class InventoryListRequirementViewModel : ListRequirementViewModelBase
    {
        public ObservableCollection<InventoryRequirementViewModel> Items { get; set; } = new ObservableCollection<InventoryRequirementViewModel>();

        public ICommand AddItemCommand { get; private set; }

        public InventoryListRequirementViewModel()
        {
            this.AddItemCommand = this.CreateCommand((parameter) =>
            {
                this.Items.Add(new InventoryRequirementViewModel(this));
                return Task.FromResult(0);
            });
        }

        public void Add(InventoryRequirementModel requirement)
        {
            this.Items.Add(new InventoryRequirementViewModel(this, requirement));
        }

        public void Delete(InventoryRequirementViewModel requirement)
        {
            this.Items.Remove(requirement);
        }
    }

    public class InventoryRequirementViewModel : RequirementViewModelBase
    {
        public IEnumerable<InventoryModel> Inventories { get { return ChannelSession.Settings.Inventory.Values; } }

        public InventoryModel SelectedInventory
        {
            get { return this.selectedInventory; }
            set
            {
                this.selectedInventory = value;

                this.SelectedItem = null;

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("Items");
                this.NotifyPropertyChanged("SelectedItem");
            }
        }
        private InventoryModel selectedInventory;

        public IEnumerable<InventoryItemModel> Items
        {
            get
            {
                List<InventoryItemModel> items = new List<InventoryItemModel>();
                if (this.SelectedInventory != null)
                {
                    items.AddRange(this.SelectedInventory.Items.Values);
                }
                return items;
            }
        }

        public InventoryItemModel SelectedItem
        {
            get { return this.selectedItem; }
            set
            {
                this.selectedItem = value;
                this.NotifyPropertyChanged();
            }
        }
        private InventoryItemModel selectedItem;

        public int Amount
        {
            get { return this.amount; }
            set
            {
                if (this.amount >= 0)
                {
                    this.amount = value;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int amount = 0;

        public ICommand DeleteCommand { get; private set; }

        private InventoryListRequirementViewModel viewModel;

        public InventoryRequirementViewModel(InventoryListRequirementViewModel viewModel)
        {
            this.viewModel = viewModel;

            this.DeleteCommand = this.CreateCommand((parameter) =>
            {
                this.viewModel.Delete(this);
                return Task.FromResult(0);
            });
        }

        public InventoryRequirementViewModel(InventoryListRequirementViewModel viewModel, InventoryRequirementModel requirement)
            : this(viewModel)
        {
            this.SelectedInventory = requirement.Inventory;
            this.SelectedItem = requirement.Item;
            this.Amount = requirement.Amount;
        }

        public override async Task<bool> Validate()
        {
            if (this.SelectedInventory == null)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidInventoryMustBeSelected);
                return false;
            }

            if (this.SelectedItem == null)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidInventoryItemMustBeSelected);
                return false;
            }

            if (this.Amount <= 0)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidInventoryItemAmountMustSpecified);
                return false;
            }

            return true;
        }

        public override RequirementModelBase GetRequirement()
        {
            return new InventoryRequirementModel(this.SelectedInventory, this.SelectedItem, this.Amount);
        }
    }
}
