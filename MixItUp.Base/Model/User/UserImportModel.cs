using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    [DataContract]
    public class UserImportModel
    {
        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();

        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; }
        [DataMember]
        public string PlatformID { get; set; }
        [DataMember]
        public string PlatformUsername { get; set; }

        [DataMember]
        public int OnlineViewingMinutes { get; set; }

        [DataMember]
        public Dictionary<Guid, int> CurrencyAmounts { get; set; } = new Dictionary<Guid, int>();
    }
}
