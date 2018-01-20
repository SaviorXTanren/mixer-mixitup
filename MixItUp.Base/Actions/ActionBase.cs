using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private Dictionary<string, string> AdditiveSpecialIdentifiers = new Dictionary<string, string>();

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
            catch (Exception ex) { Logger.Log(ex); }
            finally { this.AsyncSemaphore.Release(); }
        }

        public void AddSpecialIdentifier(string specialIdentifier, string replacement)
        {
            this.AdditiveSpecialIdentifiers[specialIdentifier] = replacement;
        }

        protected abstract Task PerformInternal(UserViewModel user, IEnumerable<string> arguments);

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str);
            await siString.ReplaceCommonSpecialModifiers(user, arguments);
            foreach (var kvp in this.AdditiveSpecialIdentifiers)
            {
                siString.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
            }
            return siString.ToString();
        }

        protected abstract SemaphoreSlim AsyncSemaphore { get; }
    }
}
