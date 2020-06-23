using Newtonsoft.Json;
using System.Reflection;
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

        public LoginEvent() { }

        public LoginEvent(string details)
        {
            //this.MixerUserID = (int)ChannelSession.MixerUser.id;
            this.AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
            this.Feature = ChannelSession.Settings.FeatureMe;
            this.Details = details;
        }
    }
}
