using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
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
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public GameCommandModelBase(string name, HashSet<string> triggers) : base(name, CommandTypeEnum.Game, triggers, includeExclamation: true, wildcards: false) { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return GameCommandModelBase.commandLockSemaphore; } }

        public override bool DoesCommandHaveWork { get { return true; } }

        protected override Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            throw new NotImplementedException();
        }
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
        public CustomCommand Command { get; set; }

        public GameOutcomeModel() { }

        public GameOutcomeModel(string name, double payout, Dictionary<UserRoleEnum, int> roleProbabilities, CustomCommand command)
            : this(name, new Dictionary<UserRoleEnum, double>() { { UserRoleEnum.User, payout } }, roleProbabilities, command)
        { }

        public GameOutcomeModel(string name, Dictionary<UserRoleEnum, double> rolePayouts, Dictionary<UserRoleEnum, int> roleProbabilities, CustomCommand command)
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

        protected override Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract]
    public class SpinGameCommandModel : SinglePlayerGameCommandModelBase
    {
        public SpinGameCommandModel(string name, HashSet<string> triggers, IEnumerable<GameOutcomeModel> outcomes) : base(name, triggers, outcomes) { }
    }
}
