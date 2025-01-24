using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum ActionTypeEnum
    {
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
        Group,
        Repeat,
        VTSPog,
        MtionStudio = 42,
        MeldStudio,
    }

    [DataContract]
    public abstract class ActionModelBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ActionTypeEnum Type { get; set; }

        [DataMember]
        public bool Enabled { get; set; } = true;

        public ActionModelBase(ActionTypeEnum type)
        {
            this.ID = Guid.NewGuid();
            this.Type = type;
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.Type);
            this.Enabled = true;
        }

        [Obsolete]
        public ActionModelBase() { }

        public virtual async Task TestPerform(Dictionary<string, string> specialIdentifiers)
        {
            await this.Perform(new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.All, new List<string>() { "@" + ChannelSession.User.Username }, specialIdentifiers) { TargetUser = ChannelSession.User });
        }

        public async Task Perform(CommandParametersModel parameters)
        {
            if (this.Enabled)
            {
                Logger.Log(LogLevel.Debug, $"Starting action performing: {this}");

                ServiceManager.Get<ITelemetryService>().TrackAction(this.Type);

                await this.PerformInternal(parameters);
            }
        }

        protected abstract Task PerformInternal(CommandParametersModel parameters);

        protected static async Task<string> ReplaceStringWithSpecialModifiers(string str, CommandParametersModel parameters, bool encode = false)
        {
            return await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(str, parameters, encode);
        }

        public override string ToString() { return string.Format("{0} - {1}", this.ID, this.Name); }

        public int CompareTo(object obj)
        {
            if (obj is ActionModelBase)
            {
                return this.CompareTo((ActionModelBase)obj);
            }
            return 0;
        }

        public int CompareTo(ActionModelBase other) { return this.Name.CompareTo(other.Name); }

        public override bool Equals(object obj)
        {
            if (obj is ActionModelBase)
            {
                return this.Equals((ActionModelBase)obj);
            }
            return false;
        }

        public bool Equals(ActionModelBase other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }
}
