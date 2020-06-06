using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for BetGameEditorControl.xaml
    /// </summary>
    public partial class BetGameEditorControl : GameEditorControlBase
    {
        private BetGameEditorControlViewModel viewModel;
        private BetGameCommand existingCommand;

        public BetGameEditorControl(CurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new BetGameEditorControlViewModel(currency);
        }

        public BetGameEditorControl(BetGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new BetGameEditorControlViewModel(this.existingCommand);
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
                this.CommandDetailsControl.SetDefaultValues("Bet", "bet", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
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
