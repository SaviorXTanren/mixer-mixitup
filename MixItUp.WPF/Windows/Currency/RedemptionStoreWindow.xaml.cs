using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Window.Currency;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Windows.Commands;
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
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, MixItUp.Base.Resources.CustomProductRedemption);
            window.CommandSaved += Window_CommandSaved;
            window.Show();
        }

        private void CustomProductCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(((CommandListingButtonsControl)sender).GetCommandFromCommandButtons());
            window.Show();
        }

        private void CustomProductCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            CommandListingButtonsControl button = (CommandListingButtonsControl)sender;
            RedemptionStoreProductViewModel product = (RedemptionStoreProductViewModel)button.DataContext;
            product.Command = null;
        }

        private void ManualRedeemNeededCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(this.viewModel.ManualRedeemNeededCommand);
            window.Show();
        }

        private void DefaultRedeemCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(this.viewModel.DefaultRedemptionCommand);
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

        private void Window_CommandSaved(object sender, CommandModelBase command)
        {
            if (this.lastSelectedProductCommand != null)
            {
                this.lastSelectedProductCommand.Command = command;
            }
            this.lastSelectedProductCommand = null;
        }
    }
}
