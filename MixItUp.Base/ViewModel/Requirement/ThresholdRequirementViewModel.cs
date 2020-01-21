using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<UserViewModel>> GetTriggeringUsers(string commandName, UserViewModel user)
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

            bool sendChatIfNotMet = !this.performs.ContainsKey(user);
            this.performs[user] = DateTime.Now;

            int remaining = this.Amount - this.performs.Count;
            if (remaining <= 0)
            {
                // Need to copy the values before we clear them
                IEnumerable<UserViewModel> triggeringUsers = this.performs.Keys.ToArray();
                this.performs.Clear();
                return triggeringUsers;
            }

            if (sendChatIfNotMet)
            {
                await ChannelSession.Services.Chat.SendMessage(string.Format("The {0} command requires {1} more user(s) to trigger!", commandName, remaining));
            }

            return null;
        }

        public async Task SendThresholdNotMetWhisper(UserViewModel user)
        {
            await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("This command requires {0} more users to trigger!", this.Amount - this.performs.Count));
        }
    }
}
