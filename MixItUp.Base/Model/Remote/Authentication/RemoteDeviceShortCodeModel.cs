using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote.Authentication
{
    [DataContract]
    public class RemoteDeviceShortCodeModel : RemoteDeviceModel
    {
        [DataMember]
        public string ShortCode { get; set; }

        [DataMember]
        public DateTimeOffset ShortCodeExpiration { get; set; }

        public RemoteDeviceShortCodeModel() { }

        public RemoteDeviceShortCodeModel(string name)
            : base(name)
        {
            this.ShortCode = String.Format("{0:000000}", RandomHelper.GenerateRandomNumber(1000, 999999));
            this.ShortCodeExpiration = DateTimeOffset.Now.AddMinutes(1);
        }
    }
}
