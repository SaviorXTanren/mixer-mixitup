using MixItUp.Base.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class RoleProbabilityPayoutModel
    {
        [DataMember]
        public UserRoleEnum Role { get; set; }

        [DataMember]
        public int Probability { get; set; }

        [DataMember]
        public double Payout { get; set; }

        public RoleProbabilityPayoutModel(UserRoleEnum role, int probability) : this(role, probability, 0) { }

        public RoleProbabilityPayoutModel(UserRoleEnum role, int probability, double payout)
        {
            this.Role = role;
            this.Probability = probability;
            this.Payout = payout;
        }

        private RoleProbabilityPayoutModel() { }
    }

    [DataContract]
    public class GameOutcomeModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Dictionary<UserRoleEnum, RoleProbabilityPayoutModel> RoleProbabilityPayouts { get; set; }

        [DataMember]
        public CustomCommandModel Command { get; set; }

        public GameOutcomeModel(string name, Dictionary<UserRoleEnum, RoleProbabilityPayoutModel> roleProbabilityPayouts, CustomCommandModel command)
        {
            this.Name = name;
            this.RoleProbabilityPayouts = roleProbabilityPayouts;
            this.Command = command;
        }

        protected GameOutcomeModel() { }

        public RoleProbabilityPayoutModel GetRoleProbabilityPayout(UserViewModel user)
        {
            var roleProbabilities = this.RoleProbabilityPayouts.Where(kvp => user.HasPermissionsTo(kvp.Key)).OrderByDescending(kvp => kvp.Key);
            if (roleProbabilities.Count() > 0)
            {
                return roleProbabilities.FirstOrDefault().Value;
            }
            return null;
        }

        public int GetPayout(UserViewModel user, int betAmount)
        {
            RoleProbabilityPayoutModel roleProbabilityPayout = this.GetRoleProbabilityPayout(user);
            if (roleProbabilityPayout != null)
            {
                return Convert.ToInt32(Convert.ToDouble(betAmount) * roleProbabilityPayout.Payout);
            }
            return 0;
        }
    }

    [DataContract]
    public abstract class GameCommandModelBase : ChatCommandModel
    {
        public const string GameBetSpecialIdentifier = "gamebet";
        public const string GamePayoutSpecialIdentifier = "gamepayout";
        public const string GameWinnersSpecialIdentifier = "gamewinners";

        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public GameCommandModelBase(string name, HashSet<string> triggers) : base(name, CommandTypeEnum.Game, triggers, includeExclamation: true, wildcards: false) { }

        protected GameCommandModelBase() : base() { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return GameCommandModelBase.commandLockSemaphore; } }

        public override bool DoesCommandHaveWork { get { return true; } }

        public GameCurrencyRequirementModel GameCurrencyRequirement { get { return (GameCurrencyRequirementModel)this.Requirements.Requirements.FirstOrDefault(r => r is GameCurrencyRequirementModel); } }

        public virtual IEnumerable<CommandModelBase> GetInnerCommands() { return new List<CommandModelBase>(); }

        protected int GetBetAmount(CommandParametersModel parameters)
        {
            GameCurrencyRequirementModel gameCurrencyRequirement = this.GameCurrencyRequirement;
            return (gameCurrencyRequirement != null) ? gameCurrencyRequirement.GetGameAmount(parameters) : 0;
        }

        protected UserViewModel GetRandomUser(CommandParametersModel parameters)
        {
            return ChannelSession.Services.User.GetRandomUser(parameters);
        }

        protected override Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            return base.ValidateRequirements(parameters);
        }

        protected override async Task<IEnumerable<CommandParametersModel>> PerformRequirements(CommandParametersModel parameters)
        {
            parameters.SpecialIdentifiers[GameCommandBase.GameBetSpecialIdentifier] = this.GetBetAmount(parameters).ToString();
            return await base.PerformRequirements(parameters);
        }

        protected override Task PerformInternal(CommandParametersModel parameters)
        {
            return base.PerformInternal(parameters);
        }

        protected GameOutcomeModel SelectRandomOutcome(UserViewModel user, IEnumerable<GameOutcomeModel> outcomes)
        {
            int randomNumber = this.GenerateProbability();
            int cumulativeOutcomeProbability = 0;
            foreach (GameOutcomeModel outcome in outcomes)
            {
                RoleProbabilityPayoutModel roleProbabilityPayout = outcome.GetRoleProbabilityPayout(user);
                if (roleProbabilityPayout != null)
                {
                    if (cumulativeOutcomeProbability < randomNumber && randomNumber <= (cumulativeOutcomeProbability + roleProbabilityPayout.Probability))
                    {
                        return outcome;
                    }
                    cumulativeOutcomeProbability += roleProbabilityPayout.Probability;
                }
            }
            return outcomes.Last();
        }

        protected async Task PerformOutcome(CommandParametersModel parameters, GameOutcomeModel outcome, int betAmount)
        {
            parameters.SpecialIdentifiers[GameCommandBase.GamePayoutSpecialIdentifier] = outcome.GetPayout(parameters.User, betAmount).ToString();
            if (outcome.Command != null)
            {
                await outcome.Command.Perform(parameters);
            }
        }

        protected int GenerateProbability() { return RandomHelper.GenerateProbability(); }

        protected int GenerateRandomNumber(int maxValue) { return RandomHelper.GenerateRandomNumber(maxValue); }

        protected int GenerateRandomNumber(int minValue, int maxValue) { return RandomHelper.GenerateRandomNumber(minValue, maxValue); }
    }
}
