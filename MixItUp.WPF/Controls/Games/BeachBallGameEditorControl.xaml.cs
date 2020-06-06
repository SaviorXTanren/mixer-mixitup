using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for BeachBallGameEditorControl.xaml
    /// </summary>
    public partial class BeachBallGameEditorControl : GameEditorControlBase
    {
        private BeachBallGameEditorControlViewModel viewModel;
        private BeachBallGameCommand existingCommand;

        public BeachBallGameEditorControl(CurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new BeachBallGameEditorControlViewModel(currency);
        }

        public BeachBallGameEditorControl(BeachBallGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new BeachBallGameEditorControlViewModel(command);
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
                this.CommandDetailsControl.SetDefaultValues("Beach Ball", "beachball", CurrencyRequirementTypeEnum.NoCurrencyCost, 0, 0);
            }
            this.CommandDetailsControl.SetAsNoCostOnly();
            await base.OnLoaded();
        }
    }
}
