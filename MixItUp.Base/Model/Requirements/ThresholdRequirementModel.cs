using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public class ThresholdRequirementModel : RequirementModelBase
    {
        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public int TimeSpan { get; set; }

        [DataMember]
        public bool RunForEachUser { get; set; }

        [JsonIgnore]
        private Dictionary<Guid, DateTimeOffset> performs = new Dictionary<Guid, DateTimeOffset>();

        [JsonIgnore]
        private List<Guid> lastApplicableUsers = new List<Guid>();
        [JsonIgnore]
        private Dictionary<Guid, CommandParametersModel> performParameters = new Dictionary<Guid, CommandParametersModel>();

        public ThresholdRequirementModel() { }

        public ThresholdRequirementModel(int amount, int timespan, bool runForEachUser = false)
        {
            this.Amount = amount;
            this.TimeSpan = timespan;
            this.RunForEachUser = runForEachUser;
        }

        internal ThresholdRequirementModel(MixItUp.Base.ViewModel.Requirement.ThresholdRequirementViewModel requirement)
            : this()
        {
            this.Amount = requirement.Amount;
            this.TimeSpan = requirement.TimeSpan;
            this.RunForEachUser = false;
        }

        public bool IsEnabled { get { return this.Amount > 0; } }

        public List<CommandParametersModel> GetApplicableUsers()
        {
            List<CommandParametersModel> users = new List<CommandParametersModel>();
            foreach (Guid userID in this.lastApplicableUsers)
            {
                if (this.performParameters.ContainsKey(userID))
                {
                    users.Add(this.performParameters[userID]);
                }
            }
            return users;
        }

        public override Task<Result> Validate(CommandParametersModel parameters)
        {
            if (this.IsEnabled)
            {
                this.performs[parameters.User.ID] = DateTimeOffset.Now;
                this.performParameters[parameters.User.ID] = parameters;

                DateTimeOffset cutoffDateTime = DateTimeOffset.MinValue;
                if (this.TimeSpan > 0)
                {
                    cutoffDateTime = DateTime.Now.Subtract(new TimeSpan(0, 0, this.TimeSpan));
                }

                foreach (Guid key in this.performs.Keys.ToList())
                {
                    if (this.performs[key] < cutoffDateTime)
                    {
                        this.performs.Remove(key);
                        this.performParameters.Remove(key);
                    }
                }

                if (this.performs.Count < this.Amount)
                {
                    return Task.FromResult(new Result(string.Format(MixItUp.Base.Resources.ThresholdRequirementNeedMore, this.Amount - this.performs.Count)));
                }

                this.lastApplicableUsers.Clear();
                this.lastApplicableUsers.AddRange(this.performs.Keys);
            }
            return Task.FromResult(new Result());
        }

        public override async Task Perform(CommandParametersModel parameters)
        {
            await base.Perform(parameters);
            this.performs.Clear();
            this.performParameters.Clear();
        }
    }
}
