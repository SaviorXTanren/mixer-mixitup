using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.ViewModels;
using MixItUp.Base.Util;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Dialogs
{
    public class GameTypeSelectorDialogControlViewModel : UIViewModelBase
    {
        public IEnumerable<GameCommandTypeEnum> GameTypes { get { return EnumHelper.GetEnumList<GameCommandTypeEnum>(); } }

        public GameCommandTypeEnum SelectedGameType
        {
            get { return this.selectedGameType; }
            set
            {
                this.selectedGameType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("GameTypeDescription");
            }
        }
        private GameCommandTypeEnum selectedGameType;

        public string GameTypeDescription
        {
            get
            {
                if (this.SelectedGameType == GameCommandTypeEnum.Bet) { return MixItUp.Base.Resources.GameCommandBetDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Bid) { return MixItUp.Base.Resources.GameCommandBidDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.CoinPusher) { return MixItUp.Base.Resources.GameCommandCoinPusherDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Duel) { return MixItUp.Base.Resources.GameCommandDuelDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Hangman) { return MixItUp.Base.Resources.GameCommandHangmanDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Heist) { return MixItUp.Base.Resources.GameCommandHeistDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Hitman) { return MixItUp.Base.Resources.GameCommandHitmanDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.HotPotato) { return MixItUp.Base.Resources.GameCommandHotPotatoDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.LockBox) { return MixItUp.Base.Resources.GameCommandLockBoxDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Roulette) { return MixItUp.Base.Resources.GameCommandRouletteDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.RussianRoulette) { return MixItUp.Base.Resources.GameCommandRussianRouletteDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.SlotMachine) { return MixItUp.Base.Resources.GameCommandSlotMachineDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Spin) { return MixItUp.Base.Resources.GameCommandSpinDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Steal) { return MixItUp.Base.Resources.GameCommandStealDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.TreasureDefense) { return MixItUp.Base.Resources.GameCommandTreasureDefenseDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Trivia) { return MixItUp.Base.Resources.GameCommandTriviaDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.Volcano) { return MixItUp.Base.Resources.GameCommandVolcanoDescription; }
                else if (this.SelectedGameType == GameCommandTypeEnum.WordScramble) { return MixItUp.Base.Resources.GameCommandWordScrambleDescription; }
                else { return string.Empty; }
            }
        }

        public GameTypeSelectorDialogControlViewModel() { }
    }
}
