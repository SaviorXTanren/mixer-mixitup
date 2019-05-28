using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for LockBoxGameEditorControl.xaml
    /// </summary>
    public partial class LockBoxGameEditorControl : GameEditorControlBase
    {
        private LockBoxGameEditorControlViewModel viewModel;
        private LockBoxGameCommand existingCommand;

        public LockBoxGameEditorControl(UserCurrencyViewModel currency)
        {
            InitializeComponent();

            this.viewModel = new LockBoxGameEditorControlViewModel(currency);
        }

        public LockBoxGameEditorControl(LockBoxGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new LockBoxGameEditorControlViewModel(command);
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
                this.CommandDetailsControl.SetDefaultValues("Lock Box", "lockbox", CurrencyRequirementTypeEnum.RequiredAmount, 10);
            }
            await base.OnLoaded();
        }
    }
}
