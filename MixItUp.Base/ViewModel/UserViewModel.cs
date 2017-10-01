using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace MixItUp.Base.ViewModel
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
    }

    [DataContract]
    public class UserViewModel : IEquatable<UserViewModel>
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string AvatarLink { get; set; }

        [JsonIgnore]
        public List<UserRole> Roles { get; set; }

        [JsonIgnore]
        public UserRole PrimaryRole { get { return this.Roles.Max(); } }

        [JsonIgnore]
        public int ChatOffenses { get; set; }

        public UserViewModel() { }

        public UserViewModel(UserModel user) : this(user.id, user.username) { }

        public UserViewModel(ChatUserModel user) : this(user.userId.GetValueOrDefault(), user.userName, user.userRoles) { }

        public UserViewModel(ChatUserEventModel userEvent) : this(userEvent.id, userEvent.username, userEvent.roles) { }

        public UserViewModel(ChatMessageEventModel messageEvent) : this(messageEvent.user_id, messageEvent.user_name, messageEvent.user_roles) { }

        public UserViewModel(uint id, string username) : this(id, username, new string[] { }) { }

        public UserViewModel(uint id, string username, string[] userRoles)
        {
            this.ID = id;
            this.UserName = username;
            this.Roles = new List<UserRole>();

            this.Roles.Add(UserRole.User);
            if (userRoles.Any(r => r.Equals("Owner"))) { this.Roles.Add(UserRole.Streamer); }
            else if (userRoles.Any(r => r.Equals("Staff"))) { this.Roles.Add(UserRole.Staff); }
            else if (userRoles.Any(r => r.Equals("Mod"))) { this.Roles.Add(UserRole.Mod); }
            else if (userRoles.Any(r => r.Equals("Subscriber"))) { this.Roles.Add(UserRole.Subscriber); }
            else if (userRoles.Any(r => r.Equals("Pro"))) { this.Roles.Add(UserRole.Pro); }
            else if (userRoles.Any(r => r.Equals("Banned"))) { this.Roles.Add(UserRole.Banned); }
        }

        public SolidColorBrush PrimaryRoleColor
        {
            get
            {
                switch (this.PrimaryRole)
                {
                    case UserRole.Streamer:
                    case UserRole.Mod:
                        return Brushes.Green;
                    case UserRole.Staff:
                        return Brushes.Gold;
                    case UserRole.Subscriber:
                    case UserRole.Pro:
                        return Brushes.Purple;
                    case UserRole.Banned:
                        return Brushes.Red;
                    default:
                        return Brushes.Blue;
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

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.UserName; }
    }
}
