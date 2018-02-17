using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserDonationViewModel
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public double Amount { get; set; }
        [DataMember]
        public string CurrencyName { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }
    }
}
