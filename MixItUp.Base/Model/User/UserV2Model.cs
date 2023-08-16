using MixItUp.Base.Model.User.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    public class UserV2Model : IEquatable<UserV2Model>
    {
        public static UserV2Model CreateUnassociated(string username = null)
        {
            UserV2Model user = new UserV2Model(new UnassociatedUserPlatformV2Model(!string.IsNullOrEmpty(username) ? username : MixItUp.Base.Resources.Anonymous));
            user.ID = Guid.Empty;
            return user;
        }

        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();

        [DataMember]
        public DateTimeOffset LastActivity { get; set; }
        [DataMember]
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;

        [DataMember]
        public int OnlineViewingMinutes { get; set; }

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
        public Guid EntranceCommandID { get; set; }
        [DataMember]
        public List<Guid> CustomCommandIDs { get; set; } = new List<Guid>();

        [DataMember]
        public string PatreonUserID { get; set; }

        [DataMember]
        public string AlejoPronounID { get; set; }

        [DataMember]
        public uint ModerationStrikes { get; set; }

        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public long TotalStreamsWatched { get; set; }
        [DataMember]
        public double TotalAmountDonated { get; set; }
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
        public Dictionary<StreamingPlatformTypeEnum, UserPlatformV2ModelBase> PlatformData { get; set; } = new Dictionary<StreamingPlatformTypeEnum, UserPlatformV2ModelBase>();

        [Obsolete]
        public UserV2Model() { }

        public UserV2Model(UserPlatformV2ModelBase platformModel)
        {
            this.AddPlatformData(platformModel);
        }

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

        public IEnumerable<UserPlatformV2ModelBase> GetAllPlatformData() { return this.PlatformData.Values.ToList(); }

        public void AddPlatformData(UserPlatformV2ModelBase platformModel) { this.PlatformData[platformModel.Platform] = platformModel; }

        public string GetPlatformID(StreamingPlatformTypeEnum platform) { return this.HasPlatformData(platform) ? this.GetPlatformData<UserPlatformV2ModelBase>(platform).ID : null; }

        public string GetPlatformUsername(StreamingPlatformTypeEnum platform) { return this.HasPlatformData(platform) ? this.GetPlatformData<UserPlatformV2ModelBase>(platform).Username : null; }

        public string GetPlatformDisplayName(StreamingPlatformTypeEnum platform)
        {
            if (this.HasPlatformData(platform))
            {
                UserPlatformV2ModelBase platformData = this.GetPlatformData<UserPlatformV2ModelBase>(platform);
                return platformData.DisplayName ?? platformData.Username;
            }
            return null;
        }

        public IEnumerable<string> GetAllPlatformUsernames() { return (this.PlatformData.Count > 0) ? this.PlatformData.Select(p => p.Value.Username) : new List<string>(); }

        public IEnumerable<string> GetAllPlatformDisplayNames() { return (this.PlatformData.Count > 0) ? this.PlatformData.Select(p => p.Value.DisplayName ?? p.Value.Username) : new List<string>(); }

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
    }
}
