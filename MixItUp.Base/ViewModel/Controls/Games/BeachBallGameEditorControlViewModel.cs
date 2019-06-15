using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class BeachBallGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string HitTimeLimitString
        {
            get { return this.HitTimeLimit.ToString(); }
            set
            {
                this.HitTimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int HitTimeLimit { get; set; } = 10;

        public bool AllowUserTargeting
        {
            get { return this.allowUserTargeting; }
            set
            {
                this.allowUserTargeting = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool allowUserTargeting { get; set; } = false;

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand BallHitCommand { get; set; }

        public CustomCommand BallMissedCommand { get; set; }

        public BeachBallGameCommand existingCommand;

        public BeachBallGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of beach ball and hit the ball to @$targetusername. Quick, type !beachball to hit it to someone else!");
            this.BallHitCommand = this.CreateBasicChatCommand("@$username has hit the ball to @$targetusername. Quick, type !beachball to hit it to someone else!");
            this.BallMissedCommand = this.CreateBasicChatCommand("@$targetusername missed the beach ball! @$username gains 100 " + currency.Name + "!");
            this.BallMissedCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, "100"));
        }

        public BeachBallGameEditorControlViewModel(BeachBallGameCommand command)
        {
            this.existingCommand = command;

            this.HitTimeLimit = this.existingCommand.HitTimeLimit;
            this.AllowUserTargeting = this.existingCommand.AllowUserTargeting;

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.BallHitCommand = this.existingCommand.BallHitCommand;
            this.BallMissedCommand = this.existingCommand.BallMissedCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new BeachBallGameCommand(name, triggers, requirements, this.HitTimeLimit, this.AllowUserTargeting, this.StartedCommand, this.BallHitCommand,
                this.BallMissedCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.HitTimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Hit Time Limit is not a valid number greater than 0");
                return false;
            }
            return true;
        }
    }
}
