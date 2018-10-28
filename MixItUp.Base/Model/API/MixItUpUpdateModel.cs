using Newtonsoft.Json;
using System;
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

        [JsonIgnore]
        public Version SystemVersion { get { return new Version(this.Version); } }
    }
}
