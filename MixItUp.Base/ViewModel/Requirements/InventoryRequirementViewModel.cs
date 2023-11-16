using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            this.AddItemCommand = this.CreateCommand(() =>
            {
                this.Items.Add(new InventoryRequirementViewModel(this));
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
        public IEnumerable<InventoryModel> Inventories { get { return ChannelSession.Settings.Inventory.Values.ToList(); } }

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
                this.amount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int amount = 0;

        public ICommand DeleteCommand { get; private set; }

        private InventoryListRequirementViewModel viewModel;

        public InventoryRequirementViewModel(InventoryListRequirementViewModel viewModel)
        {
            this.viewModel = viewModel;

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.viewModel.Delete(this);
            });
        }

        public InventoryRequirementViewModel(InventoryListRequirementViewModel viewModel, InventoryRequirementModel requirement)
            : this(viewModel)
        {
            this.SelectedInventory = requirement.Inventory;
            this.SelectedItem = requirement.Item;
            this.Amount = requirement.Amount;
        }

        public override Task<Result> Validate()
        {
            if (this.SelectedInventory == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidInventoryMustBeSelected));
            }

            if (this.SelectedItem == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidInventoryItemMustBeSelected));
            }

            if (this.Amount < 0)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidInventoryItemAmountMustSpecified));
            }

            return Task.FromResult(new Result());
        }

        public override RequirementModelBase GetRequirement()
        {
            if (this.SelectedInventory != null && this.SelectedItem != null)
            {
                return new InventoryRequirementModel(this.SelectedInventory, this.SelectedItem, this.Amount);
            }
            return null;
        }
    }
}
