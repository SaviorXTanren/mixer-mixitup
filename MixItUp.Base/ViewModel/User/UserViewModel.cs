using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Util;
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
        public const string DefaultAvatarLink = "https://mixer.com/_latest/assets/images/main/avatars/default.png";

        public static IEnumerable<UserRole> SelectableUserRoles()
        {
            List<UserRole> roles = new List<UserRole>(EnumHelper.GetEnumList<UserRole>());
            roles.Remove(UserRole.Banned);
            roles.Remove(UserRole.Custom);
            return roles;
        }

        public uint ID { get; set; }

        public string UserName { get; set; }

        public string AvatarLink { get; set; }

        public uint GameTypeID { get; set; }

        public DateTimeOffset? MixerAccountDate { get; set; }

        public DateTimeOffset? FollowDate { get; set; }

        public DateTimeOffset? SubscribeDate { get; set; }

        public HashSet<UserRole> Roles { get; set; }

        public int ChatOffenses { get; set; }

        public int Sparks { get; set; }

        public UserViewModel()
        {
            this.Roles = new HashSet<UserRole>();
            this.AvatarLink = UserViewModel.DefaultAvatarLink;
        }

        public UserViewModel(UserModel user) : this(user.id, user.username)
        {
            this.AvatarLink = (!string.IsNullOrEmpty(user.avatarUrl)) ? user.avatarUrl : UserViewModel.DefaultAvatarLink;
            this.MixerAccountDate = user.createdAt;
        }

        public UserViewModel(ChannelModel channel) : this(channel.id, channel.token) { }

        public UserViewModel(ChatUserModel user) : this(user.userId.GetValueOrDefault(), user.userName, user.userRoles) { }

        public UserViewModel(ChatUserEventModel userEvent) : this(userEvent.id, userEvent.username, userEvent.roles) { }

        public UserViewModel(ChatMessageEventModel messageEvent) : this(messageEvent.user_id, messageEvent.user_name, messageEvent.user_roles) { }

        public UserViewModel(InteractiveParticipantModel participant) : this(participant.userID, participant.username) { }

        public UserViewModel(uint id, string username) : this(id, username, new string[] { }) { }

        public UserViewModel(uint id, string username, string[] userRoles)
            : this()
        {
            this.ID = id;
            this.UserName = username;

            this.Roles.Add(UserRole.User);
            if (userRoles != null)
            {
                if (userRoles.Any(r => r.Equals("Owner"))) { this.Roles.Add(UserRole.Streamer); }
                if (userRoles.Any(r => r.Equals("Staff"))) { this.Roles.Add(UserRole.Staff); }
                if (userRoles.Any(r => r.Equals("Mod"))) { this.Roles.Add(UserRole.Mod); }
                if (userRoles.Any(r => r.Equals("Subscriber"))) { this.Roles.Add(UserRole.Subscriber); }
                if (userRoles.Any(r => r.Equals("Pro"))) { this.Roles.Add(UserRole.Pro); }
                if (userRoles.Any(r => r.Equals("Banned"))) { this.Roles.Add(UserRole.Banned); }
            }

            if (this.Roles.Contains(UserRole.Streamer))
            {
                this.Roles.Add(UserRole.Subscriber);
                this.Roles.Add(UserRole.Mod);
            }

            if (this.IsSubscriber)
            {
                this.Roles.Add(UserRole.Follower);
            }
        }

        public UserDataViewModel Data { get { return ChannelSession.Settings.UserData.GetValueIfExists(this.ID, new UserDataViewModel(this)); } }

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
        public bool IsFollower { get { return this.Roles.Contains(UserRole.Follower) || this.Roles.Contains(UserRole.Streamer); } }

        [JsonIgnore]
        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : "Not Following"; } }

        [JsonIgnore]
        public bool IsSubscriber { get { return this.Roles.Contains(UserRole.Subscriber) || this.Roles.Contains(UserRole.Streamer); } }

        [JsonIgnore]
        public string SubscribeAgeString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().GetAge() : "Not Subscribed"; } }

        [JsonIgnore]
        public int SubscribeMonths
        {
            get
            {
                int subMonths = 0;
                if (this.SubscribeDate != null)
                {
                    TimeSpan span = DateTimeOffset.Now.Date - this.SubscribeDate.GetValueOrDefault().Date;
                    double totalMonths = ((double)span.Days) / 30.0;

                    subMonths = (int)totalMonths;
                    totalMonths = totalMonths - subMonths;
                    if (totalMonths > 0.0)
                    {
                        subMonths++;
                    }
                }
                return subMonths;
            }
        }

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

        public async Task SetDetails(bool checkForFollow = true)
        {
            if (this.ID > 0)
            {
                UserWithChannelModel userWithChannel = await ChannelSession.Connection.GetUser(this.GetModel());
                if (userWithChannel != null)
                {
                    if (!string.IsNullOrEmpty(userWithChannel.avatarUrl))
                    {
                        this.AvatarLink = userWithChannel.avatarUrl;
                    }
                    this.MixerAccountDate = userWithChannel.createdAt;
                    this.Sparks = (int)userWithChannel.sparks;
                    this.GameTypeID = userWithChannel.channel.typeId.GetValueOrDefault();
                }
            }

            if (checkForFollow)
            {
                await this.SetFollowDate();

                if (this.Roles.Contains(UserRole.Subscriber))
                {
                    await this.SetSubscribeDate();
                }
            }
        }

        public async Task SetFollowDate()
        {
            if (this.FollowDate == null || this.FollowDate.GetValueOrDefault() == DateTimeOffset.MinValue)
            {
                this.FollowDate = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, this.GetModel());
                if (this.FollowDate != null && this.FollowDate.GetValueOrDefault() > DateTimeOffset.MinValue)
                {
                    this.Roles.Add(UserRole.Follower);
                }
            }
        }

        public async Task SetSubscribeDate()
        {
            if (this.SubscribeDate == null || this.SubscribeDate.GetValueOrDefault() == DateTimeOffset.MinValue)
            {
                UserWithGroupsModel userGroups = await ChannelSession.Connection.GetUserInChannel(ChannelSession.Channel, this.ID);
                if (userGroups != null && userGroups.groups != null)
                {
                    UserGroupModel subscriberGroup = userGroups.groups.FirstOrDefault(g => g.name.Equals("Subscriber") && g.deletedAt == null);
                    if (subscriberGroup != null)
                    {
                        this.SubscribeDate = subscriberGroup.createdAt;
                        this.Roles.Add(UserRole.Subscriber);
                    }
                }
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
