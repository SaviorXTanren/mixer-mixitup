using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class MixItUpUpdateModel
    {
        [JsonProperty]
        public string Version { get; set; }
        [JsonProperty]
        public string AutoUpdaterLink { get; set; }
        [JsonProperty]
        public string ChangelogLink { get; set; }
    }
}
