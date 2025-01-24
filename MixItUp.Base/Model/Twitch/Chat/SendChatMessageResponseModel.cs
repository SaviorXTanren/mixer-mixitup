using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Twitch.Chat
{
    [DataContract]
    public class SendChatMessageResponseModel
    {
        [DataMember]
        public string message_id { get; set; }
        [DataMember]
        public bool is_sent { get; set; }
        [DataMember]
        public SendChatMessageResponseDropReasonModel drop_reason { get; set; }
    }

    [DataContract]
    public class SendChatMessageResponseDropReasonModel
    {
        [DataMember]
        public string code { get; set; }
        [DataMember]
        public string message { get; set; }
    }
}
