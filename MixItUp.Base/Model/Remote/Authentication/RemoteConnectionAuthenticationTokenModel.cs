using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public string HostName { get; set; }

        [DataMember]
        public bool IsHost { get; set; }

        [JsonIgnore]
        public Guid GroupID { get; set; }

        public RemoteConnectionAuthenticationTokenModel() { }

        public RemoteConnectionAuthenticationTokenModel(RemoteConnectionModel connection)
            : base(connection.Name)
        {
            this.ID = connection.ID;
            this.HostName = connection.Name;
        }

        public RemoteConnectionAuthenticationTokenModel(RemoteConnectionModel connection, RemoteConnectionAuthenticationTokenModel hostConnection)
            : this(connection)
        {
            if (hostConnection != null)
            {
                this.GroupID = hostConnection.GroupID;
                this.HostName = hostConnection.Name;
            }
            else
            {
                this.GroupID = Guid.NewGuid();
            }
        }

        public RemoteConnectionAuthenticationTokenModel(Dictionary<string, object> databaseValues)
        {
            this.ID = (Guid)databaseValues["ID"];
            this.Name = (string)databaseValues["Name"];
            this.IsHost = (bool)databaseValues["IsHost"];
            this.IsStreamer = (bool)databaseValues["IsStreamer"];
            this.GroupID = (Guid)databaseValues["GroupID"];
            this.AccessToken = (string)databaseValues["AccessToken"];
            this.AccessTokenExpiration = (DateTimeOffset)databaseValues["AccessTokenExpiration"];
        }

        [JsonIgnore]
        public bool IsAccessTokenExpired { get { return DateTimeOffset.Now > this.AccessTokenExpiration; } }

        public void GenerateAccessToken(bool neverExpire)
        {
            this.AccessToken = string.Format("{0}-{1}", Guid.NewGuid(), Guid.NewGuid());
            this.AccessTokenExpiration = (neverExpire) ? DateTimeOffset.MaxValue : DateTimeOffset.Now.AddSeconds(30);
            this.IsTemporary = !neverExpire;
        }
    }
}
