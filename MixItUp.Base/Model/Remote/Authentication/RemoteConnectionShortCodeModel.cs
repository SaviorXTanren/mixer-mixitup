using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote.Authentication
{
    [DataContract]
    public class RemoteConnectionShortCodeModel : RemoteConnectionModel
    {
        [DataMember]
        public string ShortCode { get; set; }

        [DataMember]
        public DateTimeOffset ShortCodeExpiration { get; set; }

        [JsonIgnore]
        public bool Approved { get; set; }

        public RemoteConnectionShortCodeModel() { }

        public RemoteConnectionShortCodeModel(string name) : base(name) { }

        [JsonIgnore]
        public bool IsShortCodeExpired { get { return DateTimeOffset.Now > this.ShortCodeExpiration; } }

        public void GenerateShortCode()
        {
            this.ShortCode = String.Format("{0:000000}", RandomHelper.GenerateRandomNumber(1000, 999999));
            this.ShortCodeExpiration = DateTimeOffset.Now.AddMinutes(1);
        }
    }
}
