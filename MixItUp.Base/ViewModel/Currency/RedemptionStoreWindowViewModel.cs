using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirements;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Currency
{
    public class RedemptionStoreProductViewModel : UIViewModelBase
    {
        public RedemptionStoreProductModel Product
        {
            get { return this.product; }
            set
            {
                this.product = value;
                this.NotifyPropertyChanged();
            }
        }
        private RedemptionStoreProductModel product;

        public string Name { get { return this.Product.Name; } }

        public string Quantity
        {
            get
            {
                if (this.Product.IsInfinite)
                {
                    return MixItUp.Base.Resources.Infinite;
                }
                else
                {
                    return this.Product.CurrentAmount.ToString();
                }
            }
        }

        public CommandModelBase Command
        {
            get { return this.Product.Command; }
            set
            {
                this.Product.Command = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CustomCommandNotSet");
                this.NotifyPropertyChanged("CustomCommandSet");
            }
        }

        public bool CustomCommandNotSet { get { return this.Command == null; } }

        public bool CustomCommandSet { get { return !this.CustomCommandNotSet; } }

        public ICommand EditProductCommand { get; private set; }

        public ICommand DeleteProductCommand { get; private set; }

        private RedemptionStoreWindowViewModel viewModel;

        public RedemptionStoreProductViewModel(RedemptionStoreWindowViewModel viewModel, RedemptionStoreProductModel product)
        {
            this.viewModel = viewModel;
            this.Product = product;

            this.EditProductCommand = this.CreateCommand(() =>
            {
                this.viewModel.EditProduct(this);
            });

            this.DeleteProductCommand = this.CreateCommand(async () =>
            {
                await this.viewModel.DeleteProduct(this);
            });
        }
    }

    public class RedemptionStoreWindowViewModel : UIViewModelBase
    {
        public ObservableCollection<RedemptionStoreProductViewModel> Products { get; set; } = new ObservableCollection<RedemptionStoreProductViewModel>();

        public string ProductName
        {
            get { return this.productName; }
            set
            {
                this.productName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string productName;

        public int ProductQuantity
        {
            get { return this.productQuantity; }
            set
            {
                this.productQuantity = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ProductQuantityString");
            }
        }
        private int productQuantity = -1;

        public string ProductQuantityString
        {
            get { return (this.ProductQuantity >= 0) ? this.ProductQuantity.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int iValue) && iValue >= 0)
                {
                    this.ProductQuantity = iValue;
                }
                else
                {
                    this.ProductQuantity = -1;
                }
            }
        }

        public bool ProductAutoReplenish
        {
            get { return this.productAutoReplenish; }
            set
            {
                this.productAutoReplenish = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool productAutoReplenish;

        public bool ProductAutoRedeem
        {
            get { return this.productAutoRedeem; }
            set
            {
                this.productAutoRedeem = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool productAutoRedeem = true;

        public RequirementsSetViewModel ProductRequirements
        {
            get { return this.productRequirements; }
            set
            {
                this.productRequirements = value;
                this.NotifyPropertyChanged();
            }
        }
        private RequirementsSetViewModel productRequirements = new RequirementsSetViewModel();

        public RedemptionStoreProductModel SelectedProduct
        {
            get { return this.selectedProduct; }
            set
            {
                this.selectedProduct = value;
                if (this.selectedProduct != null)
                {
                    this.ProductName = this.SelectedProduct.Name;
                    this.ProductQuantity = this.SelectedProduct.CurrentAmount;
                    this.ProductAutoReplenish = this.SelectedProduct.AutoReplenish;
                    this.ProductAutoRedeem = this.SelectedProduct.AutoRedeem;
                    this.ProductRequirements = new RequirementsSetViewModel(this.SelectedProduct.Requirements);
                }
                else
                {
                    this.ProductName = null;
                    this.ProductQuantity = -1;
                    this.ProductAutoReplenish = false;
                    this.ProductAutoRedeem = true;
                    this.ProductRequirements = new RequirementsSetViewModel();
                }
                this.NotifyPropertyChanged();
            }
        }
        private RedemptionStoreProductModel selectedProduct = null;

        public ICommand SaveProductCommand { get; private set; }

        public string ChatPurchaseCommand
        {
            get { return this.chatPurchaseCommand; }
            set
            {
                this.chatPurchaseCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private string chatPurchaseCommand;

        public string ModRedeemCommand
        {
            get { return this.modRedeemCommand; }
            set
            {
                this.modRedeemCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private string modRedeemCommand;

        public CustomCommandModel ManualRedeemNeededCommand
        {
            get { return this.manualRedeemNeededCommand; }
            set
            {
                this.manualRedeemNeededCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel manualRedeemNeededCommand;

        public CustomCommandModel DefaultRedemptionCommand
        {
            get { return this.defaultRedemptionCommand; }
            set
            {
                this.defaultRedemptionCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel defaultRedemptionCommand;

        public RedemptionStoreWindowViewModel()
        {
            this.Products.AddRange(ChannelSession.Settings.RedemptionStoreProducts.Values.ToList().OrderBy(p => p.Name).Select(p => new RedemptionStoreProductViewModel(this, p)));

            this.ChatPurchaseCommand = ChannelSession.Settings.RedemptionStoreChatPurchaseCommand;
            this.ModRedeemCommand = ChannelSession.Settings.RedemptionStoreModRedeemCommand;

            this.ManualRedeemNeededCommand = (CustomCommandModel)ChannelSession.Settings.GetCommand(ChannelSession.Settings.RedemptionStoreManualRedeemNeededCommandID);
            this.DefaultRedemptionCommand = (CustomCommandModel)ChannelSession.Settings.GetCommand(ChannelSession.Settings.RedemptionStoreDefaultRedemptionCommandID);

            this.SaveProductCommand = this.CreateCommand(async () =>
            {
                if (string.IsNullOrEmpty(this.ProductName))
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidProductName);
                    return;
                }

                RedemptionStoreProductViewModel duplicateProduct = this.Products.FirstOrDefault(p => p.Name.Equals(this.ProductName));
                if (duplicateProduct != null && duplicateProduct.Product != this.SelectedProduct)
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.ProductNameAlreadyExists);
                    return;
                }

                IEnumerable<Result> requirementResults = await this.ProductRequirements.Validate();
                if (requirementResults.Any(r => !r.Success))
                {
                    await DialogHelper.ShowMessage(string.Join(Environment.NewLine, requirementResults.Where(r => !r.Success).Select(r => r.Message)));
                }

                RedemptionStoreProductModel product = this.SelectedProduct;
                if (this.SelectedProduct == null)
                {
                    product = new RedemptionStoreProductModel();
                }

                product.Name = this.ProductName.Trim();
                product.MaxAmount = product.CurrentAmount = this.ProductQuantity;
                product.AutoReplenish = this.ProductAutoReplenish;
                product.AutoRedeem = this.ProductAutoRedeem;
                product.Requirements = this.ProductRequirements.GetRequirements();

                if (this.SelectedProduct == null)
                {
                    this.Products.Add(new RedemptionStoreProductViewModel(this, product));
                }

                this.SelectedProduct = null;

                this.RefreshProducts();
            });
        }

        public void EditProduct(RedemptionStoreProductViewModel product)
        {
            this.SelectedProduct = product.Product;
        }

        public async Task DeleteProduct(RedemptionStoreProductViewModel product)
        {
            if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ConfirmRedemptionStoreProductDeletion))
            {
                this.Products.Remove(product);
            }
            this.SelectedProduct = null;
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.ChatPurchaseCommand))
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidChatPurchaseCommand);
                return false;
            }

            if (string.IsNullOrEmpty(this.ModRedeemCommand))
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidModRedeemCommand);
                return false;
            }

            return true;
        }

        public async Task Save()
        {
            ChannelSession.Settings.RedemptionStoreProducts.Clear();
            foreach (RedemptionStoreProductViewModel product in this.Products)
            {
                ChannelSession.Settings.RedemptionStoreProducts[product.Product.ID] = product.Product;
            }

            ChannelSession.Settings.RedemptionStoreChatPurchaseCommand = this.ChatPurchaseCommand;
            ChannelSession.Settings.RedemptionStoreModRedeemCommand = this.ModRedeemCommand;

            ChannelSession.Settings.SetCommand(this.ManualRedeemNeededCommand);
            ChannelSession.Settings.SetCommand(this.DefaultRedemptionCommand);

            await ChannelSession.SaveSettings();
        }

        private void RefreshProducts()
        {
            List<RedemptionStoreProductViewModel> products = this.Products.ToList();
            this.Products.ClearAndAddRange(products.OrderBy(p => p.Name));
        }
    }
}
