using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace MixItUp.Base.ViewModel.Chat
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

    public class ChatUserViewModel : UserViewModel, IEquatable<ChatUserViewModel>
    {
        public List<UserRole> Roles { get; set; }

        public UserRole PrimaryRole { get { return this.Roles.Max(); } }

        public int ChatOffenses { get; set; }

        public ChatUserViewModel(ChatUserModel user) : this(user.userId.GetValueOrDefault(), user.userName, user.userRoles) { }

        public ChatUserViewModel(ChatUserEventModel userEvent) : this(userEvent.id, userEvent.username, userEvent.roles) { }

        public ChatUserViewModel(ChatMessageEventModel messageEvent) : this(messageEvent.user_id, messageEvent.user_name, messageEvent.user_roles) { }

        public ChatUserViewModel(uint id, string username, string[] userRoles)
            : base(id, username)
        {
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
            if (obj is ChatUserViewModel)
            {
                return this.Equals((ChatUserViewModel)obj);
            }
            return false;
        }

        public bool Equals(ChatUserViewModel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.UserName; }
    }
}
