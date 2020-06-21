using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Window.Currency;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for RedemptionStoreWindow.xaml
    /// </summary>
    public partial class RedemptionStoreWindow : LoadingWindowBase
    {
        private RedemptionStoreWindowViewModel viewModel;

        private RedemptionStoreProductViewModel lastSelectedProductCommand = null;

        public RedemptionStoreWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel = new RedemptionStoreWindowViewModel();
            await this.viewModel.OnLoaded();
            await base.OnLoaded();
        }

        private void CustomProductNewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.lastSelectedProductCommand = (RedemptionStoreProductViewModel)button.DataContext;
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(MixItUp.Base.Resources.CustomProductRedemption)));
            window.CommandSaveSuccessfully += CustomProductNewCommandWindow_CommandSaveSuccessfully;
            window.Show();
        }

        private void CustomProductCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl button = (CommandButtonsControl)sender;
            RedemptionStoreProductViewModel product = (RedemptionStoreProductViewModel)button.DataContext;
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(product.Command));
            window.Show();
        }

        private void CustomProductCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl button = (CommandButtonsControl)sender;
            RedemptionStoreProductViewModel product = (RedemptionStoreProductViewModel)button.DataContext;
            product.Command = null;
        }

        private void ManualRedeemNeededCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(this.viewModel.ManualRedeemNeededCommand));
            window.Show();
        }

        private void DefaultRedeemCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(this.viewModel.DefaultRedemptionCommand));
            window.Show();
        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (await this.viewModel.Validate())
            {
                await this.viewModel.Save();
                this.Close();
            }
        }

        private void CustomProductNewCommandWindow_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            if (this.lastSelectedProductCommand != null)
            {
                this.lastSelectedProductCommand.Command = (CustomCommand)e;
            }
            this.lastSelectedProductCommand = null;
        }
    }
}
