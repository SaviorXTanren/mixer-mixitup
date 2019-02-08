using MixItUp.Base.Util;
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

        [DataMember]
        public bool Approved { get; set; }

        public RemoteConnectionShortCodeModel() { }

        public RemoteConnectionShortCodeModel(string name) : base(name) { }

        public void GenerateShortCode()
        {
            this.ShortCode = String.Format("{0:000000}", RandomHelper.GenerateRandomNumber(1000, 999999));
            this.ShortCodeExpiration = DateTimeOffset.Now.AddMinutes(1);
        }
    }
}
