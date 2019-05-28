using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for HeistGameEditorControl.xaml
    /// </summary>
    public partial class HeistGameEditorControl : GameEditorControlBase
    {
        private HeistGameEditorControlViewModel viewModel;
        private HeistGameCommand existingCommand;

        public HeistGameEditorControl(UserCurrencyViewModel currency)
        {
            InitializeComponent();

            this.viewModel = new HeistGameEditorControlViewModel(currency);
        }

        public HeistGameEditorControl(HeistGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new HeistGameEditorControlViewModel(command);
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
                this.CommandDetailsControl.SetDefaultValues("Heist", "heist", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
            }
            await base.OnLoaded();
        }
    }
}
