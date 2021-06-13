using MixItUp.Base.Model.Commands;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    public enum CommunityCommandTagEnum
    {
        [Name("ChatMessage")]
        Chat,
        [Name("ConsumablesCurrencyRankEtc")]
        Consumables,
        ExternalProgram,
        [Name("InputKeyboardAndMouse")]
        Input,
        [Name("OverlayImagesAndVideos")]
        Overlay,
        Sound,
        Wait,
        [Name("CounterCreateAndUpdate")]
        Counter,
        GameQueue,
        TextToSpeech,
        WebRequest,
        SpecialIdentifier,
        [Name("FileReadAndWrite")]
        File,
        Discord,
        Twitter,
        Conditional,
        [Name("StreamingSoftwareOBSSLOBS")]
        StreamingSoftware,
        Streamlabs,
        Command,
        Serial,
        Moderation,
        OvrStream,
        IFTTT,
        Twitch,
    }

    [DataContract]
    public class CommunityCommandModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ImageURL { get; set; }

        [DataMember]
        public HashSet<CommunityCommandTagEnum> Tags { get; set; } = new HashSet<CommunityCommandTagEnum>();

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string UserAvatarURL { get; set; }

        [DataMember]
        public double AverageRating { get; set; }

        [DataMember]
        public int Downloads { get; set; }

        [DataMember]
        public DateTimeOffset LastUpdated { get; set; }
    }

    [DataContract]
    public class CommunityCommandDetailsModel : CommunityCommandModel
    {
        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public List<CommunityCommandReviewModel> Reviews { get; set; } = new List<CommunityCommandReviewModel>();

        public CommandModelBase GetCommand()
        {
            try
            {
                return JSONSerializerHelper.DeserializeFromString<CommandModelBase>(this.Data);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public void SetCommand(CommandModelBase command)
        {
            try
            {
                this.Data = JSONSerializerHelper.SerializeToString(command);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
