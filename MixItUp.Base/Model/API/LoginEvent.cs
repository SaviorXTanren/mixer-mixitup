using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class LoginEvent
    {
        [JsonProperty]
        public int MixerUserID { get; set; }
        [JsonProperty]
        public string AppVersion { get; set; }
        [JsonProperty]
        public bool Feature { get; set; }
        [JsonProperty]
        public string Details { get; set; }
    }
}
