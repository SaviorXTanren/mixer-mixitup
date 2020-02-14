using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for CoinPusherGameEditorControl.xaml
    /// </summary>
    public partial class CoinPusherGameEditorControl : GameEditorControlBase
    {
        private CoinPusherGameEditorControlViewModel viewModel;
        private CoinPusherGameCommand existingCommand;

        public CoinPusherGameEditorControl(UserCurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new CoinPusherGameEditorControlViewModel(currency);
        }

        public CoinPusherGameEditorControl(CoinPusherGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new CoinPusherGameEditorControlViewModel(this.existingCommand);
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
                this.CommandDetailsControl.SetDefaultValues("Coin Pusher", "pusher", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
            }
            await base.OnLoaded();
        }
    }
}
