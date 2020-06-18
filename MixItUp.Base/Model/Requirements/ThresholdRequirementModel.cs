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

        [DataMember]
        public bool RunForEachUser { get; set; }

        [JsonIgnore]
        private Dictionary<Guid, DateTimeOffset> performs = new Dictionary<Guid, DateTimeOffset>();

        [JsonIgnore]
        private List<Guid> lastApplicableUsers = new List<Guid>();

        public ThresholdRequirementModel() { }

        public ThresholdRequirementModel(int amount, int timespan, bool runForEachUser = false)
        {
            this.Amount = amount;
            this.TimeSpan = timespan;
            this.RunForEachUser = runForEachUser;
        }

        public bool IsEnabled { get { return this.Amount > 0; } }

        public List<UserViewModel> GetApplicableUsers()
        {
            List<UserViewModel> users = new List<UserViewModel>();
            foreach (Guid userID in this.lastApplicableUsers)
            {
                UserViewModel user = ChannelSession.Services.User.GetUserByID(userID);
                if (user != null)
                {
                    users.Add(user);
                }
            }
            return users;
        }

        public override async Task<bool> Validate(UserViewModel user)
        {
            if (this.IsEnabled)
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

                this.lastApplicableUsers.Clear();
                this.lastApplicableUsers.AddRange(this.performs.Keys);
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
