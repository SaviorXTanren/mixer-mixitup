using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.API.V2.Models
{
    [DataContract]
    public class GetInventoryResponse
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<GetInventoryItemResponse> Items { get; set; }
    }

    [DataContract]
    public class GetInventoryItemResponse
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
