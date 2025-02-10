using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.Model.Twitch.Subscriptions
{
    public class TwitchSubcriptionEventModel
    {
        public const string PrimeSubPlan = "Prime";

        public UserV2ViewModel User { get; set; }
        public UserV2ViewModel Gifter { get; set; }

        public int Tier { get; set; } = 1;
        public string TierName { get; set; }

        public string PlanName { get; set; }

        public TwitchChatMessageViewModel Message { get; set; }

        public int Duration { get; set; }
        public int Streak { get; set; }
        public int Cumulative { get; set; }

        public int SubPoints { get; set; }

        public bool IsAnonymous { get; set; }
        public bool IsPrime { get; set; }
        public bool IsPrimeUpgrade { get; set; }
        public bool IsGiftedUpgrade { get; set; }

        public string CommunityGiftID { get; set; }

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchSubcriptionEventModel(UserV2ViewModel user, ChatNotification notification, UserV2ViewModel gifter = null)
        {
            this.User = user;
            this.Gifter = gifter;

            this.Message = new TwitchChatMessageViewModel(notification, user);

            if (notification.NoticeType == ChatNotificationType.sub)
            {
                this.Tier = notification.sub.TierNumber;
                this.SubPoints = notification.sub.SubPoints;
                this.IsPrime = notification.sub.is_prime;
            }
            else if (notification.NoticeType == ChatNotificationType.resub)
            {
                this.Tier = notification.resub.TierNumber;
                this.Duration = notification.resub.duration_months.GetValueOrDefault();
                this.Streak = notification.resub.streak_months.GetValueOrDefault();
                this.Cumulative = notification.resub.cumulative_months.GetValueOrDefault();
                this.SubPoints = notification.resub.SubPoints;
                this.IsPrime = notification.resub.is_prime.GetValueOrDefault();
                this.IsAnonymous = notification.resub.gifter_is_anonymous.GetValueOrDefault();
            }
            else if (notification.NoticeType == ChatNotificationType.sub_gift)
            {
                this.Tier = notification.sub_gift.TierNumber;
                this.Duration = notification.sub_gift.duration_months.GetValueOrDefault();
                this.SubPoints = notification.sub_gift.SubPoints;
                this.CommunityGiftID = notification.sub_gift.community_gift_id;
                this.IsAnonymous = notification.chatter_is_anonymous;
            }
            else if (notification.NoticeType == ChatNotificationType.gift_paid_upgrade)
            {
                this.IsGiftedUpgrade = true;
            }
            else if (notification.NoticeType == ChatNotificationType.prime_paid_upgrade)
            {
                this.IsPrimeUpgrade = true;
            }

            // Shared
            else if (notification.NoticeType == ChatNotificationType.shared_chat_sub)
            {
                this.Tier = notification.shared_chat_sub.TierNumber;
                this.SubPoints = notification.shared_chat_sub.SubPoints;
                this.IsPrime = notification.shared_chat_sub.is_prime;
            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_resub)
            {
                this.Tier = notification.shared_chat_resub.TierNumber;
                this.Duration = notification.shared_chat_resub.duration_months.GetValueOrDefault();
                this.Streak = notification.shared_chat_resub.streak_months.GetValueOrDefault();
                this.Cumulative = notification.shared_chat_resub.cumulative_months.GetValueOrDefault();
                this.SubPoints = notification.shared_chat_resub.SubPoints;
                this.IsPrime = notification.shared_chat_resub.is_prime.GetValueOrDefault();
                this.IsAnonymous = notification.shared_chat_resub.gifter_is_anonymous.GetValueOrDefault();
            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_sub_gift)
            {
                this.Tier = notification.shared_chat_sub_gift.TierNumber;
                this.Duration = notification.shared_chat_sub_gift.duration_months.GetValueOrDefault();
                this.SubPoints = notification.shared_chat_sub_gift.SubPoints;
                this.CommunityGiftID = notification.shared_chat_sub_gift.community_gift_id;
                this.IsAnonymous = notification.chatter_is_anonymous;
            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_pay_it_forward)
            {
                this.IsGiftedUpgrade = true;
            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_prime_paid_upgrade)
            {
                this.IsPrimeUpgrade = true;
            }

            this.SetTierName();
        }

        public void SetSubData(SubscriptionModel subData)
        {
            this.Tier = TwitchClient.GetSubTierNumberFromText(subData.tier);
            this.SubPoints = TwitchClient.GetSubPoints(this.Tier);

            this.SetTierName();
        }

        private void SetTierName()
        {
            if (this.IsPrime)
            {
                this.TierName = PrimeSubPlan;
            }
            else
            {
                this.TierName = $"{MixItUp.Base.Resources.Tier} {this.Tier}";
            }
            this.PlanName = this.TierName;
        }
    }
}
