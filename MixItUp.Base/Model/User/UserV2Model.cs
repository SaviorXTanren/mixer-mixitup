using MixItUp.Base.Model.User.Platform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    public class UserV2Model : IEquatable<UserV2Model>
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
        public long TotalStreamsWatched { get; set; }
        [DataMember]
        public long TotalAmountDonated { get; set; }
        [DataMember]
        public long TotalBitsCheered { get; set; }
        [DataMember]
        public long TotalSubsGifted { get; set; }
        [DataMember]
        public long TotalSubsReceived { get; set; }
        [DataMember]
        public long TotalChatMessageSent { get; set; }
        [DataMember]
        public long TotalTimesTagged { get; set; }
        [DataMember]
        public long TotalCommandsRun { get; set; }
        [DataMember]
        public long TotalMonthsSubbed { get; set; }

        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public DateTimeOffset LastActivity { get; set; }

        [DataMember]
        private Dictionary<StreamingPlatformTypeEnum, UserPlatformV2ModelBase> PlatformData { get; set; } = new Dictionary<StreamingPlatformTypeEnum, UserPlatformV2ModelBase>();

        public UserV2Model() { }

        public HashSet<StreamingPlatformTypeEnum> GetPlatforms() { return new HashSet<StreamingPlatformTypeEnum>(this.PlatformData.Keys); }

        public bool HasPlatformData(StreamingPlatformTypeEnum platform) { return this.PlatformData.ContainsKey(platform); }

        public T GetPlatformData<T>(StreamingPlatformTypeEnum platform) where T : UserPlatformV2ModelBase
        {
            if (this.PlatformData.ContainsKey(platform))
            {
                return (T)this.PlatformData[platform];
            }
            return null;
        }

        public void AddPlatformData(UserPlatformV2ModelBase platformModel) { this.PlatformData[platformModel.Platform] = platformModel; }

        public override bool Equals(object obj)
        {
            if (obj is UserV2Model)
            {
                return this.Equals((UserV2Model)obj);
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
                    this.PlatformData[platform] = other.GetPlatformData<UserPlatformV2ModelBase>(platform);
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
