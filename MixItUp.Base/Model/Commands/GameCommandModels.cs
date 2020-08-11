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

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public abstract class GameCommandModelBase : ChatCommandModel
    {
        public const string GameBetSpecialIdentifier = "gamebet";

        public const string GamePayoutSpecialIdentifier = "gamepayout";

        public const string GameWinnersSpecialIdentifier = "gamewinners";

        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public GameCommandModelBase(string name, HashSet<string> triggers) : base(name, CommandTypeEnum.Game, triggers, includeExclamation: true, wildcards: false) { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return GameCommandModelBase.commandLockSemaphore; } }

        public override bool DoesCommandHaveWork { get { return true; } }

        public GameCurrencyRequirementModel GameCurrencyRequirement { get { return (GameCurrencyRequirementModel)this.Requirements.Requirements.FirstOrDefault(r => r is GameCurrencyRequirementModel); } }

        public async Task<int> GetBetAmount(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            GameCurrencyRequirementModel gameCurrencyRequirement = this.GameCurrencyRequirement;
            return (gameCurrencyRequirement != null) ? await gameCurrencyRequirement.GetGameAmount(user, platform, arguments, specialIdentifiers) : 0;
        }

        protected override Task<bool> ValidateRequirements(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            return base.ValidateRequirements(user, platform, arguments, specialIdentifiers);
        }

        protected override Task<IEnumerable<UserViewModel>> PerformRequirements(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            return base.PerformRequirements(user, platform, arguments, specialIdentifiers);
        }

        protected override Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            return base.PerformInternal(user, platform, arguments, specialIdentifiers);
        }

        protected async Task PerformOutcome(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers, GameOutcomeModel outcome, int betAmount)
        {
            int payout = outcome.GetPayout(user, betAmount);
            specialIdentifiers[GameCommandBase.GameBetSpecialIdentifier] = betAmount.ToString();
            specialIdentifiers[GameCommandBase.GamePayoutSpecialIdentifier] = payout.ToString();

            if (outcome.Command != null)
            {
                await outcome.Command.Perform(user, platform, arguments, specialIdentifiers);
            }
        }

        protected GameOutcomeModel SelectRandomOutcome(UserViewModel user, IEnumerable<GameOutcomeModel> outcomes)
        {
            int randomNumber = this.GenerateProbability();
            int cumulativeOutcomeProbability = 0;
            foreach (GameOutcomeModel outcome in outcomes)
            {
                if (cumulativeOutcomeProbability < randomNumber && randomNumber <= (cumulativeOutcomeProbability + outcome.GetRoleProbability(user)))
                {
                    return outcome;
                }
                cumulativeOutcomeProbability += outcome.GetRoleProbability(user);
            }
            return outcomes.Last();
        }

        protected int GenerateProbability() { return RandomHelper.GenerateProbability(); }

        protected int GenerateRandomNumber(int maxValue) { return RandomHelper.GenerateRandomNumber(maxValue); }

        protected int GenerateRandomNumber(int minValue, int maxValue) { return RandomHelper.GenerateRandomNumber(minValue, maxValue); }
    }

    [DataContract]
    public class GameOutcomeModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Dictionary<UserRoleEnum, double> RolePayouts { get; set; }

        [DataMember]
        public Dictionary<UserRoleEnum, int> RoleProbabilities { get; set; }

        [DataMember]
        public CustomCommandModel Command { get; set; }

        public GameOutcomeModel() { }

        public GameOutcomeModel(string name, double payout, Dictionary<UserRoleEnum, int> roleProbabilities, CustomCommandModel command)
            : this(name, new Dictionary<UserRoleEnum, double>() { { UserRoleEnum.User, payout } }, roleProbabilities, command)
        { }

        public GameOutcomeModel(string name, Dictionary<UserRoleEnum, double> rolePayouts, Dictionary<UserRoleEnum, int> roleProbabilities, CustomCommandModel command)
        {
            this.Name = name;
            this.RolePayouts = rolePayouts;
            this.RoleProbabilities = roleProbabilities;
            this.Command = command;
        }

        public int GetRoleProbability(UserViewModel user)
        {
            var roleProbabilities = this.RoleProbabilities.Select(kvp => kvp.Key).OrderByDescending(k => k);
            if (roleProbabilities.Any(r => user.HasPermissionsTo(r)))
            {
                return this.RoleProbabilities[roleProbabilities.FirstOrDefault(r => user.HasPermissionsTo(r))];
            }
            return this.RoleProbabilities[roleProbabilities.LastOrDefault()];
        }

        public int GetPayout(UserViewModel user, int betAmount)
        {
            return Convert.ToInt32(Convert.ToDouble(betAmount) * this.GetPayoutAmount(user));
        }

        private double GetPayoutAmount(UserViewModel user)
        {
            var rolePayouts = this.RolePayouts.Select(kvp => kvp.Key).OrderByDescending(k => k);
            if (rolePayouts.Any(r => user.HasPermissionsTo(r)))
            {
                return this.RolePayouts[rolePayouts.FirstOrDefault(r => user.HasPermissionsTo(r))];
            }
            return this.RolePayouts[rolePayouts.LastOrDefault()];
        }
    }

    [DataContract]
    public abstract class SinglePlayerGameCommandModelBase : GameCommandModelBase
    {
        public List<GameOutcomeModel> Outcomes { get; set; } = new List<GameOutcomeModel>();

        public SinglePlayerGameCommandModelBase(string name, HashSet<string> triggers, IEnumerable<GameOutcomeModel> outcomes)
            : base(name, triggers)
        {
            this.Outcomes = new List<GameOutcomeModel>(outcomes);
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            await this.PerformOutcome(user, platform, arguments, specialIdentifiers, this.SelectRandomOutcome(user, this.Outcomes), await this.GetBetAmount(user, platform, arguments, specialIdentifiers));
        }
    }

    [DataContract]
    public class SpinGameCommandModel : SinglePlayerGameCommandModelBase
    {
        public SpinGameCommandModel(string name, HashSet<string> triggers, IEnumerable<GameOutcomeModel> outcomes) : base(name, triggers, outcomes) { }
    }
}
