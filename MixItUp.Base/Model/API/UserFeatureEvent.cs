using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class UserFeatureEvent
    {
        [JsonProperty]
        public int MixerUserID { get; set; }

        public UserFeatureEvent() { }

        public UserFeatureEvent(uint userID)
        {
            this.MixerUserID = (int)userID;
        }
    }
}
