using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public enum OverlayEndCreditsSectionTypeEnum
    {
        Chatters,
        [Name("NewFollowers")]
        Followers,
        Hosts,
        NewSubscribers,
        Resubscribers,
        GiftedSubs,
        Donations,
        [Obsolete]
        Sparks,
        [Obsolete]
        Embers,
        Subscribers,
        Moderators,
        FreeFormHTML,
        FreeFormHTML2,
        FreeFormHTML3,
        Bits,
        Raids,
    }

    [Obsolete]
    public enum OverlayEndCreditsSpeedEnum
    {
        Fast = 10,
        Medium = 20,
        Slow = 30
    }

    [Obsolete]
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

    [Obsolete]
    [DataContract]
    public class OverlayEndCreditsItemModel : OverlayHTMLTemplateItemModelBase
    {
        public const string CreditsWrapperHTML = @"<div id=""titles"" style=""background-color: {0}""><div id=""credits"">{1}</div></div>";

        public const string TitleHTMLTemplate = @"<div id=""the-end"">Stream Credits</div>";

        public const string SectionHTMLTemplate = @"<h1 style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">{NAME}</h1>";
        public const string FreeFormSectionHTMLTemplate = @"<h1 style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">FREE FORM</h1>";

        public const string UserHTMLTemplate = @"<p style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">{NAME}</p>";
        public const string UserDetailsHTMLTemplate = @"<dt style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">{NAME}</dt><dd style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">{DETAILS}</dd>";
        public const string FreeFormUserHTMLTemplate = @"<p style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR};"">FREE FORM</p>";

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

        [JsonIgnore]
        private HashSet<Guid> viewers = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> subs = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> mods = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> follows = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> hosts = new HashSet<Guid>();
        [JsonIgnore]
        private Dictionary<Guid, uint> raids = new Dictionary<Guid, uint>();
        [JsonIgnore]
        private HashSet<Guid> newSubs = new HashSet<Guid>();
        [JsonIgnore]
        private Dictionary<Guid, uint> resubs = new Dictionary<Guid, uint>();
        [JsonIgnore]
        private Dictionary<Guid, uint> giftedSubs = new Dictionary<Guid, uint>();
        [JsonIgnore]
        private Dictionary<Guid, double> donations = new Dictionary<Guid, double>();
        [JsonIgnore]
        private Dictionary<Guid, uint> bits = new Dictionary<Guid, uint>();

        [JsonIgnore]
        private HashSet<OverlayEndCreditsSectionTypeEnum> testDataFilled = new HashSet<OverlayEndCreditsSectionTypeEnum>();

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

        public override Task LoadTestData()
        {
            this.testDataFilled.Clear();
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Chatters) && this.viewers.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Chatters);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Subscribers) && this.subs.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Subscribers);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Moderators) && this.mods.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Moderators);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Followers) && this.follows.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Followers);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Hosts) && this.hosts.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Hosts);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Raids) && this.raids.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Raids);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.NewSubscribers) && this.newSubs.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.NewSubscribers);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Resubscribers) && this.resubs.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Resubscribers);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.GiftedSubs) && this.giftedSubs.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.GiftedSubs);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Donations) && this.donations.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Donations);
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Bits) && this.bits.Count == 0)
            {
                this.testDataFilled.Add(OverlayEndCreditsSectionTypeEnum.Bits);
            }

            UserV2ViewModel user = ChannelSession.User;
            List<Guid> userIDs = new List<Guid>(ChannelSession.Settings.Users.Keys.Take(20));
            for (int i = userIDs.Count; i < 20; i++)
            {
                userIDs.Add(user.ID);
            }

            foreach (Guid userID in userIDs)
            {
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Chatters))
                {
                    this.viewers.Add(userID);
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Subscribers))
                {
                    this.subs.Add(userID);
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Moderators))
                {
                    this.mods.Add(userID);
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Followers))
                {
                    this.follows.Add(userID);
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Hosts))
                {
                    this.hosts.Add(userID);
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Raids))
                {
                    this.raids[userID] = 10;
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.NewSubscribers))
                {
                    this.newSubs.Add(userID);
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Resubscribers))
                {
                    this.resubs[userID] = 5;
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.GiftedSubs))
                {
                    this.giftedSubs[userID] = 5;
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Donations))
                {
                    this.donations[userID] = 12.34;
                }
                if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Bits))
                {
                    this.bits[userID] = 123;
                }
            }
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Chatters) || this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Subscribers) ||
                this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Moderators))
            {
                ChatService.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Followers))
            {
                EventService.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Hosts))
            {

            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Raids))
            {
                //EventService.OnRaidOccurred += GlobalEvents_OnRaidOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.NewSubscribers))
            {
                //EventService.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Resubscribers))
            {
                //EventService.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.GiftedSubs))
            {
                //EventService.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Donations))
            {
                EventService.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.SectionTemplates.ContainsKey(OverlayEndCreditsSectionTypeEnum.Bits))
            {
                EventService.OnTwitchBitsCheeredOccurred += GlobalEvents_OnBitsOccurred;
            }
            return base.Initialize();
        }

        public override Task Disable()
        {
            ChatService.OnChatMessageReceived -= GlobalEvents_OnChatMessageReceived;
            EventService.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            EventService.OnRaidOccurred -= GlobalEvents_OnRaidOccurred;
            //EventService.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            //EventService.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            //EventService.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            EventService.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= GlobalEvents_OnBitsOccurred;
            return Task.CompletedTask;
        }

        public override async Task Reset()
        {
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Chatters))
            {
                this.viewers.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Subscribers))
            {
                this.subs.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Moderators))
            {
                this.mods.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Followers))
            {
                this.follows.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Hosts))
            {
                this.hosts.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Raids))
            {
                this.raids.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.NewSubscribers))
            {
                this.newSubs.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Resubscribers))
            {
                this.resubs.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.GiftedSubs))
            {
                this.giftedSubs.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Donations))
            {
                this.donations.Clear();
            }
            if (this.testDataFilled.Contains(OverlayEndCreditsSectionTypeEnum.Bits))
            {
                this.bits.Clear();
            }
            await base.Reset();
        }

        protected override async Task PerformReplacements(JObject jobj, CommandParametersModel parameters)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine(SectionSeparatorHTML);
            htmlBuilder.AppendLine(await ReplaceStringWithSpecialModifiers(this.TitleTemplate, parameters));

            foreach (var kvp in this.SectionTemplates)
            {
                if (kvp.Key == OverlayEndCreditsSectionTypeEnum.FreeFormHTML || kvp.Key == OverlayEndCreditsSectionTypeEnum.FreeFormHTML2 ||
                    kvp.Key == OverlayEndCreditsSectionTypeEnum.FreeFormHTML3)
                {
                    OverlayEndCreditsSectionModel sectionTemplate = this.SectionTemplates[kvp.Key];

                    string sectionHTML = this.PerformTemplateReplacements(sectionTemplate.SectionHTML, new Dictionary<string, string>());
                    sectionHTML = await ReplaceStringWithSpecialModifiers(sectionHTML, parameters);

                    string userHTML = this.PerformTemplateReplacements(sectionTemplate.UserHTML, new Dictionary<string, string>());
                    userHTML = await ReplaceStringWithSpecialModifiers(userHTML, parameters);

                    htmlBuilder.AppendLine(SectionSeparatorHTML);
                    htmlBuilder.AppendLine(sectionHTML);
                    htmlBuilder.AppendLine(userHTML);
                }
                else
                {
                    Dictionary<UserV2ViewModel, string> items = new Dictionary<UserV2ViewModel, string>();
                    switch (kvp.Key)
                    {
                        case OverlayEndCreditsSectionTypeEnum.Chatters: items = await this.GetUsersDictionary(this.viewers); break;
                        case OverlayEndCreditsSectionTypeEnum.Subscribers: items = await this.GetUsersDictionary(this.subs); break;
                        case OverlayEndCreditsSectionTypeEnum.Moderators: items = await this.GetUsersDictionary(this.mods); break;
                        case OverlayEndCreditsSectionTypeEnum.Followers: items = await this.GetUsersDictionary(this.follows); break;
                        case OverlayEndCreditsSectionTypeEnum.Hosts: items = await this.GetUsersDictionary(this.hosts); break;
                        case OverlayEndCreditsSectionTypeEnum.Raids: items = await this.GetUsersDictionary(this.raids); break;
                        case OverlayEndCreditsSectionTypeEnum.NewSubscribers: items = await this.GetUsersDictionary(this.newSubs); break;
                        case OverlayEndCreditsSectionTypeEnum.Resubscribers: items = await this.GetUsersDictionary(this.resubs); break;
                        case OverlayEndCreditsSectionTypeEnum.GiftedSubs: items = await this.GetUsersDictionary(this.giftedSubs); break;
                        case OverlayEndCreditsSectionTypeEnum.Donations: items = await this.GetUsersDictionary(this.donations); break;
                        case OverlayEndCreditsSectionTypeEnum.Bits: items = await this.GetUsersDictionary(this.bits); break;
                    }
                    await this.PerformSectionTemplateReplacement(htmlBuilder, kvp.Key, items, parameters);
                }
            }

            jobj["HTML"] = string.Format(CreditsWrapperHTML, this.BackgroundColor, htmlBuilder.ToString());
        }

        private void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (message.User != null && !message.User.IsSpecialtyExcluded)
            {
                this.AddUserForRole(message.User);
            }
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                this.AddUserForRole(user);
            }
        }

        private void GlobalEvents_OnHostOccurred(object sender, UserV2ViewModel host)
        {
            if (!this.hosts.Contains(host.ID))
            {
                this.hosts.Add(host.ID);
                this.AddUserForRole(host);
            }
        }

        private void GlobalEvents_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            if (!this.raids.ContainsKey(raid.Item1.ID))
            {
                this.raids[raid.Item1.ID] = (uint)raid.Item2;
                this.AddUserForRole(raid.Item1);
            }
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.newSubs.Contains(user.ID))
            {
                this.newSubs.Add(user.ID);
                this.AddUserForRole(user);
            }
        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> resub)
        {
            if (!this.resubs.ContainsKey(resub.Item1.ID))
            {
                this.resubs[resub.Item1.ID] = (uint)resub.Item2;
                this.AddUserForRole(resub.Item1);
            }
        }

        private void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> subGift)
        {
            if (!this.newSubs.Contains(subGift.Item2.ID))
            {
                this.newSubs.Add(subGift.Item2.ID);
                this.AddUserForRole(subGift.Item2);
            }

            if (!this.giftedSubs.ContainsKey(subGift.Item1.ID))
            {
                this.giftedSubs[subGift.Item1.ID] = 0;
                this.AddUserForRole(subGift.Item1);
            }
            this.giftedSubs[subGift.Item1.ID]++;
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            if (!this.donations.ContainsKey(donation.User.ID))
            {
                this.donations[donation.User.ID] = 0;
                this.AddUserForRole(donation.User);
            }
            this.donations[donation.User.ID] += donation.Amount;
        }

        private void GlobalEvents_OnBitsOccurred(object sender, TwitchBitsCheeredEventModel bits)
        {
            if (!this.bits.ContainsKey(bits.User.ID))
            {
                this.bits[bits.User.ID] = 0;
                this.AddUserForRole(bits.User);
            }
            this.bits[bits.User.ID] += (uint)bits.Amount;
        }

        private void AddUserForRole(UserV2ViewModel user)
        {
            if (this.ShouldIncludeUser(user))
            {
                this.viewers.Add(user.ID);
                if (user.MeetsRole(UserRoleEnum.Subscriber))
                {
                    this.subs.Add(user.ID);
                }
                if (user.MeetsRole(UserRoleEnum.Moderator) || user.MeetsRole(UserRoleEnum.TwitchChannelEditor))
                {
                    this.mods.Add(user.ID);
                }
            }
        }

        private bool ShouldIncludeUser(UserV2ViewModel user)
        {
            if (user == null)
            {
                return false;
            }

            if (user.ID.Equals(ChannelSession.User.ID))
            {
                return false;
            }

            if (user.IsSpecialtyExcluded)
            {
                return false;
            }

            return true;
        }

        private Task<Dictionary<UserV2ViewModel, string>> GetUsersDictionary(HashSet<Guid> data)
        {
            Dictionary<UserV2ViewModel, string> results = new Dictionary<UserV2ViewModel, string>();
            foreach (Guid userID in data)
            {
                try
                {
                    //UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByID(userID);
                    //if (user != null)
                    //{
                    //    results[user] = string.Empty;
                    //}
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
            return Task.FromResult(results);
        }

        private Task<Dictionary<UserV2ViewModel, string>> GetUsersDictionary(Dictionary<Guid, uint> data)
        {
            Dictionary<UserV2ViewModel, string> results = new Dictionary<UserV2ViewModel, string>();
            foreach (var kvp in data)
            {
                try
                {
                    //UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByID(kvp.Key);
                    //if (user != null)
                    //{
                    //    results[user] = kvp.Value.ToString();
                    //}
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
            return Task.FromResult(results);
        }

        private Task<Dictionary<UserV2ViewModel, string>> GetUsersDictionary(Dictionary<Guid, double> data)
        {
            Dictionary<UserV2ViewModel, string> results = new Dictionary<UserV2ViewModel, string>();
            foreach (var kvp in data)
            {
                try
                {
                    //UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByID(kvp.Key);
                    //if (user != null)
                    //{
                    //    results[user] = CurrencyHelper.ToCurrencyString(kvp.Value);
                    //}
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
            return Task.FromResult(results);
        }

        private async Task PerformSectionTemplateReplacement(StringBuilder htmlBuilder, OverlayEndCreditsSectionTypeEnum itemType, Dictionary<UserV2ViewModel, string> replacers, CommandParametersModel parameters)
        {
            if (this.SectionTemplates.ContainsKey(itemType) && replacers.Count > 0)
            {
                OverlayEndCreditsSectionModel sectionTemplate = this.SectionTemplates[itemType];

                string sectionHTML = this.PerformTemplateReplacements(sectionTemplate.SectionHTML, new Dictionary<string, string>()
                {
                    { "NAME", EnumLocalizationHelper.GetLocalizedName(itemType) },
                    { "TEXT_FONT", this.SectionTextFont },
                    { "TEXT_SIZE", this.SectionTextSize.ToString() },
                    { "TEXT_COLOR", this.SectionTextColor }
                });
                sectionHTML = await ReplaceStringWithSpecialModifiers(sectionHTML, parameters);

                List<string> userHTMLs = new List<string>();
                foreach (var kvp in replacers.OrderBy(kvp => kvp.Key.DisplayName))
                {
                    if (!string.IsNullOrEmpty(kvp.Key.DisplayName))
                    {
                        string userHTML = this.PerformTemplateReplacements(sectionTemplate.UserHTML, new Dictionary<string, string>()
                        {
                            { "NAME", kvp.Key.DisplayName },
                            { "DETAILS", kvp.Value },
                            { "TEXT_FONT", this.ItemTextFont },
                            { "TEXT_SIZE", this.ItemTextSize.ToString() },
                            { "TEXT_COLOR", this.ItemTextColor }
                        });
                        userHTML = await ReplaceStringWithSpecialModifiers(userHTML, parameters);
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
