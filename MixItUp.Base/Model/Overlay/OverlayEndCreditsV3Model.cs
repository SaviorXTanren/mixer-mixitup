using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayEndCreditsSectionV3Type
    {
        Chatters = 0,
        Followers,
        Subscribers,
        Moderators,

        Raids = 10,

        NewFollowers = 20,
        NewSubscribers,
        Resubscribers,
        GiftedSubscriptions,
        AllSubscriptions,

        TwitchBits = 40,

        YouTubeSuperChats = 50,

        TrovoSpells = 60,

        Donations = 200,

        HTML = 900,
        CustomSection = 999,
    }

    [DataContract]
    public class OverlayEndCreditsHeaderV3Model : OverlayHeaderV3ModelBase
    {
        public OverlayEndCreditsHeaderV3Model() { }
    }

    [DataContract]
    public class OverlayEndCreditsSectionV3Model
    {
        public const string SectionIDPropertyName = "SectionID";
        public const string UsernamePropertyName = "Username";
        public const string AmountPropertyName = "Amount";
        public const string TextPropertyName = "Text";

        public static readonly string DefaultHTML = OverlayResources.OverlayEndCreditsSectionDefaultHTML;

        public static readonly string UsernameItemTemplate = $"{{{UsernamePropertyName}}}";
        public static readonly string UsernameAmountItemTemplate = $"{{{UsernamePropertyName}}} - {{{AmountPropertyName}}}";
        public static readonly string TextItemTemplate = $"{{{TextPropertyName}}}";

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public OverlayEndCreditsSectionV3Type Type { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ItemTemplate { get; set; }

        [DataMember]
        public int Columns { get; set; }

        [DataMember]
        public string HTML { get; set; } = string.Empty;

        [JsonIgnore]
        public double ColumnWidthPercentage
        {
            get
            {
                return Math.Round(100.0 / ((double)this.Columns), 4);
            }
        }

        [JsonIgnore]
        public Dictionary<UserV2ViewModel, double> itemTracking = new Dictionary<UserV2ViewModel, double>();

        [JsonIgnore]
        public List<Tuple<UserV2ViewModel, string>> customTracking = new List<Tuple<UserV2ViewModel, string>>();

        public void Track(UserV2ViewModel user) { this.Track(user, 0); }

        public void Track(UserV2ViewModel user, double amount)
        {
            if (this.ShouldTrack(user))
            {
                if (!this.itemTracking.ContainsKey(user))
                {
                    this.itemTracking[user] = 0;
                }
                this.itemTracking[user] += amount;

                Logger.Log(LogLevel.Debug, $"Tracking added for {user} to {this.itemTracking[user]} for {this.Name} End Credits section");
            }
            else
            {
                Logger.Log(LogLevel.Debug, $"No tracking {user} for {this.Name} End Credits section");
            }
        }

        public void Track(UserV2ViewModel user, string text)
        {
            if (this.ShouldTrack(user))
            {
                this.customTracking.Add(new Tuple<UserV2ViewModel, string>(user, text));
                Logger.Log(LogLevel.Debug, $"Tracking added for {user} to {text} for {this.Name} End Credits section");
            }
            else
            {
                Logger.Log(LogLevel.Debug, $"No tracking {user} for {this.Name} End Credits section");
            }
        }

        public void Untrack(UserV2ViewModel user)
        {
            this.itemTracking.Remove(user);

            foreach (var item in this.customTracking.ToList())
            {
                if (item.Item1.Equals(user))
                {
                    this.customTracking.Remove(item);
                }
            }
        }

        public void ClearTracking()
        {
            this.itemTracking.Clear();
            this.customTracking.Clear();
        }

        public Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties[nameof(this.Name)] = this.Name;
            properties[OverlayEndCreditsSectionV3Model.SectionIDPropertyName] = this.ID.ToString();
            properties[nameof(this.ColumnWidthPercentage)] = this.ColumnWidthPercentage.ToString();

            return properties;
        }

        public async Task<IEnumerable<string>> GetItems()
        {
            List<string> items = new List<string>();

            if (this.Type == OverlayEndCreditsSectionV3Type.HTML)
            {
                items.Add(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.ItemTemplate, new CommandParametersModel()));
                return items;
            }
            else if (this.Type == OverlayEndCreditsSectionV3Type.CustomSection)
            {
                foreach (var item in this.customTracking.ToList())
                {
                    string text = OverlayV3Service.ReplaceProperty(this.ItemTemplate, TextPropertyName, item.Item2);
                    if (SpecialIdentifierStringBuilder.ContainsSpecialIdentifiers(text))
                    {
                        await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(text, new CommandParametersModel(item.Item1));
                    }
                    items.Add(text);
                }
                return items;
            }

            foreach (var item in this.itemTracking.ToList())
            {
                string text = OverlayV3Service.ReplaceProperty(this.ItemTemplate, UsernamePropertyName, item.Key.DisplayName);
                switch (this.Type)
                {
                    case OverlayEndCreditsSectionV3Type.Raids:
                    case OverlayEndCreditsSectionV3Type.Resubscribers:
                    case OverlayEndCreditsSectionV3Type.GiftedSubscriptions:
                    case OverlayEndCreditsSectionV3Type.TwitchBits:
                    case OverlayEndCreditsSectionV3Type.TrovoSpells:
                    case OverlayEndCreditsSectionV3Type.YouTubeSuperChats:
                    case OverlayEndCreditsSectionV3Type.Donations:
                        text = OverlayV3Service.ReplaceProperty(text, AmountPropertyName, item.Value.ToNumberDisplayString());
                        break;
                }

                if (SpecialIdentifierStringBuilder.ContainsSpecialIdentifiers(text))
                {
                    text = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(text, new CommandParametersModel(item.Key));
                }

                items.Add(text);
            }
            return items.OrderBy(i => i);
        }

        private bool ShouldTrack(UserV2ViewModel user)
        {
            return !user.IsSpecialtyExcluded;
        }
    }

    [DataContract]
    public class OverlayEndCreditsV3Model : OverlayEventTrackingV3ModelBase
    {
        public const string EndCreditsStartedPacketType = "EndCreditsStarted";
        public const string EndCreditsCompletedPacketType = "EndCreditsCompleted";

        private static readonly HashSet<OverlayEndCreditsSectionV3Type> AllChatterSectionTypes = new HashSet<OverlayEndCreditsSectionV3Type>()
        {
            OverlayEndCreditsSectionV3Type.Chatters,
            OverlayEndCreditsSectionV3Type.Followers,
            OverlayEndCreditsSectionV3Type.Subscribers,
            OverlayEndCreditsSectionV3Type.Moderators
        };

        private static readonly HashSet<OverlayEndCreditsSectionV3Type> AllSubscriberSectionTypes = new HashSet<OverlayEndCreditsSectionV3Type>()
        {
            OverlayEndCreditsSectionV3Type.Subscribers,
            OverlayEndCreditsSectionV3Type.NewSubscribers,
            OverlayEndCreditsSectionV3Type.GiftedSubscriptions,
            OverlayEndCreditsSectionV3Type.AllSubscriptions
        };

        public static readonly string DefaultHTML = OverlayResources.OverlayEndCreditsDefaultHTML;
        public static readonly string DefaultCSS =  OverlayResources.OverlayEndCreditsDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayEndCreditsDefaultJavascript;

        [DataMember]
        public OverlayEndCreditsHeaderV3Model Header { get; set; }

        [DataMember]
        public int ScrollSpeed { get; set; }
        [DataMember]
        public double ScrollRate { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public bool RunCreditsWhenVisible { get; set; }
        [DataMember]
        public bool RunEndlessly { get; set; }
        [DataMember]
        public bool DontShowNoDataError { get; set; }

        [DataMember]
        public List<OverlayEndCreditsSectionV3Model> Sections { get; set; } = new List<OverlayEndCreditsSectionV3Model>();

        [DataMember]
        public Guid StartedCommandID { get; set; }
        [DataMember]
        public Guid EndedCommandID { get; set; }

        [DataMember]
        public override bool Chatters { get { return this.Sections.Any(s => OverlayEndCreditsV3Model.AllChatterSectionTypes.Contains(s.Type)); } set { } }

        [DataMember]
        public override bool Follows { get { return this.Sections.Any(s => s.Type == OverlayEndCreditsSectionV3Type.Followers || s.Type == OverlayEndCreditsSectionV3Type.NewFollowers); } set { } }

        [DataMember]
        public override bool Raids { get { return this.Sections.Any(s => s.Type == OverlayEndCreditsSectionV3Type.Raids); } set { } }

        [DataMember]
        public override bool TwitchSubscriptions { get { return this.Sections.Any(s => OverlayEndCreditsV3Model.AllSubscriberSectionTypes.Contains(s.Type)); } set { } }
        [DataMember]
        public override bool TwitchBits { get { return this.Sections.Any(s => s.Type == OverlayEndCreditsSectionV3Type.TwitchBits); } set { } }

        [DataMember]
        public override bool YouTubeMemberships { get { return this.Sections.Any(s => OverlayEndCreditsV3Model.AllSubscriberSectionTypes.Contains(s.Type)); } set { } }
        [DataMember]
        public override bool YouTubeSuperChats { get { return this.Sections.Any(s => s.Type == OverlayEndCreditsSectionV3Type.YouTubeSuperChats); } set { } }

        [DataMember]
        public override bool TrovoSubscriptions { get { return this.Sections.Any(s => OverlayEndCreditsV3Model.AllSubscriberSectionTypes.Contains(s.Type)); } set { } }
        [DataMember]
        public override bool TrovoElixirSpells { get { return this.Sections.Any(s => s.Type == OverlayEndCreditsSectionV3Type.TrovoSpells); } set { } }

        [DataMember]
        public override bool Donations { get { return this.Sections.Any(s => s.Type == OverlayEndCreditsSectionV3Type.Donations); } set { } }

        [JsonIgnore]
        public string AnimationIterations { get { return this.RunEndlessly ? "Infinity" : "1"; } }

        [JsonIgnore]
        public override bool JQuery { get { return true; } }

        public OverlayEndCreditsV3Model() : base(OverlayItemV3Type.EndCredits) { }

        public override async Task Initialize()
        {
            if (string.Equals(this.Javascript, OverlayResources.OverlayEndCreditsDefaultJavascriptOld, System.StringComparison.OrdinalIgnoreCase))
            {
                this.Javascript = OverlayResources.OverlayEndCreditsDefaultJavascript;
            }

            if (this.PositionType != OverlayPositionV3Type.Pixel || this.XPosition != 0 || this.YPosition != 0)
            {
                this.PositionType = OverlayPositionV3Type.Pixel;
                this.XPosition = 0;
                this.YPosition = 0;
            }

            await base.Initialize();

            foreach (OverlayEndCreditsSectionV3Model section in Sections)
            {
                section.ClearTracking();
            }
        }

        public override async Task Reset()
        {
            await base.Reset();

            foreach (OverlayEndCreditsSectionV3Model section in Sections)
            {
                section.ClearTracking();
            }
        }

        public override void OnChatUserBanned(object sender, UserV2ViewModel user)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                section.Untrack(user);
            }
        }

        public override void OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                if (section.Type == OverlayEndCreditsSectionV3Type.Chatters)
                {
                    section.Track(message.User);
                }

                if (section.Type == OverlayEndCreditsSectionV3Type.Followers)
                {
                    if (message.User.IsFollower)
                    {
                        section.Track(message.User);
                    }
                }

                if (section.Type == OverlayEndCreditsSectionV3Type.Subscribers)
                {
                    if (message.User.IsSubscriber)
                    {
                        section.Track(message.User);
                    }
                }

                if (section.Type == OverlayEndCreditsSectionV3Type.Moderators)
                {
                    if (message.User.HasRole(UserRoleEnum.Moderator))
                    {
                        section.Track(message.User);
                    }
                }
            }
        }

        public override void OnFollow(object sender, UserV2ViewModel user)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                switch (section.Type)
                {
                    case OverlayEndCreditsSectionV3Type.NewFollowers:
                        section.Track(user);
                        break;
                }
            }
        }

        public override void OnRaid(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                switch (section.Type)
                {
                    case OverlayEndCreditsSectionV3Type.Raids:
                        section.Track(raid.Item1, raid.Item2);
                        break;
                }
            }
        }

        public override void OnSubscribe(object sender, SubscriptionDetailsModel subscription)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                switch (section.Type)
                {
                    case OverlayEndCreditsSectionV3Type.NewSubscribers:
                        if (subscription.Months == 1)
                        {
                            section.Track(subscription.User);
                        }
                        break;
                    case OverlayEndCreditsSectionV3Type.Resubscribers:
                        if (subscription.Months > 1)
                        {
                            section.Track(subscription.User, subscription.Months);
                        }
                        break;
                    case OverlayEndCreditsSectionV3Type.GiftedSubscriptions:
                        if (subscription.Gifter != null)
                        {
                            section.Track(subscription.Gifter, 1);
                        }
                        break;
                    case OverlayEndCreditsSectionV3Type.AllSubscriptions:
                        section.Track(subscription.User);
                        break;
                }
            }
        }

        public override void OnMassSubscription(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                switch (section.Type)
                {
                    case OverlayEndCreditsSectionV3Type.NewSubscribers:
                        foreach (var subscription in subscriptions)
                        {
                            section.Track(subscription.User);
                        }
                        break;
                    case OverlayEndCreditsSectionV3Type.GiftedSubscriptions:
                        foreach (var subscription in subscriptions)
                        {
                            if (subscription.Gifter != null)
                            {
                                section.Track(subscription.Gifter, 1);
                            }
                        }
                        break;
                    case OverlayEndCreditsSectionV3Type.AllSubscriptions:
                        foreach (var subscription in subscriptions)
                        {
                            section.Track(subscription.User);
                        }
                        break;
                }
            }
        }

        public override void OnDonation(object sender, UserDonationModel donation)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                switch (section.Type)
                {
                    case OverlayEndCreditsSectionV3Type.Donations:
                        section.Track(donation.User, donation.Amount);
                        break;
                }
            }
        }

        public override void OnTwitchBits(object sender, TwitchBitsCheeredEventModel bitsCheered)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                switch (section.Type)
                {
                    case OverlayEndCreditsSectionV3Type.TwitchBits:
                        section.Track(bitsCheered.User, bitsCheered.Amount);
                        break;
                }
            }
        }

        public override void OnYouTubeSuperChat(object sender, YouTubeSuperChatViewModel superChat)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                switch (section.Type)
                {
                    case OverlayEndCreditsSectionV3Type.YouTubeSuperChats:
                        section.Track(superChat.User, superChat.Amount);
                        break;
                }
            }
        }

        public override void OnTrovoSpell(object sender, TrovoChatSpellViewModel spell)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                switch (section.Type)
                {
                    case OverlayEndCreditsSectionV3Type.TrovoSpells:
                        section.Track(spell.User, spell.ValueTotal);
                        break;
                }
            }
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            foreach (var kvp in this.Header.GetGenerationProperties())
            {
                properties[kvp.Key] = kvp.Value;
            }

            properties[nameof(this.ScrollSpeed)] = this.ScrollSpeed.ToString();
            properties[nameof(this.ScrollRate)] = this.ScrollRate.ToString();

            properties[nameof(this.RunEndlessly)] = this.RunEndlessly.ToString().ToLower();
            properties[nameof(this.AnimationIterations)] = this.AnimationIterations;

            properties[nameof(this.BackgroundColor)] = this.BackgroundColor;
            properties[nameof(this.RunCreditsWhenVisible)] = this.RunCreditsWhenVisible.ToString().ToLower();

            List<string> sectionsHTML = new List<string>();
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                string sectionHTML = section.HTML;
                foreach (var kvp in section.GetGenerationProperties())
                {
                    sectionHTML = OverlayV3Service.ReplaceProperty(sectionHTML, kvp.Key, kvp.Value);
                }
                sectionsHTML.Add(sectionHTML);
            }

            properties[nameof(this.Sections)] = string.Join(Environment.NewLine + Environment.NewLine, sectionsHTML);

            //properties[nameof(this.ProgressOccurredAnimation)] = this.ProgressOccurredAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement);
            //properties[nameof(this.SegmentCompletedAnimation)] = this.SegmentCompletedAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement);

            return properties;
        }

        public override async Task ProcessPacket(OverlayV3Packet packet)
        {
            await base.ProcessPacket(packet);

            if (string.Equals(packet.Type, OverlayEndCreditsV3Model.EndCreditsStartedPacketType))
            {
                await ServiceManager.Get<CommandService>().Queue(this.StartedCommandID);
            }
            else if (string.Equals(packet.Type, EndCreditsCompletedPacketType))
            {
                await ServiceManager.Get<CommandService>().Queue(this.EndedCommandID);
            }
        }

        public void CustomTrack(Guid sectionID, UserV2ViewModel user, string text)
        {
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                if (section.ID.Equals(sectionID) && section.Type == OverlayEndCreditsSectionV3Type.CustomSection)
                {
                    section.Track(user, text);
                    return;
                }
            }
        }

        public async Task PlayCredits()
        {
            if (this.IsLivePreview)
            {
                await OverlayEndCreditsV3ViewModel.LoadTestData(this);
            }

            List<OverlayEndCreditsSectionV3Model> applicableSections = new List<OverlayEndCreditsSectionV3Model>();

            Dictionary<Guid, IEnumerable<string>> sectionItems = new Dictionary<Guid, IEnumerable<string>>();
            foreach (OverlayEndCreditsSectionV3Model section in this.Sections)
            {
                IEnumerable<string> items = await section.GetItems();
                if (items.Count() > 0)
                {
                    sectionItems[section.ID] = items;
                    applicableSections.Add(section);
                }
            }

            if (applicableSections.Count > 0)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data["Order"] = applicableSections.Select(s => s.ID);
                data["Columns"] = applicableSections.ToDictionary(s => s.ID, s => s.Columns);
                data["Types"] = applicableSections.ToDictionary(s => s.ID, s => s.Type.ToString());
                data["Items"] = sectionItems;

                await this.CallFunction("startCredits", data);
            }
            else
            {
                if (!this.DontShowNoDataError)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(Resources.OverlayWidgetEndCreditsNoDataCurrentlyAvailable);
                }
            }
        }

        protected override async Task Loaded()
        {
            if (this.RunCreditsWhenVisible)
            {
                await this.PlayCredits();
            }
        }
    }
}
