using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay
{
    public enum OverlayEventListV3TestTypeEnum
    {
        Follow,
        Raid,

        Subscribe,
        Resubscribe,
        SubscriptionGifted,
        MassSubscriptionGifted,

        Donation,

        TwitchBits,

        YouTubeSuperChat,

        TrovoElixirSpell,
    }

    public class OverlayEventListHeaderV3ViewModel : OverlayHeaderV3ViewModelBase
    {
        public OverlayEventListHeaderV3ViewModel() { }

        public OverlayEventListHeaderV3ViewModel(OverlayEventListHeaderV3Model model) : base(model) { }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayEventListHeaderV3Model result = new OverlayEventListHeaderV3Model();
            this.AssignProperties(result);
            return result;
        }
    }

    public class OverlayEventListV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayEventListV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayEventListV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayEventListV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public OverlayEventListHeaderV3ViewModel Header
        {
            get { return this.header; }
            set
            {
                this.header = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEventListHeaderV3ViewModel header;

        public string Height
        {
            get { return this.height > 0 ? this.height.ToString() : string.Empty; }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int height;

        public int TotalToShow
        {
            get { return this.totalToShow; }
            set
            {
                this.totalToShow = value;
                this.NotifyPropertyChanged();
            }
        }
        private int totalToShow;

        public bool AddToTop
        {
            get { return this.addToTop; }
            set
            {
                this.addToTop = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool addToTop;

        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                this.borderColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string borderColor;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;

        public bool Follows
        {
            get { return this.follows; }
            set
            {
                this.follows = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool follows;

        public string FollowsDetailsTemplate
        {
            get { return this.followsDetailsTemplate; }
            set
            {
                this.followsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string followsDetailsTemplate;

        public bool Raids
        {
            get { return this.raids; }
            set
            {
                this.raids = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool raids;

        public string RaidsDetailsTemplate
        {
            get { return this.raidsDetailsTemplate; }
            set
            {
                this.raidsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string raidsDetailsTemplate;

        public bool TwitchSubscriptions
        {
            get { return this.twitchSubscriptions; }
            set
            {
                this.twitchSubscriptions = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool twitchSubscriptions;

        public string TwitchSubscriptionsDetailsTemplate
        {
            get { return this.twitchSubscriptionsDetailsTemplate; }
            set
            {
                this.twitchSubscriptionsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string twitchSubscriptionsDetailsTemplate;

        public string TwitchResubscriptionsDetailsTemplate
        {
            get { return this.twitchResubscriptionsDetailsTemplate; }
            set
            {
                this.twitchResubscriptionsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string twitchResubscriptionsDetailsTemplate;

        public string TwitchGiftedSubscriptionsDetailsTemplate
        {
            get { return this.twitchGiftedSubscriptionsDetailsTemplate; }
            set
            {
                this.twitchGiftedSubscriptionsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string twitchGiftedSubscriptionsDetailsTemplate;

        public string TwitchMassGiftedSubscriptionsDetailsTemplate
        {
            get { return this.twitchMassGiftedSubscriptionsDetailsTemplate; }
            set
            {
                this.twitchMassGiftedSubscriptionsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string twitchMassGiftedSubscriptionsDetailsTemplate;

        public bool TwitchBits
        {
            get { return this.twitchBits; }
            set
            {
                this.twitchBits = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool twitchBits;

        public string TwitchBitsDetailsTemplate
        {
            get { return this.twitchBitsDetailsTemplate; }
            set
            {
                this.twitchBitsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string twitchBitsDetailsTemplate;

        public bool YouTubeMemberships
        {
            get { return this.youTubeMemberships; }
            set
            {
                this.youTubeMemberships = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool youTubeMemberships;

        public string YouTubeMembershipsDetailsTemplate
        {
            get { return this.youTubeMembershipsDetailsTemplate; }
            set
            {
                this.youTubeMembershipsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string youTubeMembershipsDetailsTemplate;

        public string YouTubeRenewedMembershipsDetailsTemplate
        {
            get { return this.youTubeRenewedMembershipsDetailsTemplate; }
            set
            {
                this.youTubeRenewedMembershipsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string youTubeRenewedMembershipsDetailsTemplate;

        public string YouTubeGiftedMembershipsDetailsTemplate
        {
            get { return this.youTubeGiftedMembershipsDetailsTemplate; }
            set
            {
                this.youTubeGiftedMembershipsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string youTubeGiftedMembershipsDetailsTemplate;

        public string YouTubeMassGiftedMembershipsDetailsTemplate
        {
            get { return this.youTubeMassGiftedMembershipsDetailsTemplate; }
            set
            {
                this.youTubeMassGiftedMembershipsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string youTubeMassGiftedMembershipsDetailsTemplate;

        public bool YouTubeSuperChats
        {
            get { return this.youTubeSuperChats; }
            set
            {
                this.youTubeSuperChats = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool youTubeSuperChats;

        public string YouTubeSuperChatsDetailsTemplate
        {
            get { return this.youTubeSuperChatsDetailsTemplate; }
            set
            {
                this.youTubeSuperChatsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string youTubeSuperChatsDetailsTemplate;

        public bool TrovoSubscriptions
        {
            get { return this.trovoSubscriptions; }
            set
            {
                this.trovoSubscriptions = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool trovoSubscriptions;

        public string TrovoSubscriptionsDetailsTemplate
        {
            get { return this.trovoSubscriptionsDetailsTemplate; }
            set
            {
                this.trovoSubscriptionsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string trovoSubscriptionsDetailsTemplate;

        public string TrovoResubscriptionsDetailsTemplate
        {
            get { return this.trovoResubscriptionsDetailsTemplate; }
            set
            {
                this.trovoResubscriptionsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string trovoResubscriptionsDetailsTemplate;

        public string TrovoGiftedSubscriptionsDetailsTemplate
        {
            get { return this.trovoGiftedSubscriptionsDetailsTemplate; }
            set
            {
                this.trovoGiftedSubscriptionsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string trovoGiftedSubscriptionsDetailsTemplate;

        public string TrovoMassGiftedSubscriptionsDetailsTemplate
        {
            get { return this.trovoMassGiftedSubscriptionsDetailsTemplate; }
            set
            {
                this.trovoMassGiftedSubscriptionsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string trovoMassGiftedSubscriptionsDetailsTemplate;

        public bool TrovoElixirSpells
        {
            get { return this.trovoElixirSpells; }
            set
            {
                this.trovoElixirSpells = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool trovoElixirSpells;

        public string TrovoElixirSpellsDetailsTemplate
        {
            get { return this.trovoElixirSpellsDetailsTemplate; }
            set
            {
                this.trovoElixirSpellsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string trovoElixirSpellsDetailsTemplate;

        public bool Donations
        {
            get { return this.donations; }
            set
            {
                this.donations = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool donations;

        public string DonationsDetailsTemplate
        {
            get { return this.donationsDetailsTemplate; }
            set
            {
                this.donationsDetailsTemplate = value;
                this.NotifyPropertyChanged();
            }
        }
        private string donationsDetailsTemplate;

        public OverlayAnimationV3ViewModel ItemAddedAnimation;
        public OverlayAnimationV3ViewModel ItemRemovedAnimation;

        public OverlayEventListV3ViewModel()
            : base(OverlayItemV3Type.EventList)
        {
            this.Header = new OverlayEventListHeaderV3ViewModel();

            this.FontSize = 14;

            this.width = 250;
            this.height = 100;

            this.BorderColor = "Black";
            this.BackgroundColor = "White";

            this.TotalToShow = 5;
            this.AddToTop = true;

            this.FollowsDetailsTemplate = Resources.Followed;
            this.RaidsDetailsTemplate = $"{Resources.Raid} - {{Amount}} {Resources.Viewers}";

            this.TwitchSubscriptionsDetailsTemplate = $"{Resources.Subscribed} - {Resources.Tier} {{Tier}}";
            this.TwitchResubscriptionsDetailsTemplate = $"{Resources.Resubscribed} - {Resources.Tier} {{Tier}} - {{Amount}} Months";
            this.TwitchGiftedSubscriptionsDetailsTemplate = $"{Resources.GiftedSubscription} - {Resources.Tier} {{Tier}}";
            this.TwitchMassGiftedSubscriptionsDetailsTemplate = $"{Resources.GiftedSubscription} - {Resources.Tier} {{Tier}} x{{Amount}}";

            this.TwitchBitsDetailsTemplate = $"{{Amount}} {Resources.Bits}";

            this.YouTubeMembershipsDetailsTemplate = $"{Resources.Membership} - {{MembershipName}}";
            this.YouTubeRenewedMembershipsDetailsTemplate = $"{Resources.RenewedMembership} - {{MembershipName}} - {{Amount}} Months";
            this.YouTubeGiftedMembershipsDetailsTemplate = $"{Resources.GiftedMembership} - {{MembershipName}}";
            this.YouTubeMassGiftedMembershipsDetailsTemplate = $"{Resources.GiftedMembership} - {{MembershipName}} x{{Amount}}";

            this.YouTubeSuperChatsDetailsTemplate = $"{Resources.SuperChat} - {{Amount}}";

            this.TrovoSubscriptionsDetailsTemplate = $"{Resources.Subscribed} - {Resources.Tier} {{Tier}}";
            this.TrovoResubscriptionsDetailsTemplate = $"{Resources.Resubscribed} - {Resources.Tier} {{Tier}} - {{Amount}} Months";
            this.TrovoGiftedSubscriptionsDetailsTemplate = $"{Resources.GiftedSubscription} - {Resources.Tier} {{Tier}}";
            this.TrovoMassGiftedSubscriptionsDetailsTemplate = $"{Resources.GiftedSubscription} - {Resources.Tier} {{Tier}} x{{Amount}}";

            this.TrovoElixirSpellsDetailsTemplate = $"{{Amount}} {Resources.Elixir}";

            this.DonationsDetailsTemplate = $"{Resources.Donation} - {{Amount}}";

            this.ItemAddedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemAdded, new OverlayAnimationV3Model());
            this.ItemRemovedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemRemoved, new OverlayAnimationV3Model());

            this.Initialize();
        }

        public OverlayEventListV3ViewModel(OverlayEventListV3Model item)
            : base(item)
        {
            this.Header = new OverlayEventListHeaderV3ViewModel(item.Header);

            this.height = item.Height;

            this.BorderColor = item.BorderColor;
            this.BackgroundColor = item.BackgroundColor;

            this.TotalToShow = item.TotalToShow;
            this.AddToTop = item.AddToTop;

            this.Follows = item.Follows;
            this.FollowsDetailsTemplate = item.FollowsDetailsTemplate;

            this.Raids = item.Raids;
            this.RaidsDetailsTemplate = item.RaidsDetailsTemplate;

            this.TwitchSubscriptions = item.TwitchSubscriptions;
            this.TwitchSubscriptionsDetailsTemplate = item.TwitchSubscriptionsDetailsTemplate;
            this.TwitchResubscriptionsDetailsTemplate = item.TwitchResubscriptionsDetailsTemplate;
            this.TwitchGiftedSubscriptionsDetailsTemplate = item.TwitchGiftedSubscriptionsDetailsTemplate;
            this.TwitchMassGiftedSubscriptionsDetailsTemplate = item.TwitchMassGiftedSubscriptionsDetailsTemplate;

            this.TwitchBits = item.TwitchBits;
            this.TwitchBitsDetailsTemplate = item.TwitchBitsDetailsTemplate;

            this.YouTubeMemberships = item.YouTubeMemberships;
            this.YouTubeMembershipsDetailsTemplate = item.YouTubeMembershipsDetailsTemplate;
            this.YouTubeRenewedMembershipsDetailsTemplate = item.YouTubeRenewedMembershipsDetailsTemplate;
            this.YouTubeGiftedMembershipsDetailsTemplate = item.YouTubeGiftedMembershipsDetailsTemplate;
            this.YouTubeMassGiftedMembershipsDetailsTemplate = item.YouTubeMassGiftedMembershipsDetailsTemplate;

            this.YouTubeSuperChats = item.YouTubeSuperChats;
            this.YouTubeSuperChatsDetailsTemplate = item.YouTubeSuperChatsDetailsTemplate;

            this.TrovoSubscriptions = item.TrovoSubscriptions;
            this.TrovoSubscriptionsDetailsTemplate = item.TrovoSubscriptionsDetailsTemplate;
            this.TrovoResubscriptionsDetailsTemplate = item.TrovoResubscriptionsDetailsTemplate;
            this.TrovoGiftedSubscriptionsDetailsTemplate = item.TrovoGiftedSubscriptionsDetailsTemplate;
            this.TrovoMassGiftedSubscriptionsDetailsTemplate = item.TrovoMassGiftedSubscriptionsDetailsTemplate;

            this.TrovoElixirSpells = item.TrovoElixirSpells;
            this.TrovoElixirSpellsDetailsTemplate = item.TrovoElixirSpellsDetailsTemplate;

            this.Donations = item.Donations;
            this.DonationsDetailsTemplate = item.DonationsDetailsTemplate;

            this.ItemAddedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemAdded, item.ItemAddedAnimation);
            this.ItemRemovedAnimation = new OverlayAnimationV3ViewModel(Resources.ItemRemoved, item.ItemRemovedAnimation);

            this.Initialize();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            object result = await DialogHelper.ShowEnumDropDown(EnumHelper.GetEnumList<OverlayEventListV3TestTypeEnum>());
            if (result != null)
            {
                OverlayEventListV3Model eventList = (OverlayEventListV3Model)widget.Item;

                OverlayEventListV3TestTypeEnum type = (OverlayEventListV3TestTypeEnum)result;
                if (type == OverlayEventListV3TestTypeEnum.Follow)
                {
                    eventList.OnFollow(this, ChannelSession.User);
                }
                else if (type == OverlayEventListV3TestTypeEnum.Raid)
                {
                    eventList.OnRaid(this, new Tuple<UserV2ViewModel, int>(ChannelSession.User, 10));
                }
                else if (type == OverlayEventListV3TestTypeEnum.Subscribe)
                {
                    eventList.OnSubscribe(this, new SubscriptionDetailsModel(ChannelSession.User.Platform, ChannelSession.User, tier: 2, youTubeMembershipTier: "Foobar"));
                }
                else if (type == OverlayEventListV3TestTypeEnum.Resubscribe)
                {
                    eventList.OnSubscribe(this, new SubscriptionDetailsModel(ChannelSession.User.Platform, ChannelSession.User, months: 5, tier: 2, youTubeMembershipTier: "Foobar"));
                }
                else if (type == OverlayEventListV3TestTypeEnum.SubscriptionGifted)
                {
                    eventList.OnSubscribe(this, new SubscriptionDetailsModel(ChannelSession.User.Platform, ChannelSession.User, ChannelSession.User, months: 5, tier: 2, youTubeMembershipTier: "Foobar"));
                }
                else if (type == OverlayEventListV3TestTypeEnum.MassSubscriptionGifted)
                {
                    List<SubscriptionDetailsModel> subscriptions = new List<SubscriptionDetailsModel>();
                    for (int i = 0; i < 5; i++)
                    {
                        subscriptions.Add(new SubscriptionDetailsModel(ChannelSession.User.Platform, ChannelSession.User, ChannelSession.User, months: 5, tier: 2, youTubeMembershipTier: "Foobar"));
                    }
                    eventList.OnMassSubscription(this, subscriptions);
                }
                else if (type == OverlayEventListV3TestTypeEnum.Donation)
                {
                    eventList.OnDonation(this, new UserDonationModel()
                    {
                        Source = UserDonationSourceEnum.Streamlabs,

                        User = ChannelSession.User,
                        Username = ChannelSession.User.Username,

                        Message = "Text",

                        Amount = 12.34,

                        DateTime = DateTimeOffset.Now,
                    });
                }
                else if (type == OverlayEventListV3TestTypeEnum.TwitchBits)
                {
                    eventList.OnTwitchBits(this, new TwitchBitsCheeredEventModel(ChannelSession.User, 100, new TwitchChatMessageViewModel(ChannelSession.User, "Hello World")));
                }
                else if (type == OverlayEventListV3TestTypeEnum.YouTubeSuperChat)
                {
                    eventList.OnYouTubeSuperChat(this, new YouTubeSuperChatViewModel(new LiveChatSuperChatDetails()
                    {
                        AmountDisplayString = "$12.34",
                        UserComment = "Hello World"
                    }, ChannelSession.User));
                }
                else if (type == OverlayEventListV3TestTypeEnum.TrovoElixirSpell)
                {
                    eventList.OnTrovoSpell(this, new TrovoChatSpellViewModel(ChannelSession.User, new ChatMessageModel() { content = "" })
                    {
                        Contents = new TrovoChatSpellContentModel()
                        {
                            gift = "Foobar",
                            value_type = TrovoChatSpellViewModel.ElixirValueType,
                            num = 10,
                            gift_value = 10,
                        }
                    });
                }
            }

            await base.TestWidget(widget);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayEventListV3Model result = new OverlayEventListV3Model()
            {
                Height = this.height,

                BorderColor = this.BorderColor,
                BackgroundColor = this.BackgroundColor,

                TotalToShow = this.TotalToShow,
                AddToTop = this.AddToTop,

                Follows = this.Follows,
                FollowsDetailsTemplate = this.FollowsDetailsTemplate,

                Raids = this.Raids,
                RaidsDetailsTemplate = this.RaidsDetailsTemplate,

                TwitchSubscriptions = this.TwitchSubscriptions,
                TwitchSubscriptionsDetailsTemplate = this.TwitchSubscriptionsDetailsTemplate,
                TwitchResubscriptionsDetailsTemplate = this.TwitchResubscriptionsDetailsTemplate,
                TwitchGiftedSubscriptionsDetailsTemplate = this.TwitchGiftedSubscriptionsDetailsTemplate,
                TwitchMassGiftedSubscriptionsDetailsTemplate = this.TwitchMassGiftedSubscriptionsDetailsTemplate,

                TwitchBits = this.TwitchBits,
                TwitchBitsDetailsTemplate = this.TwitchBitsDetailsTemplate,

                YouTubeMemberships = this.YouTubeMemberships,
                YouTubeMembershipsDetailsTemplate = this.YouTubeMembershipsDetailsTemplate,
                YouTubeRenewedMembershipsDetailsTemplate = this.YouTubeRenewedMembershipsDetailsTemplate,
                YouTubeGiftedMembershipsDetailsTemplate = this.YouTubeGiftedMembershipsDetailsTemplate,
                YouTubeMassGiftedMembershipsDetailsTemplate = this.YouTubeMassGiftedMembershipsDetailsTemplate,

                YouTubeSuperChats = this.YouTubeSuperChats,
                YouTubeSuperChatsDetailsTemplate = this.YouTubeSuperChatsDetailsTemplate,

                TrovoSubscriptions = this.TrovoSubscriptions,
                TrovoSubscriptionsDetailsTemplate = this.TrovoSubscriptionsDetailsTemplate,
                TrovoResubscriptionsDetailsTemplate = this.TrovoResubscriptionsDetailsTemplate,
                TrovoGiftedSubscriptionsDetailsTemplate = this.TrovoGiftedSubscriptionsDetailsTemplate,
                TrovoMassGiftedSubscriptionsDetailsTemplate = this.TrovoMassGiftedSubscriptionsDetailsTemplate,

                TrovoElixirSpells = this.TrovoElixirSpells,
                TrovoElixirSpellsDetailsTemplate = this.TrovoElixirSpellsDetailsTemplate,

                Donations = this.Donations,
                DonationsDetailsTemplate = this.DonationsDetailsTemplate,
            };

            this.AssignProperties(result);

            result.Header = (OverlayEventListHeaderV3Model)this.Header.GetItem();

            result.ItemAddedAnimation = this.ItemAddedAnimation.GetAnimation();
            result.ItemRemovedAnimation = this.ItemRemovedAnimation.GetAnimation();

            return result;
        }

        private void Initialize()
        {
            this.Animations.Add(this.ItemAddedAnimation);
            this.Animations.Add(this.ItemRemovedAnimation);

            this.Header.PropertyChanged += (sender, e) =>
            {
                this.NotifyPropertyChanged("X");
            };
        }
    }
}
