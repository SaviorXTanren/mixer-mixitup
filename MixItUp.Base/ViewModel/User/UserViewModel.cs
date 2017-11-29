using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.ScorpBot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public enum UserRole
    {
        Banned,
        User,
        Pro,
        Follower,
        Subscriber,
        Mod,
        Staff,
        Streamer,

        Custom = 99,
    }

    [DataContract]
    public class UserViewModel : IEquatable<UserViewModel>, IComparable<UserViewModel>
    {
        private const string DefaultAvatarLink = "https://mixer.com/_latest/assets/images/main/avatars/default.jpg";

        public static IEnumerable<UserRole> SelectableUserRoles()
        {
            List<UserRole> roles = new List<UserRole>(EnumHelper.GetEnumList<UserRole>());
            roles.Remove(UserRole.Banned);
            roles.Remove(UserRole.Custom);
            return roles;
        }

        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public double ViewingHours { get; set; }

        [DataMember]
        public long RankPoints { get; set; }

        [DataMember]
        public long CurrencyAmount { get; set; }

        [DataMember]
        public string AvatarLink { get; set; }

        [DataMember]
        public DateTimeOffset? MixerAccountDate { get; set; }

        [DataMember]
        public DateTimeOffset? FollowDate { get; set; }

        [JsonIgnore]
        public HashSet<UserRole> Roles { get; set; }

        [JsonIgnore]
        public int ChatOffenses { get; set; }

        [JsonIgnore]
        public int Sparks { get; set; }

        public UserViewModel()
        {
            this.Roles = new HashSet<UserRole>();
            this.AvatarLink = DefaultAvatarLink;
            this.SetFollowDate(this.FollowDate);

            this.ViewingHours = 0.0;
            this.RankPoints = 0;
            this.CurrencyAmount = 0;
        }

        public UserViewModel(UserModel user) : this(user.id, user.username)
        {
            this.AvatarLink = user.avatarUrl;
            this.MixerAccountDate = user.createdAt;
        }

        public UserViewModel(ChatUserModel user) : this(user.userId.GetValueOrDefault(), user.userName, user.userRoles) { }

        public UserViewModel(ChatUserEventModel userEvent) : this(userEvent.id, userEvent.username, userEvent.roles) { }

        public UserViewModel(ChatMessageEventModel messageEvent) : this(messageEvent.user_id, messageEvent.user_name, messageEvent.user_roles) { }

        public UserViewModel(ScorpBotViewer viewer)
            : this(viewer.ID, viewer.UserName)
        {
            this.ViewingHours = viewer.Hours;
            this.RankPoints = viewer.RankPoints;
            this.CurrencyAmount = viewer.Currency;
        }

        public UserViewModel(uint id, string username) : this(id, username, new string[] { }) { }

        public UserViewModel(uint id, string username, string[] userRoles)
            : this()
        {
            this.ID = id;
            this.UserName = username;

            this.Roles.Add(UserRole.User);
            if (userRoles.Any(r => r.Equals("Owner"))) { this.Roles.Add(UserRole.Streamer); }
            if (userRoles.Any(r => r.Equals("Staff"))) { this.Roles.Add(UserRole.Staff); }
            if (userRoles.Any(r => r.Equals("Mod"))) { this.Roles.Add(UserRole.Mod); }
            if (userRoles.Any(r => r.Equals("Subscriber"))) { this.Roles.Add(UserRole.Subscriber); }
            if (userRoles.Any(r => r.Equals("Pro"))) { this.Roles.Add(UserRole.Pro); }
            if (userRoles.Any(r => r.Equals("Banned"))) { this.Roles.Add(UserRole.Banned); }
        }

        [JsonIgnore]
        public string RolesDisplayString
        {
            get
            {
                List<UserRole> displayRoles = this.Roles.ToList();
                if (this.Roles.Contains(UserRole.Banned))
                {
                    displayRoles.Clear();
                    displayRoles.Add(UserRole.Banned);
                }
                else
                {
                    if (this.Roles.Count() > 1)
                    {
                        displayRoles.Remove(UserRole.User);
                    }

                    if (this.Roles.Contains(UserRole.Subscriber) || this.Roles.Contains(UserRole.Streamer))
                    {
                        displayRoles.Remove(UserRole.Follower);
                    }

                    if (this.Roles.Contains(UserRole.Streamer))
                    {
                        displayRoles.Remove(UserRole.Subscriber);
                        displayRoles.Remove(UserRole.Mod);
                    }
                }
                return string.Join(", ", displayRoles.OrderByDescending(r => r).Select(r => EnumHelper.GetEnumName(r)));
            }
        }

        [JsonIgnore]
        public UserRole PrimaryRole { get { return this.Roles.Max(); } }

        [JsonIgnore]
        public UserRole PrimarySortableRole { get { return this.Roles.Where(r => r != UserRole.Follower).Max(); } }

        [JsonIgnore]
        public string MixerAgeString { get { return (this.MixerAccountDate != null) ? this.MixerAccountDate.GetValueOrDefault().GetAge() : "Unknown"; } }

        [JsonIgnore]
        public bool IsFollower { get { return this.Roles.Contains(UserRole.Follower); } }

        [JsonIgnore]
        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : "Not Following"; } }

        [JsonIgnore]
        public bool IsSubscriber { get { return this.Roles.Contains(UserRole.Subscriber); } }

        [JsonIgnore]
        public string PrimaryRoleColor
        {
            get
            {
                if (this.Roles.Contains(UserRole.Streamer))
                {
                    return "#FF000000";
                }
                else if (this.Roles.Contains(UserRole.Staff))
                {
                    return "#FFFFD700";
                }
                else if (this.Roles.Contains(UserRole.Mod))
                {
                    return "#FF008000";
                }
                else if (this.Roles.Contains(UserRole.Pro))
                {
                    return "#FF800080";
                }
                else
                {
                    return "#FF0000FF";
                }
            }
        }

        [JsonIgnore]
        public string RankNameAndPoints
        {
            get
            {
                UserRankViewModel rank = null;
                if (ChannelSession.Settings.Ranks.Count > 0)
                {
                    rank = ChannelSession.Settings.Ranks.Where(r => r.MinimumPoints <= this.RankPoints).OrderByDescending(r => r.MinimumPoints).FirstOrDefault();
                }
                return string.Format("{0} - {1}", (rank != null) ? rank.Name : "No Rank", this.RankPoints);
            }
        }

        public async Task SetDetails(bool checkForFollow = true)
        {
            if (this.ID > 0)
            {
                UserWithChannelModel userWithChannel = await ChannelSession.Connection.GetUser(this.GetModel());
                if (!string.IsNullOrEmpty(userWithChannel.avatarUrl))
                {
                    this.AvatarLink = userWithChannel.avatarUrl;
                }
                this.MixerAccountDate = userWithChannel.createdAt;
                this.Sparks = (int)userWithChannel.sparks;
            }

            if (checkForFollow)
            {
                DateTimeOffset? followDate = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, this.GetModel());
                this.SetFollowDate(followDate);
            }
        }

        public void SetFollowDate(DateTimeOffset? followDate)
        {
            this.FollowDate = followDate;
            if (this.FollowDate != null && this.FollowDate.GetValueOrDefault() != DateTimeOffset.MinValue)
            {
                this.Roles.Add(UserRole.Follower);
            }
            else
            {
                this.Roles.Remove(UserRole.Follower);
            }
        }

        public UserModel GetModel()
        {
            return new UserModel()
            {
                id = this.ID,
                username = this.UserName,
            };
        }

        public ChatUserModel GetChatModel()
        {
            return new ChatUserModel()
            {
                userId = this.ID,
                userName = this.UserName,
                userRoles = this.Roles.Select(r => r.ToString()).ToArray(),
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is UserViewModel)
            {
                return this.Equals((UserViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserViewModel other) { return this.ID.Equals(other.ID); }

        public int CompareTo(UserViewModel other)
        {
            int order = this.PrimaryRole.CompareTo(other.PrimaryRole);
            if (order == 0)
            {
                return this.UserName.CompareTo(other.UserName);
            }
            return order;
        }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.UserName; }
    }
}
