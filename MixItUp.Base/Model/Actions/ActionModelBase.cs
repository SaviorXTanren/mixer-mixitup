using LinqToTwitter;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
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
        Translation,
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
        public bool IsEnabled { get; set; }

        public ActionModelBase(ActionTypeEnum type)
        {
            this.ID = Guid.NewGuid();
            this.Type = type;
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.Type);
        }

        [JsonIgnore]
        protected abstract SemaphoreSlim AsyncSemaphore { get; }

        public async Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.IsEnabled)
            {
                await this.AsyncSemaphore.WaitAndRelease(async () =>
                {
                    Logger.Log(LogLevel.Debug, $"Starting action performing: {this}");

                    ChannelSession.Services.Telemetry.TrackAction(this.Type);

                    await this.PerformInternal(user, platform, arguments, specialIdentifiers);
                });
            }
        }

        protected abstract Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers);

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers, bool encode = false)
        {
            return await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(str, user, platform, arguments, specialIdentifiers, encode);
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
