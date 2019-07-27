using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum ActionTypeEnum
    {
        Chat,
        [Name("Currency/Rank/Inventory")]
        Currency,
        [Name("External Program")]
        ExternalProgram,
        Input,
        Overlay,
        Sound,
        Wait,
        [Name("OBS Studio")]
        [Obsolete]
        OBSStudio,
        [Obsolete]
        XSplit,
        Counter,
        [Name("Game Queue")]
        GameQueue,
        [Name("MixPlay")]
        Interactive,
        [Name("Text To Speech")]
        TextToSpeech,
        [Obsolete]
        Rank,
        [Name("Web Request")]
        WebRequest,
        [Obsolete]
        [Name("Action Group")]
        ActionGroup,
        [Name("Special Identifier")]
        SpecialIdentifier,
        File,
        [Name("Song Request")]
        SongRequest,
        Spotify,
        Discord,
        Translation,
        Twitter,
        Conditional,
        [Name("Streamlabs OBS")]
        [Obsolete]
        StreamlabsOBS,
        [Name("Streaming Software")]
        StreamingSoftware,
        Streamlabs,
        [Name("Mixer Clips")]
        MixerClips,
        Command,
        Serial,
        Moderation,
        OvrStream,
        [Name("Streaming Platform")]
        StreamingPlatform,
        IFTTT,

        Custom = 99,
    }

    [DataContract]
    public abstract class ActionBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public ActionTypeEnum Type { get; set; }

        [DataMember]
        public string Label { get; set; }

        [JsonIgnore]
        protected Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();

        public ActionBase()
        {
            this.ID = Guid.NewGuid();
        }

        public ActionBase(ActionTypeEnum type)
            : this()
        {
            this.Type = type;
            this.Label = EnumHelper.GetEnumName(this.Type);
        }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            await this.AsyncSemaphore.WaitAndRelease(async () =>
            {
                this.extraSpecialIdentifiers = extraSpecialIdentifiers;

                ChannelSession.Services.Telemetry.TrackAction(this.Type);

                await this.PerformInternal(user, arguments);
            });
        }

        protected abstract Task PerformInternal(UserViewModel user, IEnumerable<string> arguments);

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments, bool encode = false)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str, encode);
            foreach (var kvp in this.extraSpecialIdentifiers)
            {
                siString.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
            }
            await siString.ReplaceCommonSpecialModifiers(user, arguments);
            return siString.ToString();
        }

        protected Dictionary<string, string> GetExtraSpecialIdentifiers() { return this.extraSpecialIdentifiers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); }

        protected abstract SemaphoreSlim AsyncSemaphore { get; }
    }
}
