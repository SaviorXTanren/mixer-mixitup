using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class CommunityCommandReportModel
    {
        [DataMember]
        public Guid CommandID { get; set; }
        [DataMember]
        public string Report { get; set; }
    }
}
