using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public enum ModerationNotificationActionType
    {
        ban,
        timeout,
        unban,
        untimeout,
        clear,
        emoteonly,
        emoteonlyoff,
        followers,
        followersoff,
        uniquechat,
        uniquechatoff,
        slow,
        slowoff,
        subscribers,
        subscribersoff,
        unraid,
        delete,
        unvip,
        vip,
        raid,
        add_blocked_term,
        add_permitted_term,
        remove_blocked_term,
        remove_permitted_term,
        mod,
        unmod,
        approve_unban_request,
        deny_unban_request,
        warn,
        shared_chat_ban,
        shared_chat_timeout,
        shared_chat_unban,
        shared_chat_untimeout,
        shared_chat_delete,
    }

    public class ModerationNotification
    {
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public string source_broadcaster_user_id { get; set; }
        public string source_broadcaster_user_login { get; set; }
        public string source_broadcaster_user_name { get; set; }
        public string moderator_user_id { get; set; }
        public string moderator_user_login { get; set; }
        public string moderator_user_name { get; set; }
        public string action { get; set; }
        public ModerationNotificationFollowers followers { get; set; }
        public ModerationNotificationSlow slow { get; set; }
        public ModerationNotificationVIP vip { get; set; }
        public ModerationNotificationUnVIP unvip { get; set; }
        public ModerationNotificationMod mod { get; set; }
        public ModerationNotificationUnMod unmod { get; set; }
        public ModerationNotificationBan ban { get; set; }
        public ModerationNotificationUnBan unban { get; set; }
        public ModerationNotificationTimeout timeout { get; set; }
        public ModerationNotificationUnTimeout untimeout { get; set; }
        public ModerationNotificationRaid raid { get; set; }
        public ModerationNotificationUnRaid unraid { get; set; }
        public ModerationNotificationDelete delete { get; set; }
        public ModerationNotificationAutomodTerms automod_terms { get; set; }
        public ModerationNotificationUnBanRequest unban_request { get; set; }
        public ModerationNotificationWarn warn { get; set; }
        public ModerationNotificationBan shared_chat_ban { get; set; }
        public ModerationNotificationUnBan shared_chat_unban { get; set; }
        public ModerationNotificationTimeout shared_chat_timeout { get; set; }
        public ModerationNotificationUnTimeout shared_chat_untimeout { get; set; }
        public ModerationNotificationDelete shared_chat_delete { get; set; }

        public ModerationNotificationActionType ActionType { get { return EnumHelper.GetEnumValueFromString<ModerationNotificationActionType>(this.action); } }
    }

    public class ModerationNotificationFollowers
    {
        public int follow_duration_minutes { get; set; }
    }

    public class ModerationNotificationSlow
    {
        public int wait_time_seconds { get; set; }
    }

    public class ModerationNotificationBasicUser
    {
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }
    }

    public class ModerationNotificationVIP : ModerationNotificationBasicUser
    {
    }

    public class ModerationNotificationUnVIP : ModerationNotificationBasicUser
    {
    }

    public class ModerationNotificationMod : ModerationNotificationBasicUser
    {
    }

    public class ModerationNotificationUnMod : ModerationNotificationBasicUser
    {
    }

    public class ModerationNotificationBan : ModerationNotificationBasicUser
    {
        public string reason { get; set; }
    }

    public class ModerationNotificationUnBan : ModerationNotificationBasicUser
    {
    }

    public class ModerationNotificationTimeout : ModerationNotificationBasicUser
    {
        public string reason { get; set; }
        public string expires_at { get; set; }

        public DateTimeOffset ExpiresAt { get { return TwitchService.GetTwitchDateTime(this.expires_at); } }
        public int TotalSeconds { get { return (int)Math.Ceiling((this.ExpiresAt - DateTimeOffset.Now).TotalSeconds); } }
    }

    public class ModerationNotificationUnTimeout : ModerationNotificationBasicUser
    {
    }

    public class ModerationNotificationRaid : ModerationNotificationBasicUser
    {
        public int viewer_count { get; set; }
    }

    public class ModerationNotificationUnRaid : ModerationNotificationBasicUser
    {
    }

    public class ModerationNotificationDelete : ModerationNotificationBasicUser
    {
        public string message_id { get; set; }
        public string message_body { get; set; }
    }

    public class ModerationNotificationAutomodTerms
    {
        public string action { get; set; }
        public string list { get; set; }
        public List<string> terms { get; set; } = new List<string>();
        public bool from_automod { get; set; }
    }

    public class ModerationNotificationUnBanRequest : ModerationNotificationBasicUser
    {
        public bool is_approved { get; set; }
        public string moderator_message { get; set; }
    }

    public class ModerationNotificationWarn : ModerationNotificationBasicUser
    {
        public string reason { get; set; }
        public List<string> chat_rules_cited { get; set; } = new List<string>();
    }
}
