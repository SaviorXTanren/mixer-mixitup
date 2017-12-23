using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserItemAcquisitonViewModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int AcquireAmount { get; set; }
        [DataMember]
        public int AcquireInterval { get; set; }

        [DataMember]
        public string ResetInterval { get; set; }
        [DataMember]
        public DateTimeOffset LastReset { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        public UserItemAcquisitonViewModel()
        {
            this.ResetInterval = "Never";
        }

        public UserItemAcquisitonViewModel(string name, int acquireAmount, int acquireInterval, string resetInterval)
        {
            this.Name = name;
            this.AcquireAmount = acquireAmount;
            this.AcquireInterval = acquireInterval;
            this.ResetInterval = resetInterval;
        }

        public bool ShouldBeReset()
        {
            if (this.Enabled && !this.ResetInterval.Equals("Never"))
            {
                DateTimeOffset newResetDate = DateTimeOffset.MinValue;
                switch (this.ResetInterval)
                {
                    case "Daily":
                        newResetDate = this.LastReset.AddDays(1);
                        break;
                    case "Weekly":
                        newResetDate = this.LastReset.AddDays(7);
                        break;
                    case "Monthly":
                        newResetDate = this.LastReset.AddMonths(1);
                        break;
                    case "Yearly":
                        newResetDate = this.LastReset.AddYears(1);
                        break;
                }
                return (newResetDate < DateTimeOffset.Now);
            }
            return false;
        }
    }
}
