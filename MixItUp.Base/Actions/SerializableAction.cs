using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class SerializableAction
    {
        public SerializableAction()
        {
            this.Values = new List<string>();
        }

        [DataMember]
        public ActionTypeEnum Type { get; set; }

        [DataMember]
        public List<string> Values { get; set; }
    }
}
