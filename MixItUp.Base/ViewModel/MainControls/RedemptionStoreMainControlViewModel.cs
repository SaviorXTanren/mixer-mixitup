using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class RedemptionStorePurchaseViewModel : ViewModels.UIViewModelBase
    {
        public RedemptionStorePurchaseModel Purchase { get; set; }

        public string Name
        {
            get
            {
                RedemptionStoreProductModel product = this.Purchase.Product;
                if (product != null)
                {
                    return product.Name;
                }
                return "Unknown";
            }
        }

        public string Username
        {
            get
            {
                UserV2ViewModel user = this.Purchase.User;
                if (user != null)
                {
                    return user.FullDisplayName;
                }
                return "Unknown";
            }
        }

        public string PurchaseDateTimeString { get { return this.Purchase.PurchaseDate.ToFriendlyDateTimeString(); } }

        public bool ManualRedeemNeeded { get { return this.Purchase.State == RedemptionStorePurchaseRedemptionState.ManualRedeemNeeded; } }

        public string StateString { get { return (this.ManualRedeemNeeded) ? MixItUp.Base.Resources.Pending : MixItUp.Base.Resources.Redeemed; } }

        public ICommand ManualRedeemCommand { get; private set; }
        public bool CanManualRedeem { get { return this.ManualRedeemNeeded; } }

        public ICommand RefundCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }

        private RedemptionStoreMainControlViewModel viewModel;

        public RedemptionStorePurchaseViewModel(RedemptionStoreMainControlViewModel viewModel, RedemptionStorePurchaseModel purchase)
        {
            this.viewModel = viewModel;
            this.Purchase = purchase;

            this.ManualRedeemCommand = this.CreateCommand(async () =>
            {
                await this.Purchase.Redeem();
                this.viewModel.Refresh();
            });

            this.RefundCommand = this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ConfirmRefundRedemptionStorePurchase))
                {
                    await this.Purchase.Refund();
                    this.viewModel.Refresh();
                }
            });

            this.DeleteCommand = this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ConfirmDeleteRedemptionStorePurchase))
                {
                    this.Purchase.Remove();
                    this.viewModel.Refresh();
                }
            });
        }
    }

    public class RedemptionStoreMainControlViewModel : WindowControlViewModelBase
    {
        public ThreadSafeObservableCollection<RedemptionStorePurchaseViewModel> Purchases { get; private set; } = new ThreadSafeObservableCollection<RedemptionStorePurchaseViewModel>();

        public bool EnableRedemptionStore
        {
            get { return ChannelSession.Settings.RedemptionStoreEnabled; }
            set
            {
                ChannelSession.Settings.RedemptionStoreEnabled = value;
                this.NotifyPropertyChanged();
            }
        }

        public RedemptionStoreMainControlViewModel(UIViewModelBase windowViewModel) : base(windowViewModel)
        {
            RedemptionStorePurchaseModel.OnRedemptionStorePurchasesUpdated += RedemptionStorePurchaseModel_OnRedemptionStorePurchasesUpdated;
        }

        public void Refresh()
        {
            List<RedemptionStorePurchaseViewModel> purchases = new List<RedemptionStorePurchaseViewModel>(ChannelSession.Settings.RedemptionStorePurchases.ToList().Select(p => new RedemptionStorePurchaseViewModel(this, p)));
            this.Purchases.ClearAndAddRange(purchases.OrderByDescending(p => p.ManualRedeemNeeded).ThenBy(p => p.Purchase.PurchaseDate));
        }

        protected override async Task OnOpenInternal()
        {
            this.Refresh();
            await base.OnVisibleInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            this.Refresh();
            await base.OnVisibleInternal();
        }

        private void RedemptionStorePurchaseModel_OnRedemptionStorePurchasesUpdated(object sender, System.EventArgs e) { this.Refresh(); }
    }
}
