using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class CurrencyRankInventoryContainerViewModel
    {
        public CurrencyModel Currency { get; private set; }
        public InventoryModel Inventory { get; private set; }

        public CurrencyRankInventoryContainerViewModel(CurrencyModel currency) { this.Currency = currency; }

        public CurrencyRankInventoryContainerViewModel(InventoryModel inventory) { this.Inventory = inventory; }

        public string Name
        {
            get
            {
                if (this.Inventory != null) { return this.Inventory.Name; }
                else { return this.Currency.Name; }
            }
        }

        public string Type
        {
            get
            {
                if (this.Inventory != null) { return Resources.Inventory; }
                else if (this.Currency.IsRank) { return Resources.Rank; }
                else { return Resources.Currency; }
            }
        }

        public string AmountSpecialIdentifiers
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (this.Inventory != null)
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Inventory.UserAmountSpecialIdentifierExample);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Inventory.UserAmountSpecialIdentifierExample);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Inventory.UserAllAmountSpecialIdentifier);
                }
                else
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.UserAmountSpecialIdentifier);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Currency.UserAmountSpecialIdentifier);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.Top10SpecialIdentifier);
                }
                return stringBuilder.ToString().Trim(new char[] { '\r', '\n' });
            }
            set { }
        }

        public string RankSpecialIdentifiers
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (this.Currency != null && this.Currency.IsRank)
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.UserRankNameSpecialIdentifier);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Currency.UserRankNameSpecialIdentifier);
                }
                return stringBuilder.ToString().Trim(new char[] { '\r', '\n' });
            }
            set { }
        }
    }

    public class CurrencyRankInventoryMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<CurrencyRankInventoryContainerViewModel> Items { get; set; } = new ObservableCollection<CurrencyRankInventoryContainerViewModel>();

        public CurrencyRankInventoryMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void RefreshList()
        {
            this.Items.Clear();
            foreach (var kvp in ChannelSession.Settings.Currency)
            {
                if (kvp.Value.IsRank)
                {
                    this.Items.Add(new CurrencyRankInventoryContainerViewModel(kvp.Value));
                }
                else
                {
                    this.Items.Add(new CurrencyRankInventoryContainerViewModel(kvp.Value));
                }
            }
            foreach (var kvp in ChannelSession.Settings.Inventory)
            {
                this.Items.Add(new CurrencyRankInventoryContainerViewModel(kvp.Value));
            }
        }

        public async void DeleteItem(CurrencyRankInventoryContainerViewModel item)
        {
            if (await DialogHelper.ShowConfirmation("Are you sure you wish to delete this?"))
            {
                if (item.Inventory != null)
                {
                    await item.Inventory.Reset();
                    ChannelSession.Settings.Inventory.Remove(item.Inventory.ID);
                }
                else
                {
                    await item.Currency.Reset();
                    ChannelSession.Settings.Currency.Remove(item.Currency.ID);
                }
                this.RefreshList();
            }
        }

        protected override async Task OnLoadedInternal()
        {
            this.RefreshList();
            await base.OnLoadedInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            this.RefreshList();
            await base.OnVisibleInternal();
        }
    }
}
