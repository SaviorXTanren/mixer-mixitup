using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreListingReportModel
    {
        [DataMember]
        public Guid ListingID { get; set; }
        [DataMember]
        public uint UserID { get; set; }

        [DataMember]
        public string Report { get; set; }

        [DataMember]
        public StoreListingModel Listing { get; set; }

        public StoreListingReportModel() { }

        public StoreListingReportModel(StoreListingModel listing, string report)
        {
            this.ListingID = listing.ID;
            this.UserID = ChannelSession.MixerUser.id;
            this.Report = report;
        }
    }
}
