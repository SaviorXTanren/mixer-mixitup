using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.Twitch.Clients.PubSub.Messages;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayEventListHeaderV3Model : OverlayHeaderV3ModelBase
    {
        public OverlayEventListHeaderV3Model() { }
    }

    [DataContract]
    public class OverlayEventListV3Model : OverlayEventTrackingV3ModelBase
    {
        public const string DetailsAmountPropertyName = "Amount";
        public const string DetailsTierPropertyName = "Tier";
        public const string DetailsMembershipNamePropertyName = "MembershipName";

        public static readonly string DefaultHTML = OverlayResources.OverlayEventListDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayEventListDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayHeaderTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayEventListDefaultJavascript;

        [DataMember]
        public OverlayEventListHeaderV3Model Header { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string BorderColor { get; set; }

        [DataMember]
        public int TotalToShow { get; set; }
        [DataMember]
        public bool AddToTop { get; set; }

        [DataMember]
        public string FollowsDetailsTemplate { get; set; }
        [DataMember]
        public string RaidsDetailsTemplate { get; set; }

        [DataMember]
        public string TwitchSubscriptionsDetailsTemplate { get; set; }
        [DataMember]
        public string TwitchResubscriptionsDetailsTemplate { get; set; }
        [DataMember]
        public string TwitchGiftedSubscriptionsDetailsTemplate { get; set; }
        [DataMember]
        public string TwitchMassGiftedSubscriptionsDetailsTemplate { get; set; }
        [DataMember]
        public string TwitchBitsDetailsTemplate { get; set; }

        [DataMember]
        public string YouTubeMembershipsDetailsTemplate { get; set; }
        [DataMember]
        public string YouTubeRenewedMembershipsDetailsTemplate { get; set; }
        [DataMember]
        public string YouTubeGiftedMembershipsDetailsTemplate { get; set; }
        [DataMember]
        public string YouTubeMassGiftedMembershipsDetailsTemplate { get; set; }
        [DataMember]
        public string YouTubeSuperChatsDetailsTemplate { get; set; }

        [DataMember]
        public string TrovoSubscriptionsDetailsTemplate { get; set; }
        [DataMember]
        public string TrovoResubscriptionsDetailsTemplate { get; set; }
        [DataMember]
        public string TrovoGiftedSubscriptionsDetailsTemplate { get; set; }
        [DataMember]
        public string TrovoMassGiftedSubscriptionsDetailsTemplate { get; set; }
        [DataMember]
        public string TrovoElixirSpellsDetailsTemplate { get; set; }

        [DataMember]
        public string DonationsDetailsTemplate { get; set; }

        [DataMember]
        public OverlayAnimationV3Model ItemAddedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model ItemRemovedAnimation { get; set; } = new OverlayAnimationV3Model();

        public OverlayEventListV3Model() : base(OverlayItemV3Type.EventList) { }

        public override async void OnFollow(object sender, UserV2ViewModel user)
        {
            await this.AddEvent(user, nameof(this.Follows), this.FollowsDetailsTemplate, new Dictionary<string, string>());
        }

        public override async void OnRaid(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            await this.AddEvent(raid.Item1, nameof(this.Raids), this.RaidsDetailsTemplate, new Dictionary<string, string>() { { DetailsAmountPropertyName, raid.Item2.ToString() } });
        }

        public override async void OnSubscribe(object sender, SubscriptionDetailsModel subscription)
        {
            if (subscription.Gifter != null)
            {
                if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    await this.AddEvent(subscription.Gifter, nameof(this.TwitchSubscriptions), this.TwitchGiftedSubscriptionsDetailsTemplate, new Dictionary<string, string>() { { DetailsTierPropertyName, subscription.Tier.ToString() } });
                }
                else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
                {
                    await this.AddEvent(subscription.Gifter, nameof(this.YouTubeMemberships), this.YouTubeGiftedMembershipsDetailsTemplate, new Dictionary<string, string>() { { DetailsMembershipNamePropertyName, subscription.Tier.ToString() } });
                }
                else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
                {
                    await this.AddEvent(subscription.Gifter, nameof(this.TrovoSubscriptions), this.TrovoGiftedSubscriptionsDetailsTemplate, new Dictionary<string, string>() { { DetailsTierPropertyName, subscription.Tier.ToString() } });
                }
            }
            else if (subscription.Months > 1)
            {
                if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    await this.AddEvent(subscription.User, nameof(this.TwitchSubscriptions), this.TwitchResubscriptionsDetailsTemplate, new Dictionary<string, string>()
                    {
                        { DetailsTierPropertyName, subscription.Tier.ToString() },
                        { DetailsAmountPropertyName, subscription.Months.ToString() }
                    });
                }
                else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
                {
                    await this.AddEvent(subscription.User, nameof(this.YouTubeMemberships), this.YouTubeRenewedMembershipsDetailsTemplate, new Dictionary<string, string>()
                    {
                        { DetailsMembershipNamePropertyName, subscription.YouTubeMembershipTier },
                        { DetailsAmountPropertyName, subscription.Months.ToString() }
                    });
                }
                else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
                {
                    await this.AddEvent(subscription.User, nameof(this.TrovoSubscriptions), this.TrovoResubscriptionsDetailsTemplate, new Dictionary<string, string>()
                    {
                        { DetailsTierPropertyName, subscription.Tier.ToString() },
                        { DetailsAmountPropertyName, subscription.Months.ToString() }
                    });
                }
            }
            else
            {
                if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    await this.AddEvent(subscription.User, nameof(this.TwitchSubscriptions), this.TwitchSubscriptionsDetailsTemplate, new Dictionary<string, string>() { { DetailsTierPropertyName, subscription.Tier.ToString() } });
                }
                else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
                {
                    await this.AddEvent(subscription.User, nameof(this.YouTubeMemberships), this.YouTubeMembershipsDetailsTemplate, new Dictionary<string, string>() { { DetailsMembershipNamePropertyName, subscription.YouTubeMembershipTier } });
                }
                else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
                {
                    await this.AddEvent(subscription.User, nameof(this.TrovoSubscriptions), this.TrovoSubscriptionsDetailsTemplate, new Dictionary<string, string>() { { DetailsTierPropertyName, subscription.Tier.ToString() } });
                }
            }
        }

        public override async void OnMassSubscription(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions)
        {
            StreamingPlatformTypeEnum platform = subscriptions.First().Platform;
            UserV2ViewModel gifter = subscriptions.First().Gifter;
            int tier = subscriptions.First().Tier;
            string membershipName = subscriptions.First().YouTubeMembershipTier;
            int amount = subscriptions.Count();

            if (platform == StreamingPlatformTypeEnum.Twitch)
            {
                await this.AddEvent(gifter, nameof(this.TwitchSubscriptions), this.TwitchMassGiftedSubscriptionsDetailsTemplate, new Dictionary<string, string>()
                {
                    { DetailsTierPropertyName, tier.ToString() },
                    { DetailsAmountPropertyName, amount.ToString() }
                });
            }
            else if (platform == StreamingPlatformTypeEnum.YouTube)
            {
                await this.AddEvent(gifter, nameof(this.YouTubeMemberships), this.YouTubeMassGiftedMembershipsDetailsTemplate, new Dictionary<string, string>()
                {
                    { DetailsMembershipNamePropertyName, membershipName },
                    { DetailsAmountPropertyName, amount.ToString() }
                });
            }
            else if (platform == StreamingPlatformTypeEnum.Trovo)
            {
                await this.AddEvent(gifter, nameof(this.TrovoSubscriptions), this.TrovoMassGiftedSubscriptionsDetailsTemplate, new Dictionary<string, string>()
                {
                    { DetailsTierPropertyName, tier.ToString() },
                    { DetailsAmountPropertyName, amount.ToString() }
                });
            }
        }

        public override async void OnDonation(object sender, UserDonationModel donation)
        {
            await this.AddEvent(donation.User, nameof(this.Donations), this.DonationsDetailsTemplate, new Dictionary<string, string>() { { DetailsAmountPropertyName, donation.AmountText } });
        }

        public override async void OnTwitchBits(object sender, TwitchBitsCheeredEventModel bitsCheered)
        {
            await this.AddEvent(bitsCheered.User, nameof(this.TwitchBits), this.TwitchBitsDetailsTemplate, new Dictionary<string, string>() { { DetailsAmountPropertyName, bitsCheered.Amount.ToString() } });
        }

        public override async void OnYouTubeSuperChat(object sender, YouTubeSuperChatViewModel superChat)
        {
            await this.AddEvent(superChat.User, nameof(this.YouTubeSuperChats), this.YouTubeSuperChatsDetailsTemplate, new Dictionary<string, string>() { { DetailsAmountPropertyName, superChat.AmountDisplay } });
        }

        public override async void OnTrovoSpell(object sender, TrovoChatSpellViewModel spell)
        {
            await this.AddEvent(spell.User, nameof(this.TrovoElixirSpells), this.TrovoElixirSpellsDetailsTemplate, new Dictionary<string, string>() { { DetailsAmountPropertyName, spell.ValueTotal.ToString() } });
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            foreach (var kvp in this.Header.GetGenerationProperties())
            {
                properties[kvp.Key] = kvp.Value;
            }

            properties[nameof(this.BackgroundColor)] = this.BackgroundColor;
            properties[nameof(this.BorderColor)] = this.BorderColor;

            properties[nameof(this.TotalToShow)] = this.TotalToShow;
            properties[nameof(this.AddToTop)] = this.AddToTop.ToString().ToLower();

            this.ItemAddedAnimation.AddAnimationProperties(properties, nameof(this.ItemAddedAnimation));
            this.ItemRemovedAnimation.AddAnimationProperties(properties, nameof(this.ItemRemovedAnimation));

            return properties;
        }

        public async Task ClearEvents()
        {
            await this.CallFunction("clear", new Dictionary<string, object>());
        }

        public async Task AddEvent(UserV2ViewModel user, string type, string template, Dictionary<string, string> properties)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return;
            }

            if (user == null)
            {
                user = ChannelSession.User;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["User"] = JObject.FromObject(user);
            data["Type"] = type;

            string details = template;
            foreach (var kvp in properties)
            {
                details = OverlayV3Service.ReplaceProperty(details, kvp.Key, kvp.Value);
            }
            details = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(details, new CommandParametersModel(user));
            data["Details"] = details;

            await this.CallFunction("add", data);
        }

        public Task TestAllEvents()
        {
            AsyncRunner.RunAsyncBackground(async (token) =>
            {
                if (this.Follows)
                {
                    this.OnFollow(this, ChannelSession.User);
                    await Task.Delay(3000);
                }

                if (this.Raids)
                {
                    this.OnRaid(this, new Tuple<UserV2ViewModel, int>(ChannelSession.User, 10));
                    await Task.Delay(3000);
                }

                if (this.TwitchSubscriptions)
                {
                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, ChannelSession.User));
                    await Task.Delay(3000);

                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, ChannelSession.User, months: 10));
                    await Task.Delay(3000);

                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, ChannelSession.User, ChannelSession.User));
                    await Task.Delay(3000);

                    List<SubscriptionDetailsModel> subs = new List<SubscriptionDetailsModel>();
                    for (int i = 0; i < 5; i++)
                    {
                        subs.Add(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, ChannelSession.User, ChannelSession.User));
                    }
                    this.OnMassSubscription(this, subs);
                    await Task.Delay(3000);
                }

                if (this.TwitchBits)
                {
                    this.OnTwitchBits(this, new TwitchBitsCheeredEventModel(ChannelSession.User, 100, new TwitchChatMessageViewModel(ChannelSession.User, "Hello World")));
                    await Task.Delay(3000);
                }

                if (this.YouTubeMemberships)
                {
                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.YouTube, ChannelSession.User, youTubeMembershipTier: "Foobar"));
                    await Task.Delay(3000);

                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.YouTube, ChannelSession.User, months: 10, youTubeMembershipTier: "Foobar"));
                    await Task.Delay(3000);

                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.YouTube, ChannelSession.User, ChannelSession.User, youTubeMembershipTier: "Foobar"));
                    await Task.Delay(3000);

                    List<SubscriptionDetailsModel> subs = new List<SubscriptionDetailsModel>();
                    for (int i = 0; i < 5; i++)
                    {
                        subs.Add(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.YouTube, ChannelSession.User, ChannelSession.User, youTubeMembershipTier: "Foobar"));
                    }
                    this.OnMassSubscription(this, subs);
                    await Task.Delay(3000);
                }

                if (this.YouTubeSuperChats)
                {
                    this.OnYouTubeSuperChat(this, new YouTubeSuperChatViewModel(new LiveChatSuperChatDetails() { AmountDisplayString = "$12.34" }, ChannelSession.User));
                    await Task.Delay(3000);
                }

                if (this.TrovoSubscriptions)
                {
                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, ChannelSession.User));
                    await Task.Delay(3000);

                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, ChannelSession.User, months: 10));
                    await Task.Delay(3000);

                    this.OnSubscribe(this, new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, ChannelSession.User, ChannelSession.User));
                    await Task.Delay(3000);

                    List<SubscriptionDetailsModel> subs = new List<SubscriptionDetailsModel>();
                    for (int i = 0; i < 5; i++)
                    {
                        subs.Add(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, ChannelSession.User, ChannelSession.User));
                    }
                    this.OnMassSubscription(this, subs);
                    await Task.Delay(3000);
                }

                if (this.TrovoElixirSpells)
                {
                    this.OnTrovoSpell(this, new TrovoChatSpellViewModel(ChannelSession.User, new ChatMessageModel() { content = "" })
                    {
                        Contents = new TrovoChatSpellContentModel()
                        {
                            num = 10,
                            gift_value = 10
                        }
                    });
                    await Task.Delay(3000);
                }

                if (this.Donations)
                {
                    this.OnDonation(this, new UserDonationModel()
                    {
                        Source = UserDonationSourceEnum.Streamlabs,

                        User = ChannelSession.User,
                        Username = ChannelSession.User.Username,

                        Message = "Text",

                        Amount = 12.34,

                        DateTime = DateTimeOffset.Now,
                    });
                    await Task.Delay(3000);
                };
            }, CancellationToken.None);

            return Task.CompletedTask;
        }
    }
}
