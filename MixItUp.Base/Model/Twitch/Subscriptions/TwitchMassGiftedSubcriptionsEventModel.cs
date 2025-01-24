using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Subscriptions
{
    public class TwitchMassGiftedSubcriptionsEventModel
    {
        public UserV2ViewModel Gifter { get; set; }

        public string CommunityGiftID { get; set; }

        public int TotalGifted { get; set; }

        public int LifetimeGifted { get; set; }

        public int Tier { get; set; }

        public string TierName { get; set; }

        public int TotalSubPoints { get; set; }

        public bool IsAnonymous { get; set; }

        public List<TwitchSubcriptionEventModel> Subs { get; set; } = new List<TwitchSubcriptionEventModel>();

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchMassGiftedSubcriptionsEventModel(ChatNotificationCommunitySubGift communitySubGift, UserV2ViewModel gifter, bool isAnonymous)
        {
            this.Gifter = gifter;
            this.CommunityGiftID = communitySubGift.id;

            this.TotalGifted = communitySubGift.total.GetValueOrDefault();
            this.LifetimeGifted = communitySubGift.cumulative_total.GetValueOrDefault();

            this.Tier = communitySubGift.TierNumber;
            this.TierName = this.TierName = $"{MixItUp.Base.Resources.Tier} {this.Tier}";

            this.TotalSubPoints = communitySubGift.TotalSubPoints;

            this.IsAnonymous = isAnonymous;
        }
    }
}
