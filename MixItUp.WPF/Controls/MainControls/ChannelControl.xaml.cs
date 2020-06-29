using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChannelControl.xaml
    /// </summary>
    public partial class ChannelControl : MainControlBase
    {
        private ChannelMainControlViewModel viewModel;

        private bool shouldShowIntellisense = false;

        public ChannelControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new ChannelMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();

            this.GameNameComboBox.Text = this.viewModel.Game?.name;

            this.shouldShowIntellisense = true;
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private async void GameNameComboBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (this.shouldShowIntellisense)
            {
                string before = this.GameNameComboBox.Text;

                await Task.Delay(1000);

                if (string.Equals(before, this.GameNameComboBox.Text) && (this.viewModel.Game == null || !string.Equals(this.viewModel.Game.name, this.GameNameComboBox.Text)))
                {
                    if (await this.viewModel.SetSearchGamesForName(this.GameNameComboBox.Text))
                    {
                        this.GameNameComboBox.IsDropDownOpen = true;
                        return;
                    }
                    this.GameNameComboBox.IsDropDownOpen = false;
                }
            }
        }
    }
}
