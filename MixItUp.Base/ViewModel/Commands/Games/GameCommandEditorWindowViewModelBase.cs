using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Games
{
    public class RoleProbabilityPayoutViewModel : NotifyPropertyChangedBase
    {
        public UserRoleEnum Role
        {
            get { return this.role; }
            set
            {
                this.role = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum role;

        public int Probability
        {
            get { return this.probability; }
            set
            {
                this.probability = value;
                this.NotifyPropertyChanged();
            }
        }
        private int probability;

        public double Payout
        {
            get { return this.payout; }
            set
            {
                this.payout = value;
                this.NotifyPropertyChanged();
            }
        }
        private double payout;

        public RoleProbabilityPayoutViewModel(UserRoleEnum role, int probability, double payout)
        {
            this.Role = role;
            this.Probability = probability;
            this.Payout = payout;
        }

        public RoleProbabilityPayoutViewModel(RoleProbabilityPayoutModel model) : this(model.UserRole, model.Probability, model.Payout * 100) { }

        public RoleProbabilityPayoutModel GetModel() { return new RoleProbabilityPayoutModel(this.Role, this.Probability, ((double)this.Payout) / 100.0); }
    }

    public class GameOutcomeViewModel : NotifyPropertyChangedBase
    {
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public ObservableCollection<RoleProbabilityPayoutViewModel> RoleProbabilityPayouts { get; set; } = new ObservableCollection<RoleProbabilityPayoutViewModel>();

        public int UserChance
        {
            get { return this.userRoleProbabilityPayout.Probability; }
            set
            {
                this.userRoleProbabilityPayout.Probability = value;
                this.NotifyPropertyChanged();
            }
        }
        public double UserPayout
        {
            get { return this.userRoleProbabilityPayout.Payout; }
            set
            {
                this.userRoleProbabilityPayout.Payout = value;
                this.NotifyPropertyChanged();
            }
        }
        private RoleProbabilityPayoutViewModel userRoleProbabilityPayout;

        public int SubChance
        {
            get { return this.subRoleProbabilityPayout.Probability; }
            set
            {
                this.subRoleProbabilityPayout.Probability = value;
                this.NotifyPropertyChanged();
            }
        }
        public double SubPayout
        {
            get { return this.subRoleProbabilityPayout.Payout; }
            set
            {
                this.subRoleProbabilityPayout.Payout = value;
                this.NotifyPropertyChanged();
            }
        }
        private RoleProbabilityPayoutViewModel subRoleProbabilityPayout;

        public int ModChance
        {
            get { return this.modRoleProbabilityPayout.Probability; }
            set
            {
                this.modRoleProbabilityPayout.Probability = value;
                this.NotifyPropertyChanged();
            }
        }
        public double ModPayout
        {
            get { return this.modRoleProbabilityPayout.Payout; }
            set
            {
                this.modRoleProbabilityPayout.Payout = value;
                this.NotifyPropertyChanged();
            }
        }
        private RoleProbabilityPayoutViewModel modRoleProbabilityPayout;

        public CustomCommandModel Command
        {
            get { return this.command; }
            set
            {
                this.command = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel command;

        public GameOutcomeViewModel() : this(string.Empty) { }

        public GameOutcomeViewModel(string name) : this(name, 0, 100) { }

        public GameOutcomeViewModel(string name, int probability, double payout) : this(name, probability, payout, new CustomCommandModel(MixItUp.Base.Resources.GameSubCommand)) { }

        public GameOutcomeViewModel(string name, int probability, double payout, CustomCommandModel command)
        {
            this.Name = name;
            this.RoleProbabilityPayouts.Add(new RoleProbabilityPayoutViewModel(UserRoleEnum.User, probability, payout));
            this.RoleProbabilityPayouts.Add(new RoleProbabilityPayoutViewModel(UserRoleEnum.Subscriber, probability, payout));
            this.RoleProbabilityPayouts.Add(new RoleProbabilityPayoutViewModel(UserRoleEnum.Moderator, probability, payout));
            this.Command = command;
            this.SetRoleProbabilityPayoutProperties();
        }

        public GameOutcomeViewModel(GameOutcomeModel model)
        {
            this.Name = model.Name;
            this.Command = model.Command;

            if (model.UserRoleProbabilityPayouts.Count > 0)
            {
                foreach (var kvp in model.UserRoleProbabilityPayouts)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    if (kvp.Key != kvp.Value.UserRole)
                    {
                        kvp.Value.UserRole = kvp.Key;
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                    this.RoleProbabilityPayouts.Add(new RoleProbabilityPayoutViewModel(kvp.Value));
                }
            }
            else
            {
                this.RoleProbabilityPayouts.Add(new RoleProbabilityPayoutViewModel(UserRoleEnum.User, 0, 0));
                this.RoleProbabilityPayouts.Add(new RoleProbabilityPayoutViewModel(UserRoleEnum.Subscriber, 0, 0));
                this.RoleProbabilityPayouts.Add(new RoleProbabilityPayoutViewModel(UserRoleEnum.Moderator, 0, 0));
            }

            this.SetRoleProbabilityPayoutProperties();
        }
        
        public virtual Result Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return new Result(MixItUp.Base.Resources.GameCommandOutcomeMissingName);
            }

            if (this.Command == null)
            {
                return new Result(MixItUp.Base.Resources.GameCommandOutcomeMissingCommand);
            }

            return new Result();
        }

        public virtual GameOutcomeModel GetModel() { return new GameOutcomeModel(this.Name, this.RoleProbabilityPayouts.ToDictionary(rpp => rpp.Role, rpp => rpp.GetModel()), this.Command); }

        private void SetRoleProbabilityPayoutProperties()
        {
            this.userRoleProbabilityPayout = this.RoleProbabilityPayouts.FirstOrDefault(rpp => rpp.Role == UserRoleEnum.User);
            this.subRoleProbabilityPayout = this.RoleProbabilityPayouts.FirstOrDefault(rpp => rpp.Role == UserRoleEnum.Subscriber);
            this.modRoleProbabilityPayout = this.RoleProbabilityPayouts.FirstOrDefault(rpp => rpp.Role == UserRoleEnum.Moderator);
        }
    }

    public abstract class GameCommandEditorWindowViewModelBase : ChatCommandEditorWindowViewModel
    {
        public ObservableCollection<GameOutcomeViewModel> Outcomes { get; set; } = new ObservableCollection<GameOutcomeViewModel>();

        public ICommand AddOutcomeCommand { get; set; }
        public ICommand DeleteOutcomeCommand { get; set; }

        protected string PrimaryCurrencyName { get; private set; } = "________";

        public GameCommandEditorWindowViewModelBase(GameCommandModelBase existingCommand) : base(existingCommand) { this.SetUICommands(); }

        public GameCommandEditorWindowViewModelBase(CurrencyModel currency)
            : base(CommandTypeEnum.Game)
        {
            if (currency != null && this.AutoAddCurrencyRequirement)
            {
                this.PrimaryCurrencyName = currency.Name;
                this.Requirements.Currency.Add(new CurrencyRequirementModel(currency, CurrencyRequirementTypeEnum.RequiredAmount, 10, 100));
            }
            this.SetUICommands();
        }

        public override bool ShowCommandGroupSelector { get { return false; } }

        public override bool CanBeUnlocked { get { return false; } }

        public override bool CheckActionCount { get { return false; } }

        public virtual bool RequirePrimaryCurrency { get { return false; } }

        public virtual bool AutoAddCurrencyRequirement { get { return true; } }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (this.RequirePrimaryCurrency)
            {
                if (this.Requirements.Currency.Items.Count == 0)
                {
                    return new Result(MixItUp.Base.Resources.GameCommandRequiresAtLeast1CurrencyRequirement);
                }
            }

            return new Result();
        }

        public override Task<CommandModelBase> CreateNewCommand() { throw new NotImplementedException(); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            GameCommandModelBase c = (GameCommandModelBase)command;
            ServiceManager.Get<CommandService>().GameCommands.Remove(c);
            ServiceManager.Get<CommandService>().GameCommands.Add(c);
            ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            return Task.CompletedTask;
        }

        protected Result ValidateOutcomes(IEnumerable<GameOutcomeViewModel> outcomes)
        {
            if (outcomes.Count() == 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandAtLeast1Outcome);
            }

            Dictionary<UserRoleEnum, int> probabilities = new Dictionary<UserRoleEnum, int>();
            foreach (RoleProbabilityPayoutViewModel rpp in outcomes.First().RoleProbabilityPayouts)
            {
                probabilities[rpp.Role] = 0;
            }

            foreach (GameOutcomeViewModel outcome in outcomes)
            {
                Result result = outcome.Validate();
                if (!result.Success)
                {
                    return result;
                }

                foreach (RoleProbabilityPayoutViewModel rpp in outcome.RoleProbabilityPayouts)
                {
                    probabilities[rpp.Role] += rpp.Probability;
                }
            }

            foreach (var kvp in probabilities)
            {
                if (kvp.Value != 100)
                {
                    return new Result(string.Format(MixItUp.Base.Resources.GameCommandRoleProbabilityNotEqualTo100, EnumLocalizationHelper.GetLocalizedName(kvp.Key)));
                }
            }

            return new Result();
        }

        protected CustomCommandModel CreateBasicCommand()
        {
            return new CustomCommandModel(MixItUp.Base.Resources.GameSubCommand)
            {
                IsEmbedded = true
            };
        }

        protected CustomCommandModel CreateBasicChatCommand(string message, bool whisper = false)
        {
            CustomCommandModel command = this.CreateBasicCommand();
            command.Actions.Add(new ChatActionModel(message, sendAsStreamer: false, isWhisper: whisper));
            return command;
        }

        protected CustomCommandModel CreateBasicChatCurrencyCommand(string message, CurrencyModel currency, string amount, bool whisper = false)
        {
            CustomCommandModel command = this.CreateBasicCommand();
            command.Actions.Add(new ChatActionModel(message, sendAsStreamer: false, isWhisper: whisper));
            command.Actions.Add(new ConsumablesActionModel(currency, ConsumablesActionTypeEnum.AddToUser, usersMustBePresent: true, amount));
            return command;
        }

        private void SetUICommands()
        {
            this.AddOutcomeCommand = this.CreateCommand(() =>
            {
                this.Outcomes.Add(new GameOutcomeViewModel());
            });

            this.DeleteOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Outcomes.Remove((GameOutcomeViewModel)parameter);
            });
        }
    }
}
