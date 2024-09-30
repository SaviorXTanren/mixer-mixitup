using System;
using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class UserPlatformData
    {
        public string Platform { get; set; }
        public string ID { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string AvatarLink { get; set; }
        public string SubscriberBadgeLink { get; set; }
        public string RoleBadgeLink { get; set; }
        public string SpecialtyBadgeLink { get; set; }
        public HashSet<string> Roles { get; set; } = new HashSet<string>();
        public DateTimeOffset? AccountDate { get; set; }
        public DateTimeOffset? FollowDate { get; set; }
        public DateTimeOffset? SubscribeDate { get; set; }
        public int SubscriberTier { get; set; } = 0;
    }
}
