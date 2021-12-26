using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services.External;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    [Obsolete]
    public enum OldUserRoleEnum
    {
        Banned,
        User = 10,
        Premium = 20,
        Affiliate = 23,
        Partner = 25,
        Follower = 30,
        Regular = 35,
        VIP = 38,
        Subscriber = 40,
        VIPExclusive = 45,
        GlobalMod = 48,
        Mod = 50,
        ChannelEditor = 55,
        Staff = 60,
        Streamer = 70,

        Custom = 99,
    }

    [Obsolete]
    [DataContract]
    public class UserDataModel : IEquatable<UserDataModel>
    {
        public static IEnumerable<OldUserRoleEnum> GetSelectableUserRoles()
        {
            List<OldUserRoleEnum> roles = new List<OldUserRoleEnum>(EnumHelper.GetEnumList<OldUserRoleEnum>());
            roles.Remove(OldUserRoleEnum.Banned);
            roles.Remove(OldUserRoleEnum.Custom);
            return roles;
        }

        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();

        [DataMember]
        public DateTimeOffset LastUpdated { get; set; }
        [JsonIgnore]
        public bool UpdatedThisSession { get; set; } = false;

        [DataMember]
        public string UnassociatedUsername { get; set; }

        #region Mixer

        [DataMember]
        public uint MixerID { get; set; }
        [DataMember]
        public string MixerUsername { get; set; }
        [DataMember]
        public uint MixerChannelID { get; set; }

        [DataMember]
        public HashSet<OldUserRoleEnum> MixerUserRoles { get; set; } = new HashSet<OldUserRoleEnum>() { OldUserRoleEnum.User };

        [DataMember]
        public DateTimeOffset? MixerAccountDate { get; set; }
        [DataMember]
        public DateTimeOffset? MixerFollowDate { get; set; }
        [DataMember]
        public DateTimeOffset? MixerSubscribeDate { get; set; }

        #endregion Mixer

        #region Twitch

        [DataMember]
        public string TwitchID { get; set; }
        [DataMember]
        public string TwitchUsername { get; set; }
        [DataMember]
        public string TwitchDisplayName { get; set; }
        [DataMember]
        public string TwitchAvatarLink { get; set; }

        [DataMember]
        public HashSet<OldUserRoleEnum> TwitchUserRoles { get; set; } = new HashSet<OldUserRoleEnum>() { OldUserRoleEnum.User };
        [DataMember]
        public Dictionary<string, int> TwitchBadges { get; set; } = new Dictionary<string, int>();
        [DataMember]
        public Dictionary<string, int> TwitchBadgeInfo { get; set; } = new Dictionary<string, int>();

        [DataMember]
        public string TwitchColor { get; set; }

        [DataMember]
        public DateTimeOffset? TwitchAccountDate { get; set; }
        [DataMember]
        public DateTimeOffset? TwitchFollowDate { get; set; }
        [DataMember]
        public DateTimeOffset? TwitchSubscribeDate { get; set; }
        [DataMember]
        public int TwitchSubscriberTier { get; set; } = 0;

        #endregion Twitch

        #region YouTube

        [DataMember]
        public string YouTubeID { get; set; }
        [DataMember]
        public string YouTubeUsername { get; set; }
        [DataMember]
        public string YouTubeDisplayName { get; set; }
        [DataMember]
        public string YouTubeAvatarLink { get; set; }
        [DataMember]
        public string YouTubeURL { get; set; }

        [DataMember]
        public HashSet<OldUserRoleEnum> YouTubeUserRoles { get; set; } = new HashSet<OldUserRoleEnum>() { OldUserRoleEnum.User };

        [DataMember]
        public DateTimeOffset? YouTubeAccountDate { get; set; }
        [DataMember]
        public DateTimeOffset? YouTubeFollowDate { get; set; }
        [DataMember]
        public DateTimeOffset? YouTubeSubscribeDate { get; set; }

        #endregion YouTube

        #region Glimesh

        [DataMember]
        public string GlimeshID { get; set; }
        [DataMember]
        public string GlimeshUsername { get; set; }
        [DataMember]
        public string GlimeshDisplayName { get; set; }
        [DataMember]
        public string GlimeshAvatarLink { get; set; }

        [DataMember]
        public HashSet<OldUserRoleEnum> GlimeshUserRoles { get; set; } = new HashSet<OldUserRoleEnum>() { OldUserRoleEnum.User };

        [DataMember]
        public DateTimeOffset? GlimeshAccountDate { get; set; }
        [DataMember]
        public DateTimeOffset? GlimeshFollowDate { get; set; }
        [DataMember]
        public DateTimeOffset? GlimeshSubscribeDate { get; set; }

        #endregion Glimesh

        #region Trovo

        [DataMember]
        public string TrovoID { get; set; }
        [DataMember]
        public string TrovoUsername { get; set; }
        [DataMember]
        public string TrovoDisplayName { get; set; }
        [DataMember]
        public string TrovoAvatarLink { get; set; }

        [DataMember]
        public HashSet<OldUserRoleEnum> TrovoUserRoles { get; set; } = new HashSet<OldUserRoleEnum>() { OldUserRoleEnum.User };

        [DataMember]
        public DateTimeOffset? TrovoAccountDate { get; set; }
        [DataMember]
        public DateTimeOffset? TrovoFollowDate { get; set; }
        [DataMember]
        public DateTimeOffset? TrovoSubscribeDate { get; set; }
        [DataMember]
        public int TrovoSubscriberLevel { get; set; } = 0;

        #endregion Trovo

        [DataMember]
        public Dictionary<Guid, int> CurrencyAmounts { get; set; } = new Dictionary<Guid, int>();
        [DataMember]
        public Dictionary<Guid, Dictionary<Guid, int>> InventoryAmounts { get; set; } = new Dictionary<Guid, Dictionary<Guid, int>>();
        [DataMember]
        public Dictionary<Guid, int> StreamPassAmounts { get; set; } = new Dictionary<Guid, int>();

        [DataMember]
        public string Color { get; set; } = string.Empty;
        [DataMember]
        public string CustomTitle { get; set; }

        [DataMember]
        public int ViewingMinutes { get; set; }

        [DataMember]
        public List<Guid> CustomCommandIDs { get; set; } = new List<Guid>();
        [DataMember]
        public Guid EntranceCommandID { get; set; }

        [DataMember]
        public bool IsCurrencyRankExempt { get; set; }

        [DataMember]
        public string PatreonUserID { get; set; }

        [DataMember]
        public uint ModerationStrikes { get; set; }

        [DataMember]
        public uint TotalStreamsWatched { get; set; }
        [DataMember]
        public double TotalAmountDonated { get; set; }
        [DataMember]
        public uint TotalBitsCheered { get; set; }
        [DataMember]
        public uint TotalSubsGifted { get; set; }
        [DataMember]
        public uint TotalSubsReceived { get; set; }
        [DataMember]
        public uint TotalChatMessageSent { get; set; }
        [DataMember]
        public uint TotalTimesTagged { get; set; }
        [DataMember]
        public uint TotalCommandsRun { get; set; }
        [DataMember]
        public uint TotalMonthsSubbed { get; set; }

        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public DateTimeOffset LastSeen { get; set; }

        [JsonIgnore]
        public DateTimeOffset LastActivity { get; set; } = DateTimeOffset.MinValue;

        [JsonIgnore]
        public HashSet<string> CustomRoles { get; set; } = new HashSet<string>();
        [JsonIgnore]
        public string RolesString { get; set; } = null;
        [JsonIgnore]
        public string RolesDisplayString { get; set; } = null;

        [JsonIgnore]
        public bool IgnoreForQueries { get; set; }
        [JsonIgnore]
        public bool IsInChat { get; set; }
        [JsonIgnore]
        public int WhispererNumber { get; set; }
        [JsonIgnore]
        public string TwitterURL { get; set; }
        [JsonIgnore]
        public PatreonCampaignMember PatreonUser { get; set; } = null;

        public UserDataModel() { }

        [JsonIgnore]
        public HashSet<StreamingPlatformTypeEnum> Platforms
        {
            get
            {
                if (this.platforms == null)
                {
                    this.platforms = new HashSet<StreamingPlatformTypeEnum>();

#pragma warning disable CS0612 // Type or member is obsolete
                    if (this.MixerID > 0) { this.platforms.Add(StreamingPlatformTypeEnum.Mixer); }
#pragma warning restore CS0612 // Type or member is obsolete
                    if (!string.IsNullOrEmpty(this.TwitchID)) { this.platforms.Add(StreamingPlatformTypeEnum.Twitch); }
                    if (!string.IsNullOrEmpty(this.YouTubeID)) { this.platforms.Add(StreamingPlatformTypeEnum.YouTube); }
                    if (!string.IsNullOrEmpty(this.GlimeshID)) { this.platforms.Add(StreamingPlatformTypeEnum.Glimesh); }
                    if (!string.IsNullOrEmpty(this.TrovoID)) { this.platforms.Add(StreamingPlatformTypeEnum.Trovo); }
                }
                return this.platforms;
            }
        }
        private HashSet<StreamingPlatformTypeEnum> platforms = null;

        [JsonIgnore]
        public string Username
        {
            get
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (this.Platforms.Contains(StreamingPlatformTypeEnum.Mixer)) { return this.MixerUsername; }
#pragma warning restore CS0612 // Type or member is obsolete
                else if (this.Platforms.Contains(StreamingPlatformTypeEnum.Twitch)) { return this.TwitchUsername; }
                else if (this.Platforms.Contains(StreamingPlatformTypeEnum.YouTube)) { return this.YouTubeUsername; }
                else if (this.Platforms.Contains(StreamingPlatformTypeEnum.Glimesh)) { return this.GlimeshUsername; }
                else if (this.Platforms.Contains(StreamingPlatformTypeEnum.Trovo)) { return this.TrovoUsername; }
                return string.Empty;
            }
        }

        [JsonIgnore]
        public HashSet<OldUserRoleEnum> UserRoles
        {
            get
            {
                if (this.Platforms.Contains(StreamingPlatformTypeEnum.Twitch)) { return this.TwitchUserRoles; }
                else if (this.Platforms.Contains(StreamingPlatformTypeEnum.YouTube)) { return this.YouTubeUserRoles; }
                else if (this.Platforms.Contains(StreamingPlatformTypeEnum.Glimesh)) { return this.GlimeshUserRoles; }
                else if (this.Platforms.Contains(StreamingPlatformTypeEnum.Trovo)) { return this.TrovoUserRoles; }
                return new HashSet<OldUserRoleEnum>() { OldUserRoleEnum.User };
            }
        }

        [JsonIgnore]
        public OldUserRoleEnum PrimaryRole { get { return (this.UserRoles.Count() > 0) ? this.UserRoles.ToList().Max() : OldUserRoleEnum.User; } }

        [JsonIgnore]
        public string ViewingHoursString { get { return (this.ViewingMinutes / 60).ToString(); } }

        [JsonIgnore]
        public string ViewingMinutesString { get { return (this.ViewingMinutes % 60).ToString(); } }

        [JsonIgnore]
        public int ViewingHoursPart
        {
            get
            {
                return this.ViewingMinutes / 60;
            }
            set
            {
                this.ViewingMinutes = value * 60 + this.ViewingMinutesPart;
            }
        }

        [JsonIgnore]
        public int ViewingMinutesPart
        {
            get
            {
                return this.ViewingMinutes % 60;
            }
            set
            {
                int extraHours = value / 60;
                this.ViewingHoursPart += extraHours;
                this.ViewingMinutes = ViewingHoursPart * 60 + (value % 60);
            }
        }

        [JsonIgnore]
        public string ViewingTimeString { get { return string.Format("{0} Hours & {1} Mins", this.ViewingHoursString, this.ViewingMinutesString); } }

        [JsonIgnore]
        public string ViewingTimeShortString { get { return string.Format("{0}H & {1}M", this.ViewingHoursString, this.ViewingMinutesString); } }

        public override bool Equals(object obj)
        {
            if (obj is UserDataModel)
            {
                return this.Equals((UserDataModel)obj);
            }
            return false;
        }

        public bool Equals(UserDataModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public override string ToString()
        {
            return this.Username;
        }

        public void MergeData(UserDataModel other)
        {
            foreach (var kvp in other.CurrencyAmounts)
            {
                if (!this.CurrencyAmounts.ContainsKey(kvp.Key))
                {
                    this.CurrencyAmounts[kvp.Key] = 0;
                }
                this.CurrencyAmounts[kvp.Key] += other.CurrencyAmounts[kvp.Key];
            }

            foreach (var kvp in other.InventoryAmounts)
            {
                if (!this.InventoryAmounts.ContainsKey(kvp.Key))
                {
                    this.InventoryAmounts[kvp.Key] = new Dictionary<Guid, int>();
                }
                foreach (var itemKVP in other.InventoryAmounts[kvp.Key])
                {
                    if (!this.InventoryAmounts[kvp.Key].ContainsKey(itemKVP.Key))
                    {
                        this.InventoryAmounts[kvp.Key][itemKVP.Key] = 0;
                    }
                    this.InventoryAmounts[kvp.Key][itemKVP.Key] += other.InventoryAmounts[kvp.Key][itemKVP.Key];
                }
            }

            foreach (var kvp in other.StreamPassAmounts)
            {
                if (!this.StreamPassAmounts.ContainsKey(kvp.Key))
                {
                    this.StreamPassAmounts[kvp.Key] = 0;
                }
                this.StreamPassAmounts[kvp.Key] += other.StreamPassAmounts[kvp.Key];
            }

            this.CustomTitle = other.CustomTitle;
            this.ViewingMinutes += other.ViewingMinutes;

            this.CustomCommandIDs = other.CustomCommandIDs;
            this.EntranceCommandID = other.EntranceCommandID;

            this.IsCurrencyRankExempt = other.IsCurrencyRankExempt;
            this.PatreonUserID = other.PatreonUserID;

            this.TotalStreamsWatched += other.TotalStreamsWatched;
            this.TotalAmountDonated += other.TotalAmountDonated;
            this.TotalSubsGifted += other.TotalSubsGifted;
            this.TotalSubsReceived += other.TotalSubsReceived;
            this.TotalChatMessageSent += other.TotalChatMessageSent;
            this.TotalTimesTagged += other.TotalTimesTagged;
            this.TotalCommandsRun += other.TotalCommandsRun;
            this.TotalMonthsSubbed += other.TotalMonthsSubbed;

            this.Notes += other.Notes;
        }

        public UserV2Model ToV2Model()
        {
            if (!string.IsNullOrEmpty(this.TwitchID))
            {
                TwitchUserPlatformV2Model platformData = new TwitchUserPlatformV2Model(this.TwitchID, this.TwitchUsername, this.TwitchDisplayName);
                UserV2Model result = new UserV2Model(platformData);

                result.ID = this.ID;
                result.LastActivity = this.LastActivity;
                result.LastUpdated = this.LastUpdated;

                foreach (var kvp in this.CurrencyAmounts)
                {
                    result.CurrencyAmounts[kvp.Key] = this.CurrencyAmounts[kvp.Key];
                }

                foreach (var kvp in this.InventoryAmounts)
                {
                    result.InventoryAmounts[kvp.Key] = new Dictionary<Guid, int>();
                    foreach (var itemKVP in this.InventoryAmounts[kvp.Key])
                    {
                        result.InventoryAmounts[kvp.Key][itemKVP.Key] = this.InventoryAmounts[kvp.Key][itemKVP.Key];
                    }
                }

                foreach (var kvp in this.StreamPassAmounts)
                {
                    result.StreamPassAmounts[kvp.Key] = this.StreamPassAmounts[kvp.Key];
                }

                result.CustomTitle = this.CustomTitle;
                result.IsSpecialtyExcluded = this.IsCurrencyRankExempt;
                result.EntranceCommandID = this.EntranceCommandID;
                result.CustomCommandIDs.AddRange(this.CustomCommandIDs);
                result.PatreonUserID = this.PatreonUserID;
                result.ModerationStrikes = this.ModerationStrikes;
                result.OnlineViewingMinutes = this.ViewingMinutes;
                result.TotalAmountDonated = this.TotalAmountDonated;
                result.TotalChatMessageSent = this.TotalChatMessageSent;
                result.TotalCommandsRun = this.TotalCommandsRun;
                result.TotalMonthsSubbed = this.TotalMonthsSubbed;
                result.TotalStreamsWatched = this.TotalStreamsWatched;
                result.TotalSubsGifted = this.TotalSubsGifted;
                result.TotalSubsReceived = this.TotalSubsReceived;
                result.TotalTimesTagged = this.TotalTimesTagged;
                result.Notes = this.Notes;

                platformData.AccountDate = this.TwitchAccountDate;
                platformData.AvatarLink = this.TwitchAvatarLink;
                platformData.BadgeInfo = this.TwitchBadgeInfo;
                platformData.Badges = this.TwitchBadges;
                platformData.Color = this.TwitchColor;
                platformData.FollowDate = this.TwitchFollowDate;
                platformData.SubscribeDate = this.TwitchSubscribeDate;
                platformData.SubscriberTier = this.TwitchSubscriberTier;

                platformData.TotalBitsCheered = this.TotalBitsCheered;

                foreach (OldUserRoleEnum role in this.TwitchUserRoles)
                {
                    platformData.Roles.Add(MixItUp.Base.Model.User.UserRoles.ConvertFromOldRole(role));
                }
                result.AddPlatformData(platformData);

                return result;
            }
            return null;
        }
    }
}