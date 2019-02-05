using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote.Authentication
{
    [DataContract]
    public class RemoteConnectionAuthenticationTokenModel : RemoteConnectionShortCodeModel
    {
        [DataMember]
        public string AccessToken { get; set; }

        [DataMember]
        public DateTimeOffset AccessTokenExpiration { get; set; }

        [JsonIgnore]
        public Guid GroupID { get; set; }

        public RemoteConnectionAuthenticationTokenModel() { }

        public RemoteConnectionAuthenticationTokenModel(RemoteConnectionModel device)
            : base(device.Name)
        {
            this.ID = device.ID;
        }

        public RemoteConnectionAuthenticationTokenModel(RemoteConnectionModel device, Guid groupID)
            : this(device)
        {
            this.GroupID = groupID;
        }

        public void GenerateAccessToken(bool neverExpire)
        {
            this.AccessToken = string.Format("{0}-{1}", Guid.NewGuid(), Guid.NewGuid());
            this.AccessTokenExpiration = (neverExpire) ? DateTimeOffset.MaxValue : DateTimeOffset.Now.AddSeconds(30);
        }
    }
}
