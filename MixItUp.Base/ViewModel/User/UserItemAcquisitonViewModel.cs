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
        public bool Enabled { get; set; }

        public UserItemAcquisitonViewModel() { }

        public UserItemAcquisitonViewModel(string name, int acquireAmount, int acquireInterval)
        {
            this.Name = name;
            this.AcquireAmount = acquireAmount;
            this.AcquireInterval = acquireInterval;
        }
    }
}
