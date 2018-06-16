using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreListingUsesModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public long Uses { get; set; }
    }
}
