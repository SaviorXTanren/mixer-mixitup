using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for BidGameEditorControl.xaml
    /// </summary>
    public partial class BidGameEditorControl : GameEditorControlBase
    {
        private BidGameEditorControlViewModel viewModel;
        private BidGameCommand existingCommand;

        public BidGameEditorControl(UserCurrencyViewModel currency)
        {
            InitializeComponent();

            this.viewModel = new BidGameEditorControlViewModel(currency);
        }

        public BidGameEditorControl(BidGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new BidGameEditorControlViewModel(this.existingCommand);
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
            this.CommandDetailsControl.SetAsMinimumOnly();

            this.DataContext = this.viewModel;

            await this.viewModel.OnLoaded();

            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Bid", "bid", CurrencyRequirementTypeEnum.MinimumOnly, 10);
            }

            await base.OnLoaded();
        }
    }
}
