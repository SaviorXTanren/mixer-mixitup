using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote.Authentication
{
    [DataContract]
    public class RemoteDeviceAuthenticationTokenModel : RemoteDeviceModel
    {
        [DataMember]
        public string AccessToken { get; set; }

        [DataMember]
        public DateTimeOffset AccessTokenExpiration { get; set; }

        public RemoteDeviceAuthenticationTokenModel() { }

        public RemoteDeviceAuthenticationTokenModel(RemoteDeviceModel device, bool neverExpire = false)
        {
            this.ID = device.ID;
            this.Name = device.Name;
            this.GroupID = device.GroupID;
            this.AccessToken = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
            this.AccessTokenExpiration = (neverExpire) ? DateTimeOffset.MaxValue : DateTimeOffset.Now.AddSeconds(30);
        }
    }
}
