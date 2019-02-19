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

        [DataMember]
        public bool IsHost { get; set; }

        [JsonIgnore]
        public Guid GroupID { get; set; }

        public RemoteConnectionAuthenticationTokenModel() { }

        public RemoteConnectionAuthenticationTokenModel(RemoteConnectionModel connection)
            : base(connection.Name)
        {
            this.ID = connection.ID;
        }

        public RemoteConnectionAuthenticationTokenModel(RemoteConnectionModel connection, Guid groupID)
            : this(connection)
        {
            this.GroupID = groupID;
        }

        [JsonIgnore]
        public bool IsAccessTokenExpired { get { return DateTimeOffset.Now > this.AccessTokenExpiration; } }

        public void GenerateAccessToken(bool neverExpire)
        {
            this.AccessToken = string.Format("{0}-{1}", Guid.NewGuid(), Guid.NewGuid());
            this.AccessTokenExpiration = (neverExpire) ? DateTimeOffset.MaxValue : DateTimeOffset.Now.AddSeconds(30);
        }
    }
}
