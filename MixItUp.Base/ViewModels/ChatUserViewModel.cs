using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModels
{
    public enum UserRole
    {
        Banned,
        User,
        Pro,
        Subscriber,
        Mod,
        Staff,
        Streamer,
    }

    public class ChatUserViewModel : IEquatable<ChatUserViewModel>
    {
        public uint ID { get; private set; }

        public string UserName { get; private set; }

        public IEnumerable<UserRole> Roles { get; private set; }

        public UserRole PrimaryRole { get { return this.Roles.Max(); } }

        public ChatUserViewModel(ChatUserModel user) : this(user.userId.GetValueOrDefault(), user.userName, user.userRoles) { }

        public ChatUserViewModel(ChatUserEventModel userEvent) : this(userEvent.id, userEvent.username, userEvent.roles) { }

        public ChatUserViewModel(ChatMessageEventModel messageEvent) : this(messageEvent.user_id, messageEvent.user_name, messageEvent.user_roles) { }

        public ChatUserViewModel(uint id, string username, string[] userRoles)
        {
            this.ID = id;
            this.UserName = username;
            List<UserRole> roles = new List<UserRole>();

            roles.Add(UserRole.User);
            if (userRoles.Any(r => r.Equals("Owner"))) { roles.Add(UserRole.Streamer); }
            else if (userRoles.Any(r => r.Equals("Staff"))) { roles.Add(UserRole.Staff); }
            else if (userRoles.Any(r => r.Equals("Mod"))) { roles.Add(UserRole.Mod); }
            else if (userRoles.Any(r => r.Equals("Subscriber"))) { roles.Add(UserRole.Subscriber); }
            else if (userRoles.Any(r => r.Equals("Pro"))) { roles.Add(UserRole.Pro); }
            else if (userRoles.Any(r => r.Equals("Banned"))) { roles.Add(UserRole.Banned); }

            this.Roles = roles;
        }

        public ChatUserModel GetModel()
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
