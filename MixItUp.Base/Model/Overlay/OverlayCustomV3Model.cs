using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayCustomV3Model : OverlayEventTrackingV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayCustomDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayCustomDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayCustomDefaultJavascript;

        [DataMember]
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public OverlayCustomV3Model() : base(OverlayItemV3Type.Custom) { }

        public override async void OnChatUserBanned(object sender, UserV2ViewModel user)
        {
            await this.CallFunction("UserBanned", new Dictionary<string, object>()
            {
                { "User", user }
            });
        }

        public override async void OnChatUserTimedOut(object sender, UserV2ViewModel user)
        {
            await this.CallFunction("UserTimeout", new Dictionary<string, object>()
            {
                { "User", user }
            });
        }

        public override async void OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            await this.CallFunction("ChatMessageReceived", OverlayChatV3Model.GetMessageProperties(message));
        }

        public override async void OnChatMessageDeleted(object sender, string messageID)
        {
            await this.CallFunction("ChatMessageDeleted", new Dictionary<string, object>()
            {
                { "MessageID", messageID }
            });
        }

        public override async void OnChatCleared(object sender, EventArgs e)
        {
            await this.CallFunction("ChatCleared", new Dictionary<string, object>());
        }

        public override async void OnFollow(object sender, UserV2ViewModel user)
        {
            await this.CallFunction("Follow", new Dictionary<string, object>()
            {
                { "User", user }
            });
        }

        public override async void OnRaid(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            await this.CallFunction("Raid", new Dictionary<string, object>()
            {
                { "User", raid.Item1 },
                { "Amount", raid.Item2 }
            });
        }

        public override async void OnSubscribe(object sender, SubscriptionDetailsModel subscription)
        {
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "User", subscription.User },
                { "Tier", subscription.Tier.ToString() },
                { "Months", subscription.Months.ToString() },
                { "Gifter", subscription.Gifter }
            };

            if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
            {
                data["Tier"] = subscription.YouTubeMembershipTier;
            }

            if (subscription.Gifter != null)
            {
                await this.CallFunction("SubscriptionGifted", data);
            }
            else
            {
                await this.CallFunction("Subscription", data);
            }
        }

        public override async void OnMassSubscription(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions)
        {
            StreamingPlatformTypeEnum platform = subscriptions.First().Platform;
            UserV2ViewModel gifter = subscriptions.First().Gifter;
            int tier = subscriptions.First().Tier;
            string membershipName = subscriptions.First().YouTubeMembershipTier;
            int amount = subscriptions.Count();

            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "Users", subscriptions.Select(s => s.User) },
                { "Tier", tier.ToString() },
                { "Amount", amount },
                { "Gifter", gifter }
            };

            if (platform == StreamingPlatformTypeEnum.YouTube)
            {
                data["Tier"] = membershipName;
            }

            await this.CallFunction("MassSubscriptionGifted", data);
        }

        public override async void OnDonation(object sender, UserDonationModel donation)
        {
            await this.CallFunction("Donation", new Dictionary<string, object>()
            {
                { "User", donation.User },
                { "Donation", donation }
            });
        }

        public override async void OnTwitchBits(object sender, TwitchBitsCheeredEventModel bitsCheered)
        {
            await this.CallFunction("TwitchBits", new Dictionary<string, object>()
            {
                { "User", bitsCheered.User },
                { "Amount", bitsCheered.Amount },
                { "Message", bitsCheered.Message }
            });
        }

        public override async void OnYouTubeSuperChat(object sender, YouTubeSuperChatViewModel superChat)
        {
            await this.CallFunction("YouTubeSuperChat", new Dictionary<string, object>()
            {
                { "User", superChat.User },
                { "Amount", superChat.Amount },
                { "AmountDisplay", superChat.AmountDisplay },
                { "Message", superChat.Message }
            });
        }

        public override async void OnTrovoSpell(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                await this.CallFunction("TrovoElixirSpell", new Dictionary<string, object>()
                {
                    { "User", spell.User },
                    { "Name", spell.Name },
                    { "Quantity", spell.Quantity },
                    { "Value", spell.Value },
                    { "Total", spell.ValueTotal }
                });
            }
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            foreach (var property in this.Properties)
            {
                properties[property.Key] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(property.Value, parameters);
            }
        }
    }
}
