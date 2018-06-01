using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [DataContract]
    public class ThresholdRequirementViewModel
    {
        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public int TimeSpan { get; set; }

        [JsonIgnore]
        private Dictionary<UserViewModel, DateTime> performs = new Dictionary<UserViewModel, DateTime>();

        public ThresholdRequirementViewModel() { }

        public ThresholdRequirementViewModel(int amount, int timeSpan)
        {
            this.Amount = amount;
            this.TimeSpan = timeSpan;
        }

        public bool DoesMeetRequirement(UserViewModel user)
        {
            DateTime cutoffDateTime = DateTime.Now.Subtract(new TimeSpan(0, 0, this.TimeSpan));
            List<UserViewModel> toRemove = new List<UserViewModel>();
            foreach (var kvp in this.performs)
            {
                if (kvp.Value < cutoffDateTime)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (UserViewModel userToRemove in toRemove)
            {
                this.performs.Remove(userToRemove);
            }

            this.performs[user] = DateTime.Now;

            if (this.performs.Count >= this.Amount)
            {
                this.performs.Clear();
                return true;
            }
            return false;
        }

        public async Task SendNotMetWhisper(UserViewModel user)
        {
            await ChannelSession.Chat.Whisper(user.UserName, string.Format("This command requires {0} more users to trigger!", this.Amount - this.performs.Count));
        }
    }
}
