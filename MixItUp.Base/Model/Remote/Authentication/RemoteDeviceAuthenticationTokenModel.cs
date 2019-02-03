using Newtonsoft.Json;
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

        [JsonIgnore]
        public Guid GroupID { get; set; }

        public RemoteDeviceAuthenticationTokenModel() { }

        public RemoteDeviceAuthenticationTokenModel(RemoteDeviceModel device, Guid groupID, bool neverExpire = false)
        {
            this.ID = device.ID;
            this.Name = device.Name;
            this.GroupID = groupID;
            this.AccessToken = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
            this.AccessTokenExpiration = (neverExpire) ? DateTimeOffset.MaxValue : DateTimeOffset.Now.AddSeconds(30);
        }
    }
}
