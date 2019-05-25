using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class RouletteGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string MinimumParticipantsString
        {
            get { return this.MinimumParticipants.ToString(); }
            set
            {
                this.MinimumParticipants = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int MinimumParticipants { get; set; } = 2;

        public string TimeLimitString
        {
            get { return this.TimeLimit.ToString(); }
            set
            {
                this.TimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int TimeLimit { get; set; } = 30;

        public string UserPayoutString
        {
            get { return this.UserPayout.ToString(); }
            set
            {
                this.UserPayout = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double UserPayout { get; set; } = 200;

        public string SubscriberPayoutString
        {
            get { return this.SubscriberPayout.ToString(); }
            set
            {
                this.SubscriberPayout = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double SubscriberPayout { get; set; } = 200;

        public string ModPayoutString
        {
            get { return this.ModPayout.ToString(); }
            set
            {
                this.ModPayout = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double ModPayout { get; set; } = 200;

        public bool IsNumberRange
        {
            get { return this.isNumberRange; }
            set
            {
                this.isNumberRange = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsTextOptions");
            }
        }
        private bool isNumberRange { get; set; } = true;

        public string NumberRangeMinimumString
        {
            get { return this.NumberRangeMinimum.ToString(); }
            set
            {
                this.NumberRangeMinimum = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int NumberRangeMinimum { get; set; } = 1;

        public string NumberRangeMaximumString
        {
            get { return this.NumberRangeMaximum.ToString(); }
            set
            {
                this.NumberRangeMaximum = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int NumberRangeMaximum { get; set; } = 30;

        public bool IsTextOptions { get { return !this.IsNumberRange; } }

        public string SelectableBetTypes
        {
            get { return this.selectableBetTypes; }
            set
            {
                this.selectableBetTypes = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectableBetTypes { get; set; }

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand UserJoinCommand { get; set; }

        public CustomCommand UserSuccessCommand { get; set; }
        public CustomCommand UserFailCommand { get; set; }

        public CustomCommand GameCompleteCommand { get; set; }

        private RouletteGameCommand existingCommand;

        public RouletteGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.StartedCommand = this.CreateBasic2ChatCommand("@$username has started a game of roulette! Type !roulette <BET TYPE> <AMOUNT> in chat to play!", "Valid Bet Types: $gamevalidbettypes");
            this.UserJoinCommand = this.CreateBasicChatCommand("You slap your chips on the number $gamebettype as the ball starts to spin around the roulette wheel!", whisper: true);
            this.UserSuccessCommand = this.CreateBasicChatCommand("Congrats, you made out with $gamepayout " + currency.Name + "!", whisper: true);
            this.UserFailCommand = this.CreateBasicChatCommand("Lady luck wasn't with you today, better luck next time...", whisper: true);
            this.GameCompleteCommand = this.CreateBasicChatCommand("The wheel slows down, revealing $gamewinningbettype as the winning bet! Total Payout: $gameallpayout");
        }

        public RouletteGameEditorControlViewModel(RouletteGameCommand command)
        {
            this.existingCommand = command;

            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;
            this.UserPayout = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.User] * 100);
            this.SubscriberPayout = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.Subscriber] * 100);
            this.ModPayout = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.Mod] * 100);
            this.IsNumberRange = this.existingCommand.IsNumberRange;
            if (this.existingCommand.IsNumberRange)
            {
                IEnumerable<int> numberBetTypes = this.existingCommand.ValidBetTypes.Select(b => int.Parse(b));
                this.NumberRangeMinimum = numberBetTypes.Min();
                this.NumberRangeMaximum = numberBetTypes.Max();
            }
            else
            {
                this.SelectableBetTypes = string.Join(Environment.NewLine, this.existingCommand.ValidBetTypes);
            }

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.UserJoinCommand = this.existingCommand.UserJoinCommand;
            this.UserSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
            this.UserFailCommand = this.existingCommand.UserFailOutcome.Command;
            this.GameCompleteCommand = this.existingCommand.GameCompleteCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            HashSet<string> validBetTypes = new HashSet<string>();
            if (this.IsNumberRange)
            {
                for (int i = this.NumberRangeMinimum; i <= this.NumberRangeMaximum; i++)
                {
                    validBetTypes.Add(i.ToString());
                }
            }
            else
            {
                foreach (string betType in this.SelectableBetTypes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    validBetTypes.Add(betType.ToLower());
                }
            }

            this.UserPayout = this.UserPayout / 100.0;
            this.SubscriberPayout = this.SubscriberPayout / 100.0;
            this.ModPayout = this.ModPayout / 100.0;

            Dictionary<MixerRoleEnum, double> successRolePayouts = new Dictionary<MixerRoleEnum, double>() { { MixerRoleEnum.User, this.UserPayout }, { MixerRoleEnum.Subscriber, this.SubscriberPayout }, { MixerRoleEnum.Mod, this.ModPayout = this.ModPayout } };
            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new RouletteGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, this.IsNumberRange, validBetTypes,
                this.StartedCommand, this.UserJoinCommand, new GameOutcome("Success", successRolePayouts, roleProbabilities, this.UserSuccessCommand),
                new GameOutcome("Failure", 0, roleProbabilities, this.UserFailCommand), this.GameCompleteCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.TimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (this.MinimumParticipants <= 0)
            {
                await DialogHelper.ShowMessage("The Minimum Participants is not a valid number greater than 0");
                return false;
            }

            if (this.UserPayout < 0)
            {
                await DialogHelper.ShowMessage("The User Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (this.SubscriberPayout < 0)
            {
                await DialogHelper.ShowMessage("The Subscriber Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (this.ModPayout < 0)
            {
                await DialogHelper.ShowMessage("The Mod Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (this.IsNumberRange)
            {
                if (this.NumberRangeMinimum <= 0)
                {
                    await DialogHelper.ShowMessage("The Min Number is not a valid number greater than 0");
                    return false;
                }

                if (this.NumberRangeMaximum <= 0)
                {
                    await DialogHelper.ShowMessage("The Max Number is not a valid number greater than 0");
                    return false;
                }

                if (this.NumberRangeMaximum < this.NumberRangeMinimum)
                {
                    await DialogHelper.ShowMessage("The Max Number can not be less than the Min Number");
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(this.SelectableBetTypes))
                {
                    await DialogHelper.ShowMessage("The Valid Bet Types does not have a value");
                    return false;
                }

                HashSet<string> validBetTypes = new HashSet<string>();
                foreach (string betType in this.SelectableBetTypes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    validBetTypes.Add(betType.ToLower());
                }

                if (validBetTypes.Count() < 2)
                {
                    await DialogHelper.ShowMessage("You must specify at least 2 different bet types");
                    return false;
                }
            }

            return true;
        }
    }
}
