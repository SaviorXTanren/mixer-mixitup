using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    public enum CommunityCommandTagEnum
    {
        // Actions Tags
        Custom = 0,
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
        [Obsolete]
        Translation,
        [Obsolete]
        Twitter,
        Conditional,
        StreamingSoftware,
        Streamlabs,
        Command,
        Serial,
        Moderation,
        OvrStream,
        IFTTT,
        Twitch,
        PixelChat,
        VTubeStudio,
        Voicemod,
        YouTube,
        Trovo,
        PolyPop,
        SAMMI,
        InfiniteAlbum,
        TITS,
        MusicPlayer,
        LumiaStream,
        Random,
        Script,

        // Command Tags
        ChatCommand = 1000,
        EventCommand,
        TimerCommand,
        ActionGroupCommand,
        StreamlootsCardCommand,
        TwitchChannelPointsCommand,
        GameCommand,
        Webhook,
        TrovoSpell,
        TwitchBits,
        CrowdControlEffect,

        // Extra Tags
        [Obsolete]
        Stuff = 100000,
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
        public string ScreenshotURL { get; set; }

        [DataMember]
        public HashSet<CommunityCommandTagEnum> Tags { get; set; } = new HashSet<CommunityCommandTagEnum>();

        [DataMember]
        public Guid UserId { get; set; }

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

        public List<CommandModelBase> GetCommands()
        {
            try
            {
                return JSONSerializerHelper.DeserializeFromString<List<CommandModelBase>>(this.Data);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public void SetCommands(IEnumerable<CommandModelBase> commands)
        {
            try
            {
                this.Data = JSONSerializerHelper.SerializeToString(commands);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }

    [DataContract]
    public class CommunityCommandUploadModel
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
        public string ScreenshotURL { get; set; }

        [DataMember]
        public HashSet<CommunityCommandTagEnum> Tags { get; set; } = new HashSet<CommunityCommandTagEnum>();

        [DataMember]
        public byte[] ImageFileData { get; set; }

        [DataMember]
        public byte[] ScreenshotFileData { get; set; }

        [DataMember]
        public string Data { get; set; }

        public void SetCommands(IEnumerable<CommandModelBase> commands)
        {
            try
            {
                this.Data = JSONSerializerHelper.SerializeToString(commands);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
