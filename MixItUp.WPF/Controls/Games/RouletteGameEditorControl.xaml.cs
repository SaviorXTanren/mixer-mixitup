using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for RouletteGameEditorControl.xaml
    /// </summary>
    public partial class RouletteGameEditorControl : GameEditorControlBase
    {
        private RouletteGameEditorControlViewModel viewModel;
        private RouletteGameCommand existingCommand;

        public RouletteGameEditorControl(CurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new RouletteGameEditorControlViewModel(currency);
        }

        public RouletteGameEditorControl(RouletteGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new RouletteGameEditorControlViewModel(command);
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
                this.CommandDetailsControl.SetDefaultValues("Roulette", "roulette", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
            }
            await base.OnLoaded();
        }
    }
}
