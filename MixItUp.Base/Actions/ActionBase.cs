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
        [Name("Currency/Rank")]
        Currency,
        [Name("External Program")]
        ExternalProgram,
        Input,
        Overlay,
        Sound,
        Wait,
        [Name("OBS Studio")]
        OBSStudio,
        XSplit,
        Counter,
        [Name("Game Queue")]
        GameQueue,
        Interactive,
        [Name("Text To Speech")]
        TextToSpeech,
        [Obsolete]
        Rank,
        [Name("Web Request")]
        WebRequest,
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
        StreamlabsOBS,

        Custom = 99,
    }

    [DataContract]
    public abstract class ActionBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public ActionTypeEnum Type { get; set; }

        [JsonIgnore]
        private Dictionary<string, string> additiveSpecialIdentifiers = new Dictionary<string, string>();
        [JsonIgnore]
        private Guid randomUserSpecialIdentifierGroup = Guid.Empty;

        public ActionBase()
        {
            this.ID = Guid.NewGuid();
        }

        public ActionBase(ActionTypeEnum type)
            : this()
        {
            this.Type = type;
        }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            await this.AsyncSemaphore.WaitAsync();

            try
            {
                await this.PerformInternal(user, arguments);
            }
            catch (Exception ex) { Util.Logger.Log(ex); }
            finally { this.AsyncSemaphore.Release(); }

            this.additiveSpecialIdentifiers.Clear();
        }

        public void AddSpecialIdentifier(string specialIdentifier, string replacement)
        {
            this.additiveSpecialIdentifiers[specialIdentifier] = replacement;
        }

        public void AssignRandomUserSpecialIdentifierGroup(Guid id) { this.randomUserSpecialIdentifierGroup = id; }

        protected abstract Task PerformInternal(UserViewModel user, IEnumerable<string> arguments);

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments, bool encode = false)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str, this.randomUserSpecialIdentifierGroup, encode);
            await siString.ReplaceCommonSpecialModifiers(user, arguments);
            foreach (var kvp in this.additiveSpecialIdentifiers)
            {
                siString.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
            }
            return siString.ToString();
        }

        protected IDictionary<string, string> GetAdditiveSpecialIdentifiers() { return this.additiveSpecialIdentifiers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); }

        protected abstract SemaphoreSlim AsyncSemaphore { get; }
    }
}
