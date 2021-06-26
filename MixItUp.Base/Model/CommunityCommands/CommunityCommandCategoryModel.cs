using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class CommunityCommandCategoryModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public List<CommunityCommandModel> Commands { get; set; } = new List<CommunityCommandModel>();
    }
}
