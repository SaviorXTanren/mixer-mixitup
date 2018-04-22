using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    [DataContract]
    public class UserDonationModel
    {
        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string ImageLink { get; set; }

        [DataMember]
        public double Amount { get; set; }
        [DataMember]
        public string AmountText { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }
    }
}
