using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class CrowdControlEffectCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public ObservableCollection<CrowdControlGame> Games { get; private set; } = new ObservableCollection<CrowdControlGame>();

        public CrowdControlGame SelectedGame
        {
            get { return this.selectedGame; }
            set
            {
                this.selectedGame = value;
                this.NotifyPropertyChanged();

                if (this.SelectedGame != null)
                {
                    AsyncRunner.RunAsyncBackground(async (token) =>
                    {
                        this.Packs.ClearAndAddRange(await ServiceManager.Get<CrowdControlService>().GetGamePacks(this.SelectedGame));
                        if (this.existingPackID != null)
                        {
                            this.SelectedPack = this.Packs.FirstOrDefault(p => string.Equals(p.gamePackID, this.existingPackID));
                            this.existingPackID = null;
                        }

                        if (this.SelectedPack == null)
                        {
                            this.SelectedPack = this.Packs.FirstOrDefault();
                        }
                    }, CancellationToken.None);
                }
            }
        }
        private CrowdControlGame selectedGame;

        public ThreadSafeObservableCollection<CrowdControlGamePack> Packs { get; private set; } = new ThreadSafeObservableCollection<CrowdControlGamePack>();

        public CrowdControlGamePack SelectedPack
        {
            get { return this.selectedPack; }
            set
            {
                this.selectedPack = value;
                this.NotifyPropertyChanged();

                if (this.SelectedPack != null)
                {
                    this.Effects.ClearAndAddRange(this.SelectedPack.GameEffects);
                    if (this.existingEffectID != null)
                    {
                        this.SelectedEffect = this.Effects.FirstOrDefault(e => string.Equals(e.id, this.existingEffectID));
                        this.existingEffectID = null;
                    }
                }
            }
        }
        private CrowdControlGamePack selectedPack;

        public ThreadSafeObservableCollection<CrowdControlGamePackEffect> Effects { get; private set; } = new ThreadSafeObservableCollection<CrowdControlGamePackEffect>();

        public CrowdControlGamePackEffect SelectedEffect
        {
            get { return this.selectedEffect; }
            set
            {
                this.selectedEffect = value;
                this.NotifyPropertyChanged();
            }
        }
        private CrowdControlGamePackEffect selectedEffect;

        private string existingGameID;
        private string existingPackID;
        private string existingEffectID;

        public CrowdControlEffectCommandEditorWindowViewModel(CrowdControlEffectCommandModel existingCommand)
            : base(existingCommand)
        {
            this.existingGameID = existingCommand.GameID;
            this.existingPackID = existingCommand.PackID;
            this.existingEffectID = existingCommand.EffectID;
        }

        public CrowdControlEffectCommandEditorWindowViewModel() : base(CommandTypeEnum.CrowdControlEffect) { }

        public override Task<Result> Validate()
        {
            if (this.SelectedGame == null && this.selectedEffect == null)
            {
                return Task.FromResult(new Result(Resources.CrowdControlCommandValidGameAndEffectMustBeSelected));
            }
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new CrowdControlEffectCommandModel(this.SelectedGame, this.SelectedPack, this.SelectedEffect));
        }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return CrowdControlEffectCommandModel.GetEffectTestSpecialIdentifiers(); }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            ((CrowdControlEffectCommandModel)command).GameID = this.SelectedGame.gameID;
            ((CrowdControlEffectCommandModel)command).GameName = this.SelectedGame.Name;
            ((CrowdControlEffectCommandModel)command).PackID = this.SelectedPack.Name;
            ((CrowdControlEffectCommandModel)command).PackName = this.SelectedPack.gamePackID;
            ((CrowdControlEffectCommandModel)command).EffectID = this.SelectedEffect.id;
            ((CrowdControlEffectCommandModel)command).EffectName = this.SelectedEffect.Name;
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ServiceManager.Get<CommandService>().CrowdControlEffectCommands.Remove((CrowdControlEffectCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().CrowdControlEffectCommands.Add((CrowdControlEffectCommandModel)command);
            return Task.CompletedTask;
        }

        protected override async Task OnOpenInternal()
        {
            if (ServiceManager.Get<CrowdControlService>().IsConnected)
            {
                this.Games.AddRange(ServiceManager.Get<CrowdControlService>().GetGames());

                if (this.existingGameID != null)
                {
                    this.SelectedGame = this.Games.FirstOrDefault(g => string.Equals(g.gameID, this.existingGameID));
                }
            }
            await base.OnOpenInternal();
        }
    }
}