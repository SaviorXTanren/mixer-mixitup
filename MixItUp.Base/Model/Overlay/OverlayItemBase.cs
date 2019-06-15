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
        public string ItemType { get; set; }

        [JsonIgnore]
        public bool IsInitialized { get; private set; }

        public OverlayItemBase()
        {
            this.ID = Guid.NewGuid();
        }

        public OverlayItemBase(string itemType)
            : this()
        {
            this.ItemType = itemType;
        }

        [JsonIgnore]
        public virtual bool SupportsTestButton { get { return false; } }

        public virtual Task LoadTestData() { return Task.FromResult(0); }

        public abstract Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);

        public virtual Task Initialize()
        {
            this.IsInitialized = true;
            return Task.FromResult(0);
        }

        public virtual Task Disable()
        {
            this.IsInitialized = false;
            return Task.FromResult(0);
        }

        public T Copy<T>() { return SerializerHelper.DeserializeFromString<T>(SerializerHelper.SerializeToString(this)); }

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
