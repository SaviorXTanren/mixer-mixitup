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
        public string Notes { get; set; }
        public Dictionary<string, UserPlatformData> PlatformData { get; set; } = new Dictionary<string, UserPlatformData>();
    }
}
