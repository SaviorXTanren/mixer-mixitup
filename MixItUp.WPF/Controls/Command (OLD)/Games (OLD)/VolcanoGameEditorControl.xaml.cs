﻿using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for VolcanoGameEditorControl.xaml
    /// </summary>
    public partial class VolcanoGameEditorControl : GameEditorControlBase
    {
        private VolcanoGameCommandEditorWindowViewModel viewModel;
        private VolcanoGameCommand existingCommand;

        public VolcanoGameEditorControl(CurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new VolcanoGameCommandEditorWindowViewModel(currency);
        }

        public VolcanoGameEditorControl(VolcanoGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new VolcanoGameCommandEditorWindowViewModel(command);
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
                this.CommandDetailsControl.SetDefaultValues("Volcano", "volcano", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
            }

            await base.OnLoaded();
        }
    }
}