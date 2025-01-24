using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Twitch.Clients.PubSub
{
    /// <summary>
    /// A PubSub web socket message packet.
    /// </summary>
    public class PubSubMessagePacketModel : PubSubPacketModel
    {
        /// <summary>
        /// The topic of the message.
        /// </summary>
        [JsonIgnore]
        public string topic { get { return (this.data != null && this.data.ContainsKey("topic")) ? this.data["topic"].ToString() : null; } }
        /// <summary>
        /// The topic type of the message.
        /// </summary>
        [JsonIgnore]
        public PubSubTopicsEnum topicType
        {
            get
            {
                if (!string.IsNullOrEmpty(this.topic))
                {
                    string[] splits = this.topic.Split(new char[] { '.' });
                    if (splits.Length > 0)
                    {
                        return EnumHelper.GetEnumValueFromString<PubSubTopicsEnum>(splits[0]);
                    }
                }
                return default(PubSubTopicsEnum);
            }
        }
        /// <summary>
        /// The topic ID of the message.
        /// </summary>
        public string topicID
        {
            get
            {
                if (!string.IsNullOrEmpty(this.topic))
                {
                    string[] splits = this.topic.Split(new char[] { '.' });
                    if (splits.Length > 1)
                    {
                        return splits[1];
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The message contents as a string.
        /// </summary>
        [JsonIgnore]
        public string message { get { return (this.data != null && this.data.ContainsKey("message")) ? this.data["message"].ToString() : null; } }
        /// <summary>
        /// The message contents as a data model.
        /// </summary>
        [JsonIgnore]
        public PubSubMessagePacketDataModel messageData { get { return (!string.IsNullOrEmpty(this.message)) ? JSONSerializerHelper.DeserializeFromString<PubSubMessagePacketDataModel>(this.message) : null; } }
    }

    /// <summary>
    /// The message data for a packet.
    /// </summary>
    public class PubSubMessagePacketDataModel
    {
        /// <summary>
        /// The type of data.
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// The JToken data of the message.
        /// </summary>
        public object data { get; set; }
        /// <summary>
        /// The JToken data of the message.
        /// </summary>
        [JsonIgnore]
        public JToken data_object { get { return (this.data is string) ? JToken.Parse((string)this.data) : (JToken)this.data; } }
    }
}
