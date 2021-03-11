using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    public enum UserRoleEnum
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

    [DataContract]
    public class UserDataModel : IEquatable<UserDataModel>
    {
        public static IEnumerable<UserRoleEnum> GetSelectableUserRoles()
        {
            List<UserRoleEnum> roles = new List<UserRoleEnum>(EnumHelper.GetEnumList<UserRoleEnum>());
            roles.Remove(UserRoleEnum.Banned);
            roles.Remove(UserRoleEnum.Custom);
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
        public HashSet<UserRoleEnum> MixerUserRoles { get; set; } = new HashSet<UserRoleEnum>() { UserRoleEnum.User };

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
        public HashSet<UserRoleEnum> TwitchUserRoles { get; set; } = new HashSet<UserRoleEnum>() { UserRoleEnum.User };
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
        public HashSet<UserRoleEnum> GlimeshUserRoles { get; set; } = new HashSet<UserRoleEnum>() { UserRoleEnum.User };

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
        public HashSet<UserRoleEnum> TrovoUserRoles { get; set; } = new HashSet<UserRoleEnum>() { UserRoleEnum.User };

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
        public int OfflineViewingMinutes { get; set; }

        [DataMember]
        public List<Guid> CustomCommandIDs { get; set; } = new List<Guid>();
        [DataMember]
        public Guid EntranceCommandID { get; set; }

        [DataMember]
        [Obsolete]
        public LockedList<ChatCommand> CustomCommands { get; set; } = new LockedList<ChatCommand>();
        [DataMember]
        [Obsolete]
        public CustomCommand EntranceCommand { get; set; }

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
        public DateTimeOffset LastSeen { get; set; }

        [JsonIgnore]
        public DateTimeOffset LastActivity { get; set; } = DateTimeOffset.MinValue;

        [JsonIgnore]
        public HashSet<string> CustomRoles { get; set; } = new HashSet<string>();
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
        public StreamingPlatformTypeEnum Platform
        {
            get
            {
                StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None;

#pragma warning disable CS0612 // Type or member is obsolete
                if (this.MixerID > 0) { platform = platform | StreamingPlatformTypeEnum.Mixer; }
#pragma warning restore CS0612 // Type or member is obsolete
                if (!string.IsNullOrEmpty(this.TwitchID)) { platform = platform | StreamingPlatformTypeEnum.Twitch; }
                if (!string.IsNullOrEmpty(this.GlimeshID)) { platform = platform | StreamingPlatformTypeEnum.Glimesh; }
                if (!string.IsNullOrEmpty(this.TrovoID)) { platform = platform | StreamingPlatformTypeEnum.Trovo; }

                return platform;
            }
        }

        [JsonIgnore]
        public string Username
        {
            get
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (this.Platform.HasFlag(StreamingPlatformTypeEnum.Mixer)) { return this.MixerUsername; }
#pragma warning restore CS0612 // Type or member is obsolete
                else if (this.Platform.HasFlag(StreamingPlatformTypeEnum.Twitch)) { return this.TwitchUsername; }
                else if (this.Platform.HasFlag(StreamingPlatformTypeEnum.Glimesh)) { return this.GlimeshUsername; }
                else if (this.Platform.HasFlag(StreamingPlatformTypeEnum.Trovo)) { return this.TrovoUsername; }
                return string.Empty;
            }
        }

        [JsonIgnore]
        public HashSet<UserRoleEnum> UserRoles
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchUserRoles; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshUserRoles; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.TrovoUserRoles; }
                return new HashSet<UserRoleEnum>() { UserRoleEnum.User };
            }
        }

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

        [JsonIgnore]
        public int PrimaryCurrency
        {
            get
            {
                CurrencyModel currency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => !c.IsRank && c.IsPrimary);
                if (currency != null)
                {
                    return currency.GetAmount(this);
                }
                return 0;
            }
        }

        [JsonIgnore]
        public RankModel Rank
        {
            get
            {
                CurrencyModel currency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => !c.IsRank && c.IsPrimary);
                if (currency != null)
                {
                    return currency.GetRank(this);
                }
                return CurrencyModel.NoRank;
            }
        }

        [JsonIgnore]
        public int PrimaryRankPoints
        {
            get
            {
                CurrencyModel currency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => c.IsRank && c.IsPrimary);
                if (currency != null)
                {
                    return currency.GetAmount(this);
                }
                return 0;
            }
        }

        [JsonIgnore]
        public string PrimaryRankName
        {
            get
            {
                return this.Rank.Name;
            }
        }

        [JsonIgnore]
        public string PrimaryRankNameAndPoints
        {
            get
            {
                return string.Format("{0} - {1}", this.PrimaryRankName, this.PrimaryRankPoints);
            }
        }

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
            this.OfflineViewingMinutes += other.OfflineViewingMinutes;

            this.CustomCommandIDs = other.CustomCommandIDs;
            this.EntranceCommandID = other.EntranceCommandID;

            this.IsCurrencyRankExempt = other.IsCurrencyRankExempt;
            this.PatreonUserID = other.PatreonUserID;

            this.TotalStreamsWatched = other.TotalStreamsWatched;
            this.TotalAmountDonated = other.TotalAmountDonated;
            this.TotalSubsGifted = other.TotalSubsGifted;
            this.TotalSubsReceived = other.TotalSubsReceived;
            this.TotalChatMessageSent = other.TotalChatMessageSent;
            this.TotalTimesTagged = other.TotalTimesTagged;
            this.TotalCommandsRun = other.TotalCommandsRun;
            this.TotalMonthsSubbed = other.TotalMonthsSubbed;
        }
    }
}