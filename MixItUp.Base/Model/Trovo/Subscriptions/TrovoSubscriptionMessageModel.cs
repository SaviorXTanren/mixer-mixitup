using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Util;
using System;
using System.Text.RegularExpressions;

namespace MixItUp.Base.Model.Trovo.Subscriptions
{
    public class TrovoSubscriptionMessageModel
    {
        private const string SubscriptionRenewedMessageText = "has renewed subscription";
        private const string SubscriptionTierMessageFormatText = "Tier \\d+";
        private const string SubscriptionMonthsMessageFormatText = "\\d+ months";

        public bool IsResub { get; private set; } = false;

        public int Months { get; private set; } = 1;

        public int Tier { get; private set; } = 1;

        public ChatMessageModel Message { get; private set; }

        public TrovoSubscriptionMessageModel(ChatMessageModel message)
        {
            this.Message = message;

            if (!string.IsNullOrEmpty(message.sub_lv) && int.TryParse(message.sub_lv.Replace("L", string.Empty), out int tier))
            {
                this.Tier = tier;

                Match match = Regex.Match(message.content, SubscriptionTierMessageFormatText);
                if (match != null && match.Success)
                {
                    string[] splits = match.Value.Split(new char[] { ' ' });
                    if (splits != null && splits.Length > 1 && int.TryParse(splits[1], out tier))
                    {
                        this.Months = tier;
                    }
                }
            }

            this.IsResub = message.content.Contains(SubscriptionRenewedMessageText, StringComparison.OrdinalIgnoreCase);

            if (this.IsResub)
            {
                Match match = Regex.Match(message.content, SubscriptionMonthsMessageFormatText);
                if (match != null && match.Success)
                {
                    string[] splits = match.Value.Split(new char[] { ' ' });
                    if (splits != null && splits.Length > 0 && int.TryParse(splits[0], out int months))
                    {
                        this.Months = months;
                    }
                }
            }
            else
            {
                this.Months = 1;
            }
        }
    }
}
