using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for SpinGameEditorControl.xaml
    /// </summary>
    public partial class SpinGameEditorControl : GameEditorControlBase
    {
        private SpinGameControlViewModel viewModel;
        private SpinGameCommand existingCommand;

        public SpinGameEditorControl(UserCurrencyViewModel currency)
        {
            InitializeComponent();

            this.viewModel = new SpinGameControlViewModel(currency);
        }

        public SpinGameEditorControl(SpinGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new SpinGameControlViewModel(command);
        }

        public override async Task<bool> Validate()
        {
            if (!await this.CommandDetailsControl.Validate())
            {
                return false;
            }
            return await this.viewModel.Validate();
        }

        public override void SaveGameCommand()
        {
            this.viewModel.SaveGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers, this.CommandDetailsControl.GetRequirements());
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();

            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Spin", "spin", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
            }

            await base.OnLoaded();
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;

        }
    }
}
