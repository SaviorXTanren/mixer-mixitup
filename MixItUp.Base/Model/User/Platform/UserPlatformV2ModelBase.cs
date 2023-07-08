using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public abstract class UserPlatformV2ModelBase : IEquatable<UserPlatformV2ModelBase>, IComparable<UserPlatformV2ModelBase>
    {
        public static readonly TimeSpan DefaultRefreshTimeSpan = TimeSpan.FromMinutes(5);

        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; }
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string AvatarLink { get; set; }
        [DataMember]
        public string SubscriberBadgeLink { get; set; }
        [DataMember]
        public string RoleBadgeLink { get; set; }
        [DataMember]
        public string SpecialtyBadgeLink { get; set; }

        [DataMember]
        public HashSet<UserRoleEnum> Roles { get; set; } = new HashSet<UserRoleEnum>() { UserRoleEnum.User };

        [DataMember]
        public DateTimeOffset? AccountDate { get; set; }
        [DataMember]
        public DateTimeOffset? FollowDate { get; set; }
        [DataMember]
        public DateTimeOffset? SubscribeDate { get; set; }
        [DataMember]
        public int SubscriberTier { get; set; } = 1;

        protected UserPlatformV2ModelBase() { }

        public virtual TimeSpan RefreshTimeSpan { get { return DefaultRefreshTimeSpan; } }

        public abstract Task Refresh();

        public override bool Equals(object obj)
        {
            if (obj is UserPlatformV2ModelBase)
            {
                return this.Equals((UserPlatformV2ModelBase)obj);
            }
            return false;
        }

        public bool Equals(UserPlatformV2ModelBase other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public int CompareTo(UserPlatformV2ModelBase other) { return this.Username.CompareTo(other.Username); }
    }

    [DataContract]
    public class UnassociatedUserPlatformV2Model : UserPlatformV2ModelBase
    {
        public UnassociatedUserPlatformV2Model(string username)
        {
            this.Platform = StreamingPlatformTypeEnum.None;
            this.ID = Guid.Empty.ToString();
            this.Username = this.DisplayName = username;
        }

        [Obsolete]
        public UnassociatedUserPlatformV2Model() { }

        public override Task Refresh() { return Task.CompletedTask; }
    }
}
