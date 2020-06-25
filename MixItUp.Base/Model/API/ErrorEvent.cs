using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Reflection;
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

        public ErrorEvent() { }

        public ErrorEvent(string details, bool isCrash)
        {
            //this.MixerUserID = (int)ChannelSession.MixerUser.id;
            this.AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
            this.Details = details;
            this.IsCrash = isCrash;
        }

        [JsonIgnore]
        public string ErrorHash { get { return HashHelper.ComputeMD5Hash(this.Details + this.IsCrash.ToString()); } }
    }
}
