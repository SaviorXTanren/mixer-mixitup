using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Import
{
    [DataContract]
    public class SoundwaveButton
    {
        [DataMember]
        public string id { get; set; }
        
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string path { get; set; }

        [DataMember]
        public int cooldown { get; set; }

        [DataMember]
        public int sparks { get; set; }

        [DataMember]
        public int volume { get; set; }
    }
}
