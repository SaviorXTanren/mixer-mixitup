using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class IssueReportModel
    {
        [JsonProperty]
        public uint MixerUserID { get; set; }
        [JsonProperty]
        public string EmailAddress { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string LogContents { get; set; }

        public IssueReportModel() { }
    }
}
