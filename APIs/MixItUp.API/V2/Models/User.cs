using System;
using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class User
    {
        public Guid ID { get; set; }
        public DateTimeOffset LastActivity { get; set; } = DateTimeOffset.MinValue;
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;
        public int OnlineViewingMinutes { get; set; }
        public Dictionary<Guid, int> CurrencyAmounts { get; set; } = new Dictionary<Guid, int>();
        public Dictionary<Guid, Dictionary<Guid, int>> InventoryAmounts { get; set; } = new Dictionary<Guid, Dictionary<Guid, int>>();
        public Dictionary<Guid, int> StreamPassAmounts { get; set; } = new Dictionary<Guid, int>();
        public string CustomTitle { get; set; }
        public bool IsSpecialtyExcluded { get; set; }
        public Guid EntranceCommandID { get; set; }
        public List<Guid> CustomCommandIDs { get; set; } = new List<Guid>();
        public string PatreonUserID { get; set; }
        public uint ModerationStrikes { get; set; }
        public string Notes { get; set; }
        public long TotalStreamsWatched { get; set; }
        public double TotalAmountDonated { get; set; }
        public long TotalSubsGifted { get; set; }
        public long TotalSubsReceived { get; set; }
        public long TotalChatMessageSent { get; set; }
        public long TotalTimesTagged { get; set; }
        public long TotalCommandsRun { get; set; }
        public long TotalMonthsSubbed { get; set; }
        public Dictionary<string, UserPlatformData> PlatformData { get; set; } = new Dictionary<string, UserPlatformData>();
    }
}
