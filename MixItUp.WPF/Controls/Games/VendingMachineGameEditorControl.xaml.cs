using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for VendingMachineGameEditorControl.xaml
    /// </summary>
    public partial class VendingMachineGameEditorControl : GameEditorControlBase
    {
        private VendingMachineGameEditorControlViewModel viewModel;
        private VendingMachineGameCommand existingCommand;

        public VendingMachineGameEditorControl(UserCurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new VendingMachineGameEditorControlViewModel(currency);
        }

        public VendingMachineGameEditorControl(VendingMachineGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new VendingMachineGameEditorControlViewModel(command);
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
                this.CommandDetailsControl.SetDefaultValues("Vending Machine", "vend", CurrencyRequirementTypeEnum.RequiredAmount, 10);
            }

            await base.OnLoaded();
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.viewModel.DeleteOutcomeCommand.Execute(button.DataContext);
        }
    }
}
