using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class ErrorEvent
    {
        [JsonProperty]
        public int MixerUserID { get; set; }
        [JsonProperty]
        public string AppVersion { get; set; }
        [JsonProperty]
        public string Details { get; set; }
        [JsonProperty]
        public bool IsCrash { get; set; }

        [JsonIgnore]
        public string ErrorHash { get { return HashHelper.ComputeMD5Hash(this.Details + this.IsCrash.ToString()); } }
    }
}
