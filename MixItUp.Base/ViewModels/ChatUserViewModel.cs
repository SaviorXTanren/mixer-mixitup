using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using System.Linq;

namespace MixItUp.Base.ViewModels
{
    public enum UserRole
    {
        User,
        Pro,
        Subscriber,
        Mod,
        Streamer
    }

    public class ChatUserViewModel
    {
        public uint ID { get; private set; }

        public string UserName { get; private set; }

        public UserRole Role { get; private set; }

        public ChatUserViewModel(ChatUserModel user) : this(user.userId.GetValueOrDefault(), user.userName, user.userRoles) { }

        public ChatUserViewModel(ChatUserEventModel userEvent) : this(userEvent.id, userEvent.username, userEvent.roles) { }

        public ChatUserViewModel(ChatMessageEventModel messageEvent) : this(messageEvent.user_id, messageEvent.user_name, messageEvent.user_roles) { }

        public ChatUserViewModel(uint id, string username, string[] userRoles)
        {
            this.ID = id;
            this.UserName = username;

            this.Role = UserRole.User;
            if (userRoles.Any(r => r.Equals("Owner"))) { this.Role = UserRole.Streamer; }
            else if (userRoles.Any(r => r.Equals("Mod"))) { this.Role = UserRole.Mod; }
            else if (userRoles.Any(r => r.Equals("Subscriber"))) { this.Role = UserRole.Subscriber; }
            else if (userRoles.Any(r => r.Equals("Pro"))) { this.Role = UserRole.Pro; }
        }

        public override string ToString() { return this.UserName; }
    }
}
