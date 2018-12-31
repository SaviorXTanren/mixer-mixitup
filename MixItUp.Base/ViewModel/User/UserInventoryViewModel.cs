using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserInventoryViewModel
    {
        [DataMember]
        public Guid ID { get; set; }
    }
}
