using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Controls.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for HangmanGameEditorControl.xaml
    /// </summary>
    public partial class HangmanGameEditorControl : GameEditorControlBase
    {
        private HangmanGameEditorControlViewModel viewModel;
        private HangmanGameCommand existingCommand;

        public HangmanGameEditorControl(UserCurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new HangmanGameEditorControlViewModel(currency);
        }

        public HangmanGameEditorControl(HangmanGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new HangmanGameEditorControlViewModel(command);
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
                this.CommandDetailsControl.SetDefaultValues("Hangman", "hangman", CurrencyRequirementTypeEnum.RequiredAmount, 10);
            }
            await base.OnLoaded();
        }
    }
}
