using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    [DataContract]
    public class UserTitleModel : IEquatable<UserTitleModel>, IComparable<UserTitleModel>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public MixerRoleEnum Role { get; set; }

        [DataMember]
        public int Months { get; set; }

        [JsonProperty]
        public string RoleString { get { return EnumHelper.GetEnumName(this.Role); } }

        [JsonProperty]
        public string MonthsString { get { return (this.Months > 0) ? this.Months.ToString() : string.Empty; } }

        public UserTitleModel() { }

        public UserTitleModel(string name, MixerRoleEnum role, int months = 0)
        {
            this.Name = name;
            this.Role = role;
            this.Months = months;
        }

        public bool MeetsTitle(UserViewModel user)
        {
            if (user.HasPermissionsTo(this.Role))
            {
                if (this.Role == MixerRoleEnum.Follower)
                {
                    if (user.FollowDate != null)
                    {
                        return user.FollowDate.GetValueOrDefault().TotalMonthsFromNow() >= this.Months;
                    }
                    else if (!user.ExceedsPermissions(this.Role))
                    {
                        return false;
                    }
                }
                else if (this.Role == MixerRoleEnum.Subscriber)
                {
                    if (user.MixerSubscribeDate != null)
                    {
                        return user.MixerSubscribeDate.GetValueOrDefault().TotalMonthsFromNow() >= this.Months;
                    }
                    else if (user.IsEquivalentToMixerSubscriber() && this.Months == 1)
                    {
                        return true;
                    }
                    else if (!user.ExceedsPermissions(this.Role))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is UserTitleModel)
            {
                return this.Equals((UserTitleModel)obj);
            }
            return false;
        }

        public bool Equals(UserTitleModel other) { return this.Name.Equals(other.Name); }

        public int CompareTo(UserTitleModel other)
        {
            if (this.Role == other.Role)
            {
                if (this.Months < other.Months)
                {
                    return -1;
                }
                else if (this.Months > other.Months)
                {
                    return 1;
                }
                return 0;
            }
            return this.Role.CompareTo(other.Role);
        }

        public override int GetHashCode() { return this.Name.GetHashCode(); }
    }
}
