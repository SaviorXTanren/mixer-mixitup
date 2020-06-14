using MixItUp.Base.ViewModel.User;
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

        [JsonIgnore]
        private Dictionary<Guid, DateTimeOffset> performs = new Dictionary<Guid, DateTimeOffset>();

        public ThresholdRequirementModel() { }

        public ThresholdRequirementModel(int amount, int timespan)
        {
            this.Amount = amount;
            this.TimeSpan = timespan;
        }

        public List<UserViewModel> GetApplicableUsers()
        {
            DateTime cutoffDateTime = DateTime.Now.Subtract(new TimeSpan(0, 0, this.TimeSpan));
            List<UserViewModel> users = new List<UserViewModel>();
            foreach (var kvp in this.performs)
            {
                if (kvp.Value >= cutoffDateTime)
                {
                    UserViewModel user = ChannelSession.Services.User.GetUserByID(kvp.Key);
                    if (user != null)
                    {
                        users.Add(user);
                    }
                }
            }
            return users;
        }

        public override async Task<bool> Validate(UserViewModel user)
        {
            this.performs[user.ID] = DateTimeOffset.Now;

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
                }
            }

            if (this.performs.Count < this.Amount)
            {
                await this.SendChatMessage(string.Format("This command requires {0} more users to trigger!", this.Amount - this.performs.Count));
                return false;
            }

            return true;
        }

        public override Task Perform(UserViewModel user)
        {
            this.performs.Clear();
            return Task.FromResult(0);
        }
    }
}
