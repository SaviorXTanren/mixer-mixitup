using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class BeachBallGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string LowerTimeLimitString
        {
            get { return this.LowerTimeLimit.ToString(); }
            set
            {
                this.LowerTimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int LowerTimeLimit { get; set; } = 10;

        public string UpperTimeLimitString
        {
            get { return this.UpperTimeLimit.ToString(); }
            set
            {
                this.UpperTimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int UpperTimeLimit { get; set; } = 10;

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

        public BeachBallGameEditorControlViewModel(UserCurrencyModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of beach ball and hit the ball to @$targetusername. Quick, type !beachball to hit it to someone else!");
            this.BallHitCommand = this.CreateBasicChatCommand("@$username has hit the ball to @$targetusername. Quick, type !beachball to hit it to someone else!");
            this.BallMissedCommand = this.CreateBasicChatCommand("@$targetusername missed the beach ball! @$username gains 100 " + currency.Name + "!");
            this.BallMissedCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, "100"));
        }

        public BeachBallGameEditorControlViewModel(BeachBallGameCommand command)
        {
            this.existingCommand = command;

#pragma warning disable CS0612 // Type or member is obsolete
            if (this.existingCommand.HitTimeLimit > 0)
            {
                this.existingCommand.LowerLimit = this.existingCommand.HitTimeLimit;
                this.existingCommand.UpperLimit = this.existingCommand.HitTimeLimit;
                this.existingCommand.HitTimeLimit = 0;
            }
#pragma warning restore CS0612 // Type or member is obsolete

            this.LowerTimeLimit = this.existingCommand.LowerLimit;
            this.UpperTimeLimit = this.existingCommand.UpperLimit;
            this.AllowUserTargeting = this.existingCommand.AllowUserTargeting;

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.BallHitCommand = this.existingCommand.BallHitCommand;
            this.BallMissedCommand = this.existingCommand.BallMissedCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new BeachBallGameCommand(name, triggers, requirements, this.LowerTimeLimit, this.UpperTimeLimit, this.AllowUserTargeting, this.StartedCommand, this.BallHitCommand,
                this.BallMissedCommand);
            this.SaveGameCommand(newCommand, this.existingCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.LowerTimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Lower Time Limit is not a valid number greater than 0");
                return false;
            }

            if (this.UpperTimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Upper Time Limit is not a valid number greater than 0");
                return false;
            }
            return true;
        }
    }
}
