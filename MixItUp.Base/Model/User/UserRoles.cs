using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.User
{
    public enum UserRoleEnum
    {
        [Obsolete]
        Banned = 0,

        [GenericUserRole, TwitchUserRole, YouTubeUserRole, TrovoUserRole, GlimeshUserRole]
        User = 100,

        [TwitchUserRole]
        TwitchAffiliate = 200,

        [TwitchUserRole]
        TwitchPartner = 250,

        [GenericUserRole, TwitchUserRole, TrovoUserRole, GlimeshUserRole]
        Follower = 300,
        [YouTubeUserRole]
        YouTubeSubscriber = 301,

        [GenericUserRole, TwitchUserRole, YouTubeUserRole, TrovoUserRole, GlimeshUserRole]
        Regular = 400,

        [TwitchUserRole]
        TwitchVIP = 500,

        [GenericUserRole, TwitchUserRole, TrovoUserRole, GlimeshUserRole]
        Subscriber = 600,
        [YouTubeUserRole]
        YouTubeMember = 601,

        [TwitchUserRole]
        TwitchGlobalMod = 701,
        [TrovoUserRole]
        TrovoWarden = 702,

        [TwitchUserRole]
        TwitchStaff = 751,
        [TrovoUserRole]
        TrovoAdmin = 752,
        [GlimeshUserRole]
        GlimeshAdmin = 753,

        [GenericUserRole, TwitchUserRole, YouTubeUserRole, TrovoUserRole, GlimeshUserRole]
        Moderator = 800,

        [TrovoUserRole]
        TrovoSuperMod = 825,

        [TwitchUserRole]
        TwitchChannelEditor = 850,
        [TrovoUserRole]
        TrovoEditor = 851,

        [GenericUserRole, TwitchUserRole, YouTubeUserRole, TrovoUserRole, GlimeshUserRole]
        Streamer = 900,
    }

    public static class UserRoles
    {
        public static IEnumerable<UserRoleEnum> All { get { return all; } }
        private readonly static IEnumerable<UserRoleEnum> all = EnumHelper.GetEnumList<UserRoleEnum>();

        public static IEnumerable<UserRoleEnum> Generic { get { return generic; } }
        private readonly static IEnumerable<UserRoleEnum> generic = GetSelectableRoles<GenericUserRoleAttribute>();

        public static IEnumerable<UserRoleEnum> Twitch { get { return twitch; } }
        private readonly static IEnumerable<UserRoleEnum> twitch = GetSelectableRoles<TwitchUserRoleAttribute>();

        public static IEnumerable<UserRoleEnum> YouTube { get { return youtube; } }
        private readonly static IEnumerable<UserRoleEnum> youtube = GetSelectableRoles<YouTubeUserRoleAttribute>();

        public static IEnumerable<UserRoleEnum> Trovo { get { return trovo; } }
        private readonly static IEnumerable<UserRoleEnum> trovo = GetSelectableRoles<TrovoUserRoleAttribute>();

        public static IEnumerable<UserRoleEnum> Glimesh { get { return glimesh; } }
        private readonly static IEnumerable<UserRoleEnum> glimesh = GetSelectableRoles<GlimeshUserRoleAttribute>();

        private static IEnumerable<UserRoleEnum> GetSelectableRoles<T>() where T : UserRoleAttributeBase
        {
            List<UserRoleEnum> roles = new List<UserRoleEnum>();
            foreach (UserRoleEnum role in EnumHelper.GetEnumList<UserRoleEnum>())
            {
                var attributes = (T[])role.GetType().GetField(role.ToString()).GetCustomAttributes(typeof(T), false);
                if (attributes != null && attributes.Length > 0)
                {
                    roles.Add(role);
                }
            }
            return roles;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        public static UserRoleEnum ConvertFromOldRole(OldUserRoleEnum role)
        {
            switch (role)
            {
                case OldUserRoleEnum.Affiliate: return UserRoleEnum.TwitchAffiliate;
                case OldUserRoleEnum.Partner: return UserRoleEnum.TwitchPartner;
                case OldUserRoleEnum.Follower: return UserRoleEnum.Follower;
                case OldUserRoleEnum.Regular: return UserRoleEnum.Regular;
                case OldUserRoleEnum.VIP: return UserRoleEnum.TwitchVIP;
                case OldUserRoleEnum.Subscriber: return UserRoleEnum.Subscriber;
                case OldUserRoleEnum.VIPExclusive: return UserRoleEnum.TwitchVIP;
                case OldUserRoleEnum.GlobalMod: return UserRoleEnum.TwitchGlobalMod;
                case OldUserRoleEnum.Mod: return UserRoleEnum.Moderator;
                case OldUserRoleEnum.ChannelEditor: return UserRoleEnum.TwitchChannelEditor;
                case OldUserRoleEnum.Staff: return UserRoleEnum.TwitchStaff;
                case OldUserRoleEnum.Streamer: return UserRoleEnum.Streamer;
                default: return UserRoleEnum.User;
            }
        }
#pragma warning restore CS0612 // Type or member is obsolete
    }

    public abstract class UserRoleAttributeBase : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    public class GenericUserRoleAttribute : UserRoleAttributeBase
    {
        public static readonly GenericUserRoleAttribute Default;

        public GenericUserRoleAttribute() { }

        public override bool Equals(object obj) { return (obj is GenericUserRoleAttribute); }

        public override int GetHashCode()
        {
            int hashCode = -86145682;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(TypeId);
            return hashCode;
        }

        public override bool IsDefaultAttribute() { return this.Equals(GenericUserRoleAttribute.Default); }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class TwitchUserRoleAttribute : UserRoleAttributeBase
    {
        public static readonly TwitchUserRoleAttribute Default;

        public TwitchUserRoleAttribute() { }

        public override bool Equals(object obj) { return (obj is TwitchUserRoleAttribute); }

        public override int GetHashCode()
        {
            int hashCode = -86145682;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(TypeId);
            return hashCode;
        }

        public override bool IsDefaultAttribute() { return this.Equals(TwitchUserRoleAttribute.Default); }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class YouTubeUserRoleAttribute : UserRoleAttributeBase
    {
        public static readonly YouTubeUserRoleAttribute Default;

        public YouTubeUserRoleAttribute() { }

        public override bool Equals(object obj) { return (obj is YouTubeUserRoleAttribute); }

        public override int GetHashCode()
        {
            int hashCode = -86145682;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(TypeId);
            return hashCode;
        }

        public override bool IsDefaultAttribute() { return this.Equals(YouTubeUserRoleAttribute.Default); }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class TrovoUserRoleAttribute : UserRoleAttributeBase
    {
        public static readonly TrovoUserRoleAttribute Default;

        public TrovoUserRoleAttribute() { }

        public override bool Equals(object obj) { return (obj is TrovoUserRoleAttribute); }

        public override int GetHashCode()
        {
            int hashCode = -86145682;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(TypeId);
            return hashCode;
        }

        public override bool IsDefaultAttribute() { return this.Equals(TrovoUserRoleAttribute.Default); }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class GlimeshUserRoleAttribute : UserRoleAttributeBase
    {
        public static readonly GlimeshUserRoleAttribute Default;

        public GlimeshUserRoleAttribute() { }

        public override bool Equals(object obj) { return (obj is GlimeshUserRoleAttribute); }

        public override int GetHashCode()
        {
            int hashCode = -86145682;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(TypeId);
            return hashCode;
        }

        public override bool IsDefaultAttribute() { return this.Equals(GlimeshUserRoleAttribute.Default); }
    }
}
