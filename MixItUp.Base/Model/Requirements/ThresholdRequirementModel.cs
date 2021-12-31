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
        private static DateTimeOffset requirementErrorCooldown = DateTimeOffset.MinValue;

        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public int TimeSpan { get; set; }

        [DataMember]
        public bool RunForEachUser { get; set; }

        [JsonIgnore]
        private Dictionary<Guid, DateTimeOffset> performs = new Dictionary<Guid, DateTimeOffset>();
        [JsonIgnore]
        private Dictionary<Guid, CommandParametersModel> performParameters = new Dictionary<Guid, CommandParametersModel>();

        [JsonIgnore]
        private List<CommandParametersModel> applicablePerformParameters = new List<CommandParametersModel>();

        public ThresholdRequirementModel(int amount, int timespan, bool runForEachUser = false)
        {
            this.Amount = amount;
            this.TimeSpan = timespan;
            this.RunForEachUser = runForEachUser;
        }

        public ThresholdRequirementModel() { }

        protected override DateTimeOffset RequirementErrorCooldown { get { return ThresholdRequirementModel.requirementErrorCooldown; } set { ThresholdRequirementModel.requirementErrorCooldown = value; } }

        public bool IsEnabled { get { return this.Amount > 0; } }

        public IEnumerable<CommandParametersModel> GetApplicableUsers() { return this.applicablePerformParameters;  }

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

                this.applicablePerformParameters.Clear();
                foreach (Guid key in this.performs.Keys)
                {
                    this.applicablePerformParameters.Add(this.performParameters[key]);
                }

                this.performs.Clear();
                this.performParameters.Clear();
            }
            return Task.FromResult(new Result());
        }
    }
}
