using System;
using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class TwitchAction : ActionBase
    {
        public string ActionType { get; set; }
        public bool ShowInfoInChat { get; set; }
        public string Username { get; set; }
        public int AdLength { get; set; }
        public bool ClipIncludeDelay { get; set; }
        public string StreamMarkerDescription { get; set; }
        public Guid ChannelPointRewardID { get; set; }
        public bool ChannelPointRewardState { get; set; }
        public string ChannelPointRewardCostString { get; set; }
        public bool ChannelPointRewardUpdateCooldownsAndLimits { get; set; }
        public string ChannelPointRewardMaxPerStreamString { get; set; }
        public string ChannelPointRewardMaxPerUserString { get; set; }
        public string ChannelPointRewardGlobalCooldownString { get; set; }
        public string PollTitle { get; set; }
        public int PollDurationSeconds { get; set; }
        public int PollChannelPointsCost { get; set; }
        public int PollBitsCost { get; set; }
        public List<string> PollChoices { get; set; } = new List<string>();
        public string PredictionTitle { get; set; }
        public int PredictionDurationSeconds { get; set; }
        public List<string> PredictionOutcomes { get; set; } = new List<string>();
        public string TimeLength { get; set; }
        public List<ActionBase> Actions { get; set; } = new List<ActionBase>();
    }
}
