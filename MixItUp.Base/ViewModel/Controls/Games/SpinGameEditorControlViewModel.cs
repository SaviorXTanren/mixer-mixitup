using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class SpinOutcome : UIViewModelBase
    {
        public string Name { get; set; }

        public CustomCommand Command { get; set; }

        public int Payout { get; set; }

        public int UserChance { get; set; }
        public int SubscriberChance { get; set; }
        public int ModChance { get; set; }

        public SpinOutcome(string name, CustomCommand command, int payout = 0, int userChance = 0, int subscriberChance = 0, int modChance = 0)
        {
            this.Name = name;
            this.Command = command;
            this.Payout = payout;
            this.UserChance = userChance;
            this.SubscriberChance = subscriberChance;
            this.ModChance = modChance;
        }

        public SpinOutcome(GameOutcome outcome) : this(outcome.Name, outcome.Command)
        {
            this.Payout = Convert.ToInt32(outcome.Payout * 100.0);
            this.UserChance = outcome.RoleProbabilities[MixerRoleEnum.User];
            this.SubscriberChance = outcome.RoleProbabilities[MixerRoleEnum.Subscriber];
            this.ModChance = outcome.RoleProbabilities[MixerRoleEnum.Mod];
        }

        public string PayoutString
        {
            get { return this.Payout.ToString(); }
            set { this.Payout = this.GetPositiveIntFromString(value); }
        }

        public string UserChanceString
        {
            get { return this.UserChance.ToString(); }
            set { this.UserChance = this.GetPositiveIntFromString(value); }
        }

        public string SubscriberChanceString
        {
            get { return this.SubscriberChance.ToString(); }
            set { this.SubscriberChance = this.GetPositiveIntFromString(value); }
        }

        public string ModChanceString
        {
            get { return this.ModChance.ToString(); }
            set { this.ModChance = this.GetPositiveIntFromString(value); }
        }

        public GameOutcome GetGameOutcome()
        {
            return new GameOutcome(this.Name, Convert.ToDouble(this.Payout) / 100.0,
                new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, this.UserChance }, { MixerRoleEnum.Subscriber, this.SubscriberChance }, { MixerRoleEnum.Mod, this.ModChance } },
                this.Command);
        }
    }

    public class SpinGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public ObservableCollection<SpinOutcome> Outcomes { get; set; } = new ObservableCollection<SpinOutcome>();

        public ICommand AddOutcomeCommand { get; set; }
        public ICommand DeleteOutcomeCommand { get; set; }

        private SpinGameCommand existingCommand;

        public SpinGameEditorControlViewModel(UserCurrencyModel currency)
            : this()
        {
            this.Outcomes.Add(new SpinOutcome("Lose", this.CreateBasicChatCommand("Sorry @$username, you lost the spin!"), 0, 70, 70, 70));
            this.Outcomes.Add(new SpinOutcome("Win", this.CreateBasicChatCommand("Congrats @$username, you won $gamepayout " + currency.Name + "!"), 200, 30, 30, 30));
        }

        public SpinGameEditorControlViewModel(SpinGameCommand command)
            : this()
        {
            this.existingCommand = command;
            foreach (GameOutcome outcome in this.existingCommand.Outcomes)
            {
                this.Outcomes.Add(new SpinOutcome(outcome));
            }
        }

        private SpinGameEditorControlViewModel()
        {
            this.AddOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Outcomes.Add(new SpinOutcome("", this.CreateBasicChatCommand("@$username")));
                return Task.FromResult(0);
            });

            this.DeleteOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Outcomes.Remove((SpinOutcome)parameter);
                return Task.FromResult(0);
            });
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new SpinGameCommand(name, triggers, requirements, this.Outcomes.Select(o => o.GetGameOutcome()));
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            foreach (SpinOutcome outcome in this.Outcomes)
            {
                if (string.IsNullOrEmpty(outcome.Name))
                {
                    await DialogHelper.ShowMessage("An outcome is missing a name");
                    return false;
                }

                if (outcome.Command == null)
                {
                    await DialogHelper.ShowMessage("An outcome is missing a command");
                    return false;
                }
            }

            int userTotalChance = this.Outcomes.Select(o => o.UserChance).Sum();
            if (userTotalChance != 100)
            {
                await DialogHelper.ShowMessage("The combined User Chance %'s do not equal 100");
                return false;
            }

            int subscriberTotalChance = this.Outcomes.Select(o => o.SubscriberChance).Sum();
            if (subscriberTotalChance != 100)
            {
                await DialogHelper.ShowMessage("The combined Sub Chance %'s do not equal 100");
                return false;
            }

            int modTotalChance = this.Outcomes.Select(o => o.ModChance).Sum();
            if (modTotalChance != 100)
            {
                await DialogHelper.ShowMessage("The combined Mod Chance %'s do not equal 100");
                return false;
            }

            return true;
        }
    }
}
