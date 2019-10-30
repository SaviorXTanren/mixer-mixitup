using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayEndCreditsSectionTypeEnum
    {
        [Name("Viewers")]
        Chatters,
        [Name("New Followers")]
        Followers,
        Hosts,
        [Name("New Subscribers")]
        NewSubscribers,
        Resubscribers,
        [Name("Gifted Subs")]
        GiftedSubs,
        Donations,
        Sparks,
        Embers,
        Subscribers,
        Moderators,
    }

    public enum OverlayEndCreditsSpeedEnum
    {
        Fast = 10,
        Medium = 20,
        Slow = 30
    }

    [DataContract]
    public class OverlayEndCreditsSectionModel
    {
        [DataMember]
        public OverlayEndCreditsSectionTypeEnum SectionType { get; set; }
        [DataMember]
        public string SectionHTML { get; set; }
        [DataMember]
        public string UserHTML { get; set; }
    }

    [DataContract]
    public class OverlayEndCreditsItemModel : OverlayHTMLTemplateItemModelBase
    {
        public const string CreditsWrapperHTML = @"<div id=""titles"" style=""background-color: {0}""><div id=""credits"">{1}</div></div>";

        public const string TitleHTMLTemplate = @"<div id=""the-end"">Stream Credits</div>";

        public const string SectionHTMLTemplate = @"<h1 style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">{NAME}</h1>";

        public const string UserHTMLTemplate = @"<p style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">{NAME}</p>";

        public const string UserDetailsHTMLTemplate = @"<dt style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">{NAME}</dt><dd style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">{DETAILS}</dd>";

        public const string SectionSeparatorHTML = @"<div class=""clearfix""></div>";

        [DataMember]
        public string TitleTemplate { get; set; }
        [DataMember]
        public Dictionary<OverlayEndCreditsSectionTypeEnum, OverlayEndCreditsSectionModel> SectionTemplates { get; set; } = new Dictionary<OverlayEndCreditsSectionTypeEnum, OverlayEndCreditsSectionModel>();

        [DataMember]
        public string BackgroundColor { get; set; }

        [DataMember]
        public string SectionTextColor { get; set; }
        [DataMember]
        public string SectionTextFont { get; set; }
        [DataMember]
        public int SectionTextSize { get; set; }

        [DataMember]
        public string ItemTextColor { get; set; }
        [DataMember]
        public string ItemTextFont { get; set; }
        [DataMember]
        public int ItemTextSize { get; set; }

        [DataMember]
        public OverlayEndCreditsSpeedEnum Speed { get; set; }
        [DataMember]
        public int SpeedNumber { get { return (int)this.Speed; } }

        private HashSet<uint> viewers = new HashSet<uint>();
        private HashSet<uint> subs = new HashSet<uint>();
        private HashSet<uint> mods = new HashSet<uint>();
        private HashSet<uint> follows = new HashSet<uint>();
        private HashSet<uint> hosts = new HashSet<uint>();
        private HashSet<uint> newSubs = new HashSet<uint>();
        private Dictionary<uint, uint> resubs = new Dictionary<uint, uint>();
        private Dictionary<uint, uint> giftedSubs = new Dictionary<uint, uint>();
        private Dictionary<uint, double> donations = new Dictionary<uint, double>();
        private Dictionary<uint, uint> sparks = new Dictionary<uint, uint>();
        private Dictionary<uint, uint> embers = new Dictionary<uint, uint>();

        public OverlayEndCreditsItemModel() : base() { }

        public OverlayEndCreditsItemModel(string titleTemplate, Dictionary<OverlayEndCreditsSectionTypeEnum, OverlayEndCreditsSectionModel> sectionTemplates, string backgroundColor,
            string sectionTextColor, string sectionTextFont, int sectionTextSize, string itemTextColor, string itemTextFont, int itemTextSize,
            OverlayEndCreditsSpeedEnum speed)
            : base(OverlayItemModelTypeEnum.EndCredits, string.Empty)
        {
            this.TitleTemplate = titleTemplate;
            this.SectionTemplates = sectionTemplates;
            this.BackgroundColor = backgroundColor;
            this.SectionTextColor = sectionTextColor;
            this.SectionTextFont = sectionTextFont;
            this.SectionTextSize = sectionTextSize;
            this.ItemTextColor = itemTextColor;
            this.ItemTextFont = itemTextFont;
            this.ItemTextSize = itemTextSize;
            this.Speed = speed;
        }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        public override async Task LoadTestData()
        {
            UserViewModel user = await ChannelSession.GetCurrentUser();
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Chatters))
            {
                this.viewers.Add(user.ID);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Subscribers))
            {
                this.subs.Add(user.ID);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Moderators))
            {
                this.mods.Add(user.ID);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Followers))
            {
                this.follows.Add(user.ID);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Hosts))
            {
                this.hosts.Add(user.ID);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.NewSubscribers))
            {
                this.newSubs.Add(user.ID);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Resubscribers))
            {
                this.resubs[user.ID] = 5;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.GiftedSubs))
            {
                this.giftedSubs[user.ID] = 5;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Donations))
            {
                this.donations[user.ID] = 12.34;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Sparks))
            {
                this.sparks[user.ID] = 10;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Embers))
            {
                this.embers[user.ID] = 10;
            }
        }

        public override async Task Initialize()
        {
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Hosts))
            {
                GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Followers))
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Hosts))
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.NewSubscribers))
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Resubscribers))
            {
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.GiftedSubs))
            {
                GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Donations))
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Sparks))
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Embers))
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }

            await base.Initialize();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnChatMessageReceived -= GlobalEvents_OnChatMessageReceived;
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;

            this.viewers.Clear();
            this.subs.Clear();
            this.mods.Clear();
            this.follows.Clear();
            this.hosts.Clear();
            this.newSubs.Clear();
            this.resubs.Clear();
            this.giftedSubs.Clear();
            this.donations.Clear();
            this.sparks.Clear();
            this.embers.Clear();

            await base.Disable();
        }

        protected override async Task PerformReplacements(JObject jobj, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine(SectionSeparatorHTML);
            htmlBuilder.AppendLine(await this.ReplaceStringWithSpecialModifiers(this.TitleTemplate, await ChannelSession.GetCurrentUser(), new List<string>(), new Dictionary<string, string>()));

            foreach (var kvp in this.SectionTemplates)
            {
                Dictionary<UserViewModel, string> items = new Dictionary<UserViewModel, string>();
                switch (kvp.Key)
                {
                    case OverlayEndCreditsSectionTypeEnum.Chatters: items = this.GetUsersDictionary(this.viewers); break;
                    case OverlayEndCreditsSectionTypeEnum.Subscribers: items = this.GetUsersDictionary(this.subs); break;
                    case OverlayEndCreditsSectionTypeEnum.Moderators: items = this.GetUsersDictionary(this.mods); break;
                    case OverlayEndCreditsSectionTypeEnum.Followers: items = this.GetUsersDictionary(this.follows); break;
                    case OverlayEndCreditsSectionTypeEnum.Hosts: items = this.GetUsersDictionary(this.hosts); break;
                    case OverlayEndCreditsSectionTypeEnum.NewSubscribers: items = this.GetUsersDictionary(this.newSubs); break;
                    case OverlayEndCreditsSectionTypeEnum.Resubscribers: items = this.GetUsersDictionary(this.resubs); break;
                    case OverlayEndCreditsSectionTypeEnum.GiftedSubs: items = this.GetUsersDictionary(this.giftedSubs); break;
                    case OverlayEndCreditsSectionTypeEnum.Donations: items = this.GetUsersDictionary(this.donations); break;
                    case OverlayEndCreditsSectionTypeEnum.Sparks: items = this.GetUsersDictionary(this.sparks); break;
                    case OverlayEndCreditsSectionTypeEnum.Embers: items = this.GetUsersDictionary(this.embers); break;
                }
                await this.PerformSectionTemplateReplacement(htmlBuilder, kvp.Key, items);
            }

            jobj["HTML"] = string.Format(CreditsWrapperHTML, this.BackgroundColor, htmlBuilder.ToString());
        }

        private void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (message.User != null && !message.User.IgnoreForQueries)
            {
                if (message.User.ID.Equals(ChannelSession.MixerStreamerUser.id))
                {
                    return;
                }
                if (ChannelSession.MixerBotUser != null && message.User.ID.Equals(ChannelSession.MixerBotUser.id))
                {
                    return;
                }

                this.viewers.Add(message.User.ID);
                if (message.User.MixerRoles.Contains(MixerRoleEnum.Subscriber) || message.User.IsEquivalentToMixerSubscriber())
                {
                    this.subs.Add(message.User.ID);
                }
                if (message.User.MixerRoles.Contains(MixerRoleEnum.Mod) || message.User.MixerRoles.Contains(MixerRoleEnum.ChannelEditor))
                {
                    this.mods.Add(message.User.ID);
                }
            }
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
            }
        }

        private void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.ID))
            {
                this.hosts.Add(host.Item1.ID);
            }
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.newSubs.Contains(user.ID))
            {
                this.newSubs.Add(user.ID);
            }
        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.resubs.ContainsKey(user.Item1.ID))
            {
                this.resubs[user.Item1.ID] = (uint)user.Item2;
            }
        }

        private void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            if (!this.newSubs.Contains(e.Item2.ID))
            {
                this.newSubs.Add(e.Item2.ID);
            }

            if (!this.giftedSubs.ContainsKey(e.Item1.ID))
            {
                this.giftedSubs[e.Item1.ID] = 0;
            }
            this.giftedSubs[e.Item1.ID]++;
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            if (!this.donations.ContainsKey(donation.User.ID))
            {
                this.donations[donation.User.ID] = 0;
            }
            this.donations[donation.User.ID] += donation.Amount;
        }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> sparkUsage)
        {
            if (!this.sparks.ContainsKey(sparkUsage.Item1.ID))
            {
                this.sparks[sparkUsage.Item1.ID] = 0;
            }
            this.sparks[sparkUsage.Item1.ID] += sparkUsage.Item2;
        }

        private void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage)
        {
            if (!this.embers.ContainsKey(emberUsage.User.ID))
            {
                this.embers[emberUsage.User.ID] = 0;
            }
            this.embers[emberUsage.User.ID] += emberUsage.Amount;
        }

        private Dictionary<UserViewModel, string> GetUsersDictionary(HashSet<uint> data)
        {
            Dictionary<UserViewModel, string> results = new Dictionary<UserViewModel, string>();
            foreach (uint userID in data)
            {
                UserViewModel user = this.GetUser(userID);
                results[user] = string.Empty;
            }
            return results;
        }

        private Dictionary<UserViewModel, string> GetUsersDictionary(Dictionary<uint, uint> data)
        {
            Dictionary<UserViewModel, string> results = new Dictionary<UserViewModel, string>();
            foreach (var kvp in data)
            {
                UserViewModel user = this.GetUser(kvp.Key);
                results[user] = kvp.Value.ToString();
            }
            return results;
        }

        private Dictionary<UserViewModel, string> GetUsersDictionary(Dictionary<uint, double> data)
        {
            Dictionary<UserViewModel, string> results = new Dictionary<UserViewModel, string>();
            foreach (var kvp in data)
            {
                UserViewModel user = this.GetUser(kvp.Key);
                results[user] = string.Format("{0:C}", Math.Round(kvp.Value, 2));
            }
            return results;
        }

        private UserViewModel GetUser(uint userID)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByID(userID);
            if (user == null)
            {
                user = new UserViewModel(userID, string.Empty);
                user = new UserViewModel(userID, user.Data.UserName);
            }
            return user;
        }

        private async Task PerformSectionTemplateReplacement(StringBuilder htmlBuilder, OverlayEndCreditsSectionTypeEnum itemType, Dictionary<UserViewModel, string> replacers)
        {
            if (this.SectionTemplates.ContainsKey(itemType) && replacers.Count > 0)
            {
                OverlayEndCreditsSectionModel sectionTemplate = this.SectionTemplates[itemType];

                string sectionHTML = this.PerformTemplateReplacements(sectionTemplate.SectionHTML, new Dictionary<string, string>()
                {
                    { "NAME", EnumHelper.GetEnumName(itemType) },
                    { "TEXT_FONT", this.SectionTextFont },
                    { "TEXT_SIZE", this.SectionTextSize.ToString() },
                    { "TEXT_COLOR", this.SectionTextColor }
                });
                sectionHTML = await this.ReplaceStringWithSpecialModifiers(sectionHTML, await ChannelSession.GetCurrentUser(), new List<string>(), new Dictionary<string, string>());

                List<string> userHTMLs = new List<string>();
                foreach (var kvp in replacers)
                {
                    if (!string.IsNullOrEmpty(kvp.Key.UserName))
                    {
                        string userHTML = this.PerformTemplateReplacements(sectionTemplate.UserHTML, new Dictionary<string, string>()
                        {
                            { "NAME", kvp.Key.UserName },
                            { "DETAILS", kvp.Value },
                            { "TEXT_FONT", this.ItemTextFont },
                            { "TEXT_SIZE", this.ItemTextSize.ToString() },
                            { "TEXT_COLOR", this.ItemTextColor }
                        });
                        userHTML = await this.ReplaceStringWithSpecialModifiers(userHTML, kvp.Key, new List<string>(), new Dictionary<string, string>());
                        userHTMLs.Add(userHTML);
                    }
                }

                htmlBuilder.AppendLine(SectionSeparatorHTML);
                htmlBuilder.AppendLine(sectionHTML);
                foreach (string userHTML in userHTMLs)
                {
                    htmlBuilder.AppendLine(userHTML);
                }
            }
        }
    }
}
