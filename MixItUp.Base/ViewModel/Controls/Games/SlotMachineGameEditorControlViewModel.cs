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
    public class SlotMachineOutcome : UIViewModelBase
    {
        public string Symbol1 { get; set; }
        public string Symbol2 { get; set; }
        public string Symbol3 { get; set; }

        public CustomCommand Command { get; set; }

        public double UserPayout { get; set; }
        public double SubscriberPayout { get; set; }
        public double ModPayout { get; set; }

        public bool AnyOrder { get; set; }

        public SlotMachineOutcome(string symbol1, string symbol2, string symbol3, CustomCommand command, double userPayout = 0, double subscriberPayout = 0, double modPayout = 0, bool anyOrder = false)
        {
            this.Symbol1 = symbol1;
            this.Symbol2 = symbol2;
            this.Symbol3 = symbol3;
            this.AnyOrder = anyOrder;
            this.Command = command;
            this.UserPayout = userPayout;
            this.SubscriberPayout = subscriberPayout;
            this.ModPayout = modPayout;
        }

        public SlotMachineOutcome(SlotsGameOutcome outcome)
        {
            this.Symbol1 = outcome.Symbol1;
            this.Symbol2 = outcome.Symbol2;
            this.Symbol3 = outcome.Symbol3;
            this.AnyOrder = outcome.AnyOrder;
            this.Command = outcome.Command;
            this.UserPayout = outcome.RolePayouts[MixerRoleEnum.User] * 100.0;
            this.SubscriberPayout = outcome.RolePayouts[MixerRoleEnum.Subscriber] * 100.0;
            this.ModPayout = outcome.RolePayouts[MixerRoleEnum.Mod] * 100.0;
        }

        public string UserPayoutString
        {
            get { return this.UserPayout.ToString(); }
            set { this.UserPayout = this.GetPositiveIntFromString(value); }
        }

        public string SubscriberPayoutString
        {
            get { return this.SubscriberPayout.ToString(); }
            set { this.SubscriberPayout = this.GetPositiveIntFromString(value); }
        }

        public string ModPayoutString
        {
            get { return this.ModPayout.ToString(); }
            set { this.ModPayout = this.GetPositiveIntFromString(value); }
        }

        public SlotsGameOutcome GetGameOutcome()
        {
            return new SlotsGameOutcome(this.Symbol1 + " " + this.Symbol2 + " " + this.Symbol3, this.Symbol1, this.Symbol2, this.Symbol3,
                new Dictionary<MixerRoleEnum, double>() { { MixerRoleEnum.User, this.UserPayout / 100.0 }, { MixerRoleEnum.Subscriber, this.SubscriberPayout / 100.0 }, { MixerRoleEnum.Mod, this.ModPayout / 100.0 } },
                this.Command, this.AnyOrder);
        }
    }

    public class SlotMachineGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string Symbols
        {
            get { return this.symbols; }
            set
            {
                this.symbols = value;
                this.NotifyPropertyChanged();
            }
        }
        private string symbols = "X O $";

        public ObservableCollection<SlotMachineOutcome> Outcomes { get; set; } = new ObservableCollection<SlotMachineOutcome>();

        public CustomCommand FailureOutcomeCommand { get; set; }

        public ICommand AddOutcomeCommand { get; set; }
        public ICommand DeleteOutcomeCommand { get; set; }

        private SlotMachineGameCommand existingCommand;

        public SlotMachineGameEditorControlViewModel(UserCurrencyModel currency)
            : this()
        {
            this.FailureOutcomeCommand = this.CreateBasicChatCommand("Result: $gameslotsoutcome - Looks like luck was not on your side. Better luck next time...", whisper: true);

            this.Outcomes.Add(new SlotMachineOutcome("O", "O", "O", this.CreateBasicChatCommand("Result: $gameslotsoutcome - @$username walks away with $gamepayout " + currency.Name + "!"), 200, 200, 200));
            this.Outcomes.Add(new SlotMachineOutcome("$", "O", "$", this.CreateBasicChatCommand("Result: $gameslotsoutcome - @$username walks away with $gamepayout " + currency.Name + "!"), 150, 150, 150, anyOrder: true));
            this.Outcomes.Add(new SlotMachineOutcome("X", "$", "O", this.CreateBasicChatCommand("Result: $gameslotsoutcome - @$username walks away with $gamepayout " + currency.Name + "!"), 500, 500, 500, anyOrder: true));
        }

        public SlotMachineGameEditorControlViewModel(SlotMachineGameCommand command)
            : this()
        {
            this.existingCommand = command;

            this.Symbols = string.Join(" ", this.existingCommand.AllSymbols);
            this.FailureOutcomeCommand = this.existingCommand.FailureOutcomeCommand;

            foreach (GameOutcome outcome in this.existingCommand.Outcomes)
            {
                this.Outcomes.Add(new SlotMachineOutcome((SlotsGameOutcome)outcome));
            }
        }

        private SlotMachineGameEditorControlViewModel()
        {
            this.AddOutcomeCommand = this.CreateCommand((parameter) =>
            {
                UserCurrencyModel currency = (UserCurrencyModel)parameter;
                this.Outcomes.Add(new SlotMachineOutcome(null, null, null, this.CreateBasicChatCommand("Result: $gameslotsoutcome - @$username walks away with $gamepayout " + currency.Name + "!")));
                return Task.FromResult(0);
            });

            this.DeleteOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Outcomes.Remove((SlotMachineOutcome)parameter);
                return Task.FromResult(0);
            });
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            List<string> symbolsList = new List<string>(this.Symbols.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));

            GameCommandBase newCommand = new SlotMachineGameCommand(name, triggers, requirements, this.Outcomes.Select(o => o.GetGameOutcome()), symbolsList, this.FailureOutcomeCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.Symbols))
            {
                await DialogHelper.ShowMessage("No slot symbols have been entered");
                return false;
            }

            List<string> symbolsList = new List<string>(this.Symbols.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            if (symbolsList.Count < 2)
            {
                await DialogHelper.ShowMessage("At least 2 slots symbols must be entered");
                return false;
            }

            if (symbolsList.GroupBy(s => s).Any(g => g.Count() > 1))
            {
                await DialogHelper.ShowMessage("All slot symbols must be unique");
                return false;
            }

            HashSet<string> symbols = new HashSet<string>(symbolsList);

            foreach (SlotMachineOutcome outcome in this.Outcomes)
            {
                if (string.IsNullOrEmpty(outcome.Symbol1) || string.IsNullOrEmpty(outcome.Symbol2) || string.IsNullOrEmpty(outcome.Symbol3))
                {
                    await DialogHelper.ShowMessage("An outcome is missing its symbols");
                    return false;
                }

                if (!symbols.Contains(outcome.Symbol1) || !symbols.Contains(outcome.Symbol2) || !symbols.Contains(outcome.Symbol3))
                {
                    await DialogHelper.ShowMessage("An outcome contains a symbol not found in the set of all slot symbols");
                    return false;
                }

                if (outcome.UserPayout < 0)
                {
                    await DialogHelper.ShowMessage("The User Payout %'s is not a valid number greater than or equal to 0");
                    return false;
                }

                if (outcome.SubscriberPayout < 0)
                {
                    await DialogHelper.ShowMessage("The Subscriber Payout %'s is not a valid number greater than or equal to 0");
                    return false;
                }

                if (outcome.ModPayout < 0)
                {
                    await DialogHelper.ShowMessage("The Mod Payout %'s is not a valid number greater than or equal to 0");
                    return false;
                }

                if (outcome.Command == null)
                {
                    await DialogHelper.ShowMessage("An outcome is missing a command");
                    return false;
                }
            }

            return true;
        }
    }
}
