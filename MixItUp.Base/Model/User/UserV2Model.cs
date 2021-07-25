using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    public class UserV2Model
    {
        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();

        [DataMember]
        public DateTimeOffset LastUpdated { get; set; }
        [JsonIgnore]
        public bool UpdatedThisSession { get; set; } = false;

        [DataMember]
        public Dictionary<Guid, int> CurrencyAmounts { get; set; } = new Dictionary<Guid, int>();
        [DataMember]
        public Dictionary<Guid, Dictionary<Guid, int>> InventoryAmounts { get; set; } = new Dictionary<Guid, Dictionary<Guid, int>>();
        [DataMember]
        public Dictionary<Guid, int> StreamPassAmounts { get; set; } = new Dictionary<Guid, int>();

        [DataMember]
        public string CustomTitle { get; set; }
        [DataMember]
        public bool IsSpecialtyExcluded { get; set; }

        [DataMember]
        public int OnlineViewingMinutes { get; set; }

        [DataMember]
        public List<Guid> CustomCommandIDs { get; set; } = new List<Guid>();
        [DataMember]
        public Guid EntranceCommandID { get; set; }

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

        [DataMember]
        private Dictionary<StreamingPlatformTypeEnum, UserPlatformV2Model> PlatformData { get; set; } = new Dictionary<StreamingPlatformTypeEnum, UserPlatformV2Model>();

        public UserV2Model() { }

        public HashSet<StreamingPlatformTypeEnum> GetPlatforms() { return new HashSet<StreamingPlatformTypeEnum>(this.PlatformData.Keys); }

        public bool HasPlatformData(StreamingPlatformTypeEnum platform) { return this.PlatformData.ContainsKey(platform); }

        public T GetPlatformData<T>(StreamingPlatformTypeEnum platform) where T : UserPlatformV2Model
        {
            if (!this.PlatformData.ContainsKey(platform))
            {
                if (platform == StreamingPlatformTypeEnum.Twitch) { this.PlatformData[platform] = new TwitchUserPlatformV2Model(); }
                else if (platform == StreamingPlatformTypeEnum.YouTube) { this.PlatformData[platform] = new YouTubeUserPlatformV2Model(); }
                else if (platform == StreamingPlatformTypeEnum.Glimesh) { this.PlatformData[platform] = new GlimeshUserPlatformV2Model(); }
                else if (platform == StreamingPlatformTypeEnum.Trovo) { this.PlatformData[platform] = new TrovoUserPlatformV2Model(); }
            }
            return (T)this.PlatformData[platform];
        }

        public override bool Equals(object obj)
        {
            if (obj is UserDataModel)
            {
                return this.Equals((UserDataModel)obj);
            }
            return false;
        }

        public bool Equals(UserV2Model other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public void MergeData(UserV2Model other)
        {
            foreach (StreamingPlatformTypeEnum platform in other.GetPlatforms())
            {
                if (!this.HasPlatformData(platform))
                {
                    this.PlatformData[platform] = other.GetPlatformData<UserPlatformV2Model>(platform);
                }
            }

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
            this.OnlineViewingMinutes += other.OnlineViewingMinutes;

            this.CustomCommandIDs = other.CustomCommandIDs;
            this.EntranceCommandID = other.EntranceCommandID;

            this.IsSpecialtyExcluded = other.IsSpecialtyExcluded;
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
    }
}
