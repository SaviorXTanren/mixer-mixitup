using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreListingReportModel
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public uint UserID { get; set; }

        [DataMember]
        public string Report { get; set; }

        [DataMember]
        public StoreListingModel Listing { get; set; }
    }
}
