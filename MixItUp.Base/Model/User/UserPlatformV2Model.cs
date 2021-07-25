using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    [DataContract]
    public class UserPlatformV2Model
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string AvatarLink { get; set; }

        [DataMember]
        public HashSet<UserRoleEnum> Roles { get; set; } = new HashSet<UserRoleEnum>() { UserRoleEnum.User };

        [DataMember]
        public DateTimeOffset? AccountDate { get; set; }
        [DataMember]
        public DateTimeOffset? FollowDate { get; set; }
        [DataMember]
        public DateTimeOffset? SubscribeDate { get; set; }
        [DataMember]
        public int SubscriberTier { get; set; } = 0;
    }

    [DataContract]
    public class TwitchUserPlatformV2Model : UserPlatformV2Model
    {
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public Dictionary<string, int> Badges { get; set; } = new Dictionary<string, int>();
        [DataMember]
        public Dictionary<string, int> BadgeInfo { get; set; } = new Dictionary<string, int>();
    }

    [DataContract]
    public class YouTubeUserPlatformV2Model : UserPlatformV2Model
    {
        [DataMember]
        public string YouTubeURL { get; set; }
    }

    [DataContract]
    public class GlimeshUserPlatformV2Model : UserPlatformV2Model
    {
    }

    [DataContract]
    public class TrovoUserPlatformV2Model : UserPlatformV2Model
    {
    }
}
