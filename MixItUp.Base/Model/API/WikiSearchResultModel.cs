using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class WikiSearchResultPageModel
    {
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string path { get; set; }
        [DataMember]
        public string locale { get; set; }
    }
}
