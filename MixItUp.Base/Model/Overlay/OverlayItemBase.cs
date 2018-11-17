using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public abstract class OverlayItemBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public virtual string ItemType { get { return string.Empty; } }

        [JsonIgnore]
        public bool IsInitialized { get; private set; }

        public OverlayItemBase()
        {
            this.ID = Guid.NewGuid();
        }

        public abstract Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);

        public virtual Task Initialize()
        {
            this.IsInitialized = true;
            return Task.FromResult(0);
        }

        public T Copy<T>()
        {
            JObject jobj = JObject.FromObject(this);
            return jobj.ToObject<T>();
        }

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, bool encode = false)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str, Guid.NewGuid(), encode);
            if (extraSpecialIdentifiers != null)
            {
                foreach (var kvp in extraSpecialIdentifiers)
                {
                    siString.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
                }
            }
            await siString.ReplaceCommonSpecialModifiers(user, arguments);
            return siString.ToString();
        }
    }
}
