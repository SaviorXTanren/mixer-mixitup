using Mixer.Base.Model.Clips;
using Mixer.Base.Model.Leaderboards;
using Mixer.Base.Model.Patronage;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    [DataContract]
    public class OverlayChatMessage
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [Obsolete]
    [DataContract]
    public class OverlayChatMessages : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
        @"<div style=""border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px;"">
          <p style=""padding: 10px; margin: auto;"">
            <img src=""{USER_IMAGE}"" width=""{TEXT_SIZE}"" height=""{TEXT_SIZE}"" style=""vertical-align: middle; padding-right: 2px"">
            <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; word-wrap: break-word; color: {USER_COLOR}; vertical-align: middle;"">{USERNAME}</span>
            <img src=""{SUB_IMAGE}"" style=""vertical-align: middle; padding-right: 5px"" onerror=""this.style.display='none'"">
            {MESSAGE}
          </p>
        </div>";

        public const string ChatMessagesItemType = "chatmessages";

        private const string TextMessageHTMLTemplate = @"<span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; word-wrap: break-word; color: {TEXT_COLOR}; vertical-align: middle; margin-left: 10px;"">{TEXT}</span>";
        private const string EmoticonMessageHTMLTemplate = @"<span role=""img"" style=""height: {EMOTICON_SIZE}px; width: {EMOTICON_SIZE}px; background-repeat: no-repeat; display: inline-block; background-image: url({EMOTICON}); background-position: {EMOTICON_X}px {EMOTICON_Y}px;""></span>";
        private const string SkillImageMessageHTMLTemplate = @"<img src=""{IMAGE}"" style=""vertical-align: middle; margin-left: 10px; max-height: 80px;""></img>";

        private static readonly Dictionary<string, string> userColors = new Dictionary<string, string>()
        {
            { "UserStreamerRoleColor", "#FFFFFF" },
            { "UserStaffRoleColor", "#FFD700" },
            { "UserModRoleColor", "#008000" },
            { "UserGlobalModRoleColor", "#07FDC6" },
            { "UserProRoleColor", "#800080" },
            { "UserDefaultRoleColor", "#0000FF" },
        };

        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public int TextSize { get; set; }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }

        [DataMember]
        public List<OverlayChatMessage> Messages = new List<OverlayChatMessage>();

        [DataMember]
        public List<Guid> DeletedMessages = new List<Guid>();

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private List<ChatMessageViewModel> allMessages = new List<ChatMessageViewModel>();
        private List<ChatMessageViewModel> messagesToProcess = new List<ChatMessageViewModel>();

        public OverlayChatMessages() : base(ChatMessagesItemType, HTMLTemplate) { }

        public OverlayChatMessages(string htmlText, int totalToShow, int width, string borderColor, string backgroundColor, string textColor,
            string textFont, int textSize, OverlayEffectEntranceAnimationTypeEnum addEventAnimation)
            : base(ChatMessagesItemType, htmlText)
        {
            this.TotalToShow = totalToShow;
            this.Width = width;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
            this.AddEventAnimation = addEventAnimation;
        }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            await Task.Delay(1000);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;
            GlobalEvents.OnChatMessageDeleted += GlobalEvents_OnChatMessageDeleted;

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return await this.semaphore.WaitAndRelease(async () =>
            {
                if (this.allMessages.Count > 0 || this.DeletedMessages.Count > 0)
                {
                    OverlayChatMessages copy = this.Copy<OverlayChatMessages>();

                    this.DeletedMessages.Clear();

                    if (this.allMessages.Count > 0)
                    {
                        int skip = this.allMessages.Count;
                        if (skip > this.TotalToShow)
                        {
                            skip = skip - this.TotalToShow;
                        }
                        else
                        {
                            skip = 0;
                        }

                        this.messagesToProcess = new List<ChatMessageViewModel>(this.allMessages.Skip(skip));
                        this.allMessages.Clear();

                        while (this.messagesToProcess.Count > 0)
                        {
                            OverlayCustomHTMLItem overlayItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
                            copy.Messages.Add(new OverlayChatMessage()
                            {
                                ID = Guid.Parse(this.messagesToProcess.ElementAt(0).ID),
                                Message = overlayItem.HTMLText,
                            });
                            this.messagesToProcess.RemoveAt(0);
                        }
                    }

                    return copy;
                }
                return null;
            });
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayChatMessages>(); }

        protected override async Task<string> PerformReplacement(string text, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return await this.ReplaceStringWithSpecialModifiers(text, user, arguments, extraSpecialIdentifiers);
        }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            ChatMessageViewModel message = this.messagesToProcess.First();

            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["WIDTH"] = this.Width.ToString();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();

            replacementSets["USER_IMAGE"] = message.User.AvatarLink;
            replacementSets["USERNAME"] = message.User.Username;
            replacementSets["USER_COLOR"] = OverlayChatMessages.userColors[message.User.PrimaryRoleColorName];

            replacementSets["SUB_IMAGE"] = "";
            if (message.User.IsPlatformSubscriber && ChannelSession.MixerChannel.badge != null)
            {
                replacementSets["SUB_IMAGE"] = ChannelSession.MixerChannel.badge.url;
            }

            //if (message.Skill != null)
            //{
            //    replacementSets["IMAGE"] = message.Skill.ImageUrl;
            //}
            //else if (message.ChatSkill != null)
            //{
            //    replacementSets["IMAGE"] = message.ChatSkill.icon_url;
            //}
            //else
            //{
            //    StringBuilder text = new StringBuilder();
            //    foreach (ChatMessageDataModel messageData in message.MessageComponents)
            //    {
            //        MixerChatEmoteModel emoticon = MixerChatEmoteModel.GetEmoteForMessageData(messageData);
            //        if (emoticon != null)
            //        {
            //            string emoticonText = OverlayChatMessages.EmoticonMessageHTMLTemplate;
            //            emoticonText = emoticonText.Replace("{EMOTICON}", emoticon.Uri);
            //            emoticonText = emoticonText.Replace("{TEXT_SIZE}", this.TextSize.ToString());
            //            emoticonText = emoticonText.Replace("{EMOTICON_SIZE}", emoticon.Width.ToString());
            //            emoticonText = emoticonText.Replace("{EMOTICON_X}", (-emoticon.X).ToString());
            //            emoticonText = emoticonText.Replace("{EMOTICON_Y}", (-emoticon.Y).ToString());
            //            text.Append(emoticonText + " ");
            //        }
            //        else
            //        {
            //            text.Append(messageData.text + " ");
            //        }
            //    }
            //    replacementSets["TEXT"] = text.ToString().Trim();
            //}

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
        }

        private async void GlobalEvents_OnChatMessageDeleted(object sender, Guid id)
        {
            await this.semaphore.WaitAndRelease(() =>
            {
                this.DeletedMessages.Add(id);
                return Task.FromResult(0);
            });
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayCustomHTMLItem : OverlayItemBase
    {
        public const string CustomItemType = "custom";

        [DataMember]
        public string HTMLText { get; set; }

        public OverlayCustomHTMLItem() : base(OverlayCustomHTMLItem.CustomItemType) { }

        public OverlayCustomHTMLItem(string htmlTemplate) : this(OverlayCustomHTMLItem.CustomItemType, htmlTemplate) { }

        public OverlayCustomHTMLItem(string type, string htmlTemplate)
            : base(type)
        {
            this.HTMLText = htmlTemplate;
        }

        public virtual OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayCustomHTMLItem>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayCustomHTMLItem item = this.GetCopy();
            item.HTMLText = await this.PerformReplacement(item.HTMLText, user, arguments, extraSpecialIdentifiers);
            return item;
        }

        protected virtual async Task<string> PerformReplacement(string text, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            foreach (var kvp in await this.GetReplacementSets(user, arguments, extraSpecialIdentifiers))
            {
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return await this.ReplaceStringWithSpecialModifiers(text, user, arguments, extraSpecialIdentifiers);
        }

        protected virtual Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }

    [Obsolete]
    public enum EventListItemTypeEnum
    {
        Followers,
        Hosts,
        Subscribers,
        Donations,
        Milestones,
        Sparks,
        Embers,
    }

    [Obsolete]
    [DataContract]
    public class OverlayEventListItem
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Details { get; set; }

        public OverlayEventListItem() { }

        public OverlayEventListItem(string name, string details)
        {
            this.Name = name;
            this.Details = details;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayEventList : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
    @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
  <p style=""position: absolute; top: 35%; left: 5%; width: 50%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TOP_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{NAME}</p>
  <p style=""position: absolute; top: 80%; right: 5%; width: 50%; text-align: right; font-family: '{TEXT_FONT}'; font-size: {BOTTOM_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{DETAILS}</p>
</div>";

        public const string EventListItemType = "eventlist";

        [DataMember]
        public List<EventListItemTypeEnum> ItemTypes { get; set; }

        [DataMember]
        public int TotalToShow { get; set; }
        [DataMember]
        public bool ResetOnLoad { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum RemoveEventAnimation { get; set; }
        [DataMember]
        public string RemoveEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.RemoveEventAnimation); } set { } }

        [DataMember]
        private List<OverlayEventListItem> events = new List<OverlayEventListItem>();

        private List<OverlayEventListItem> eventsToAdd = new List<OverlayEventListItem>();

        private HashSet<uint> follows = new HashSet<uint>();
        private HashSet<uint> hosts = new HashSet<uint>();
        private HashSet<uint> subs = new HashSet<uint>();

        public OverlayEventList() : base(EventListItemType, HTMLTemplate) { }

        public OverlayEventList(string htmlText, IEnumerable<EventListItemTypeEnum> itemTypes, int totalToShow, bool resetOnLoad, string textFont, int width, int height,
            string borderColor, string backgroundColor, string textColor, OverlayEffectEntranceAnimationTypeEnum addEventAnimation, OverlayEffectExitAnimationTypeEnum removeEventAnimation)
            : base(EventListItemType, htmlText)
        {
            this.ItemTypes = new List<EventListItemTypeEnum>(itemTypes);
            this.TotalToShow = totalToShow;
            this.ResetOnLoad = resetOnLoad;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.AddEventAnimation = addEventAnimation;
            this.RemoveEventAnimation = removeEventAnimation;
        }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            for (int i = 0; i < 5; i++)
            {
                this.AddEvent("Joe Smoe", "Followed");

                await Task.Delay(1000);
            }
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;
            GlobalEvents.OnPatronageMilestoneReachedOccurred -= GlobalEvents_OnPatronageMilestoneReachedOccurred;

            if (this.ResetOnLoad)
            {
                this.events.Clear();
            }

            if (this.ItemTypes.Contains(EventListItemTypeEnum.Followers))
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Hosts))
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Subscribers))
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Donations))
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Sparks))
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Embers))
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Milestones))
            {
                GlobalEvents.OnPatronageMilestoneReachedOccurred += GlobalEvents_OnPatronageMilestoneReachedOccurred;
            }

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.eventsToAdd.Count > 0)
            {
                return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
            }
            return null;
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayEventList>(); }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayEventListItem eventToAdd = this.eventsToAdd.First();
            this.eventsToAdd.RemoveAt(0);

            if (this.events.Count >= this.TotalToShow)
            {
                this.events.RemoveAt(0);
            }
            this.events.Add(eventToAdd);

            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TOP_TEXT_HEIGHT"] = ((int)(0.4 * ((double)this.Height))).ToString();
            replacementSets["BOTTOM_TEXT_HEIGHT"] = ((int)(0.2 * ((double)this.Height))).ToString();

            replacementSets["NAME"] = eventToAdd.Name;
            replacementSets["DETAILS"] = eventToAdd.Details;

            return Task.FromResult(replacementSets);
        }

        private void AddEvent(string name, string details)
        {
            this.eventsToAdd.Add(new OverlayEventListItem(name, details));
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.MixerID))
            {
                this.follows.Add(user.MixerID);
                this.AddEvent(user.Username, "Followed");
            }
        }

        private void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.MixerID))
            {
                this.hosts.Add(host.Item1.MixerID);
                this.AddEvent(host.Item1.Username, string.Format("Hosted ({0})", host.Item2));
            }
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.MixerID))
            {
                this.subs.Add(user.MixerID);
                this.AddEvent(user.Username, "Subscribed");
            }
        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.MixerID))
            {
                this.subs.Add(user.Item1.MixerID);
                this.AddEvent(user.Item1.Username, string.Format("Resubscribed ({0} months)", user.Item2));
            }
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.AddEvent(donation.Username, string.Format("Donated {0}", donation.AmountText)); }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> sparkUsage) { this.AddEvent(sparkUsage.Item1.Username, string.Format("{0} Sparks", sparkUsage.Item2)); }

        private void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage) { this.AddEvent(emberUsage.User.Username, string.Format("{0} Embers", emberUsage.Amount)); }

        private void GlobalEvents_OnPatronageMilestoneReachedOccurred(object sender, PatronageMilestoneModel patronageMilestone) { this.AddEvent(string.Format("{0} Milestone", patronageMilestone.PercentageAmountText()), string.Format("{0} Sparks", patronageMilestone.target)); }
    }

    [Obsolete]
    [DataContract]
    public class OverlayGameQueueItem
    {
        [DataMember]
        public string HTMLText { get; set; }
    }

    [Obsolete]
    [DataContract]
    public class OverlayGameQueue : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
    @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
  <p style=""position: absolute; top: 50%; left: 5%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">#{POSITION} {USERNAME}</p>
</div>";

        private const string GameQueueUserPositionSpecialIdentifier = "gamequeueuserposition";

        public const string GameQueueItemType = "gamequeue";

        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum RemoveEventAnimation { get; set; }
        [DataMember]
        public string RemoveEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.RemoveEventAnimation); } set { } }

        [DataMember]
        public List<OverlayGameQueueItem> GameQueueUpdates = new List<OverlayGameQueueItem>();

        [JsonIgnore]
        private bool gameQueueUpdated = true;

        [JsonIgnore]
        private List<UserViewModel> testGameQueueList = new List<UserViewModel>();

        public OverlayGameQueue() : base(GameQueueItemType, HTMLTemplate) { }

        public OverlayGameQueue(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayEffectEntranceAnimationTypeEnum addEventAnimation, OverlayEffectExitAnimationTypeEnum removeEventAnimation)
            : base(GameQueueItemType, htmlText)
        {
            this.TotalToShow = totalToShow;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.AddEventAnimation = addEventAnimation;
            this.RemoveEventAnimation = removeEventAnimation;
        }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            for (int i = 0; i < 5; i++)
            {
                this.testGameQueueList.Add(await ChannelSession.GetCurrentUser());
                this.gameQueueUpdated = true;
                await Task.Delay(1500);
            }
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnGameQueueUpdated += GlobalEvents_OnGameQueueUpdated;

            this.testGameQueueList.Clear();

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.gameQueueUpdated)
            {
                this.gameQueueUpdated = false;

                List<UserViewModel> users = new List<UserViewModel>((this.testGameQueueList.Count > 0) ? this.testGameQueueList : ChannelSession.Services.GameQueueService.Queue);

                this.GameQueueUpdates.Clear();
                OverlayGameQueue copy = this.Copy<OverlayGameQueue>();
                for (int i = 0; i < users.Count && i < this.TotalToShow; i++)
                {
                    extraSpecialIdentifiers[GameQueueUserPositionSpecialIdentifier] = (i + 1).ToString();
                    OverlayCustomHTMLItem overlayItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(users[i], arguments, extraSpecialIdentifiers);
                    copy.GameQueueUpdates.Add(new OverlayGameQueueItem() { HTMLText = overlayItem.HTMLText });
                }
                return copy;
            }
            return null;
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayEventList>(); }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_HEIGHT"] = ((int)(0.4 * ((double)this.Height))).ToString();

            replacementSets["USERNAME"] = user.Username;
            replacementSets["POSITION"] = extraSpecialIdentifiers[GameQueueUserPositionSpecialIdentifier];

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnGameQueueUpdated(object sender, System.EventArgs e) { this.gameQueueUpdated = true; }
    }

    [Obsolete]
    public class OverlayGameStats : OverlayItemBase
    {
        public override Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            throw new NotImplementedException();
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayHTMLItem : OverlayItemBase
    {
        public const string HTMLItemType = "html";

        [DataMember]
        public string HTMLText { get; set; }

        public OverlayHTMLItem() : base(HTMLItemType) { }

        public OverlayHTMLItem(string htmlText)
            : base(HTMLItemType)
        {
            this.HTMLText = htmlText;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayHTMLItem item = this.Copy<OverlayHTMLItem>();
            item.HTMLText = await this.ReplaceStringWithSpecialModifiers(item.HTMLText, user, arguments, extraSpecialIdentifiers);
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayImageItem : OverlayItemBase
    {
        public const string ImageItemType = "image";

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public string FileID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}?nonce={1}", this.FileID, Guid.NewGuid());
                }
                return this.FilePath;
            }
            set { }
        }

        public OverlayImageItem() : base(ImageItemType) { }

        public OverlayImageItem(string filepath, int width, int height)
            : base(ImageItemType)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.FileID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayImageItem item = this.Copy<OverlayImageItem>();
            item.FilePath = await this.ReplaceStringWithSpecialModifiers(item.FilePath, user, arguments, extraSpecialIdentifiers);
            if (!Uri.IsWellFormedUriString(item.FilePath, UriKind.RelativeOrAbsolute))
            {
                item.FilePath = item.FilePath.ToFilePathString();
            }
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public abstract class OverlayItemBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string ItemType { get; set; }

        [JsonIgnore]
        public bool IsInitialized { get; private set; }

        public OverlayItemBase()
        {
            this.ID = Guid.NewGuid();
        }

        public OverlayItemBase(string itemType)
            : this()
        {
            this.ItemType = itemType;
        }

        [JsonIgnore]
        public virtual bool SupportsTestButton { get { return false; } }

        public virtual Task LoadTestData() { return Task.FromResult(0); }

        public abstract Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);

        public virtual Task Initialize()
        {
            this.IsInitialized = true;
            return Task.FromResult(0);
        }

        public virtual Task Disable()
        {
            this.IsInitialized = false;
            return Task.FromResult(0);
        }

        public T Copy<T>() { return SerializerHelper.DeserializeFromString<T>(SerializerHelper.SerializeToString(this)); }

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, bool encode = false)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str, encode: encode);
            if (extraSpecialIdentifiers != null)
            {
                foreach (var kvp in extraSpecialIdentifiers)
                {
                    siString.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
                }
            }
            await siString.ReplaceCommonSpecialModifiers(user, arguments);
            return siString.ToString();
        }
    }

    [Obsolete]
    public enum OverlayEffectEntranceAnimationTypeEnum
    {
        None,

        [Name("Bounce In")]
        BounceIn,
        [Name("Bounce In Up")]
        BounceInUp,
        [Name("Bounce In Down")]
        BounceInDown,
        [Name("Bounce In Left")]
        BounceInLeft,
        [Name("Bounce In Right")]
        BounceInRight,

        [Name("Fade In")]
        FadeIn,
        [Name("Fade In Up")]
        FadeInUp,
        [Name("Fade In Down")]
        FadeInDown,
        [Name("Fade In Left")]
        FadeInLeft,
        [Name("Fade In Right")]
        FadeInRight,

        [Name("Flip In X")]
        FlipInX,
        [Name("Flip In Y")]
        FlipInY,

        [Name("Light Speed In")]
        LightSpeedIn,

        [Name("Rotate In")]
        RotateIn,


        [Name("Rotate In Up")]
        [Obsolete]
        RotateInUp,
        [Name("Rotate In Down")]
        [Obsolete]
        RotateInDown,
        [Name("Rotate In Left")]
        [Obsolete]
        RotateInLeft,
        [Name("Rotate In Right")]
        [Obsolete]
        RotateInRight,


        [Name("Slide In Up")]
        SlideInUp,
        [Name("Slide In Down")]
        SlideInDown,
        [Name("Slide In Left")]
        SlideInLeft,
        [Name("Slide In Right")]
        SlideInRight,

        [Name("Zoom In")]
        ZoomIn,
        [Name("Zoom In Up")]
        ZoomInUp,
        [Name("Zoom In Down")]
        ZoomInDown,
        [Name("Zoom In Left")]
        ZoomInLeft,
        [Name("Zoom In Right")]
        ZoomInRight,

        [Name("Jack In The Box")]
        JackInTheBox,

        [Name("Roll In")]
        RollIn,

        Random,
    }

    [Obsolete]
    public enum OverlayEffectVisibleAnimationTypeEnum
    {
        None,

        Bounce,
        Flash,
        Pulse,
        [Name("Rubber Band")]
        RubberBand,
        Shake,
        Swing,
        Tada,
        Wobble,
        Jello,
        Flip,

        Random,
    }

    [Obsolete]
    public enum OverlayEffectExitAnimationTypeEnum
    {
        None,

        [Name("Bounce Out")]
        BounceOut,
        [Name("Bounce Out Up")]
        BounceOutUp,
        [Name("Bounce Out Down")]
        BounceOutDown,
        [Name("Bounce Out Left")]
        BounceOutLeft,
        [Name("Bounce Out Right")]
        BounceOutRight,

        [Name("Fade Out")]
        FadeOut,
        [Name("Fade Out Up")]
        FadeOutUp,
        [Name("Fade Out Down")]
        FadeOutDown,
        [Name("Fade Out Left")]
        FadeOutLeft,
        [Name("Fade Out Right")]
        FadeOutRight,

        [Name("Flip Out X")]
        FlipOutX,
        [Name("Flip Out Y")]
        FlipOutY,

        [Name("Light Speed Out")]
        LightSpeedOut,

        [Name("Rotate Out")]
        RotateOut,


        [Name("Rotate Out Up")]
        [Obsolete]
        RotateOutUp,
        [Name("Rotate Out Down")]
        [Obsolete]
        RotateOutDown,
        [Name("Rotate Out Left")]
        [Obsolete]
        RotateOutLeft,
        [Name("Rotate Out Right")]
        [Obsolete]
        RotateOutRight,


        [Name("Slide Out Up")]
        SlideOutUp,
        [Name("Slide Out Down")]
        SlideOutDown,
        [Name("Slide Out Left")]
        SlideOutLeft,
        [Name("Slide Out Right")]
        SlideOutRight,

        [Name("Zoom Out")]
        ZoomOut,
        [Name("Zoom Out Up")]
        ZoomOutUp,
        [Name("Zoom Out Down")]
        ZoomOutDown,
        [Name("Zoom Out Left")]
        ZoomOutLeft,
        [Name("Zoom Out Right")]
        ZoomOutRight,

        Hinge,

        [Name("Roll Out")]
        RollOut,

        Random,
    }

    [Obsolete]
    [DataContract]
    public class OverlayItemEffects
    {
        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum EntranceAnimation { get; set; }
        [DataMember]
        public string EntranceAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.EntranceAnimation); } set { } }
        [DataMember]
        public OverlayEffectVisibleAnimationTypeEnum VisibleAnimation { get; set; }
        [DataMember]
        public string VisibleAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.VisibleAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum ExitAnimation { get; set; }
        [DataMember]
        public string ExitAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.ExitAnimation); } set { } }

        [DataMember]
        public double Duration;

        public OverlayItemEffects() { }

        public OverlayItemEffects(OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectVisibleAnimationTypeEnum visible, OverlayEffectExitAnimationTypeEnum exit, double duration)
        {
            this.EntranceAnimation = entrance;
            this.VisibleAnimation = visible;
            this.ExitAnimation = exit;
            this.Duration = duration;
        }

        public static string GetAnimationClassName<T>(T animationType)
        {
            string name = animationType.ToString();

            if (EnumHelper.IsObsolete(animationType))
            {
                name = string.Empty;
            }

            if (!string.IsNullOrEmpty(name) && name.Equals("Random"))
            {
                List<T> values = EnumHelper.GetEnumList<T>().ToList();
                values.RemoveAll(v => v.ToString().Equals("None") || v.ToString().Equals("Random"));
                name = values[RandomHelper.GenerateRandomNumber(values.Count)].ToString();
            }

            if (!string.IsNullOrEmpty(name) && !name.Equals("None"))
            {
                return Char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
            return string.Empty;
        }
    }

    [Obsolete]
    public enum OverlayEffectPositionType
    {
        Percentage,
        Pixel,
    }

    [Obsolete]
    public class OverlayItemPosition
    {
        [DataMember]
        public OverlayEffectPositionType PositionType;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;

        [DataMember]
        public bool IsPercentagePosition { get { return this.PositionType == OverlayEffectPositionType.Percentage; } }
        [DataMember]
        public bool IsPixelPosition { get { return this.PositionType == OverlayEffectPositionType.Pixel; } }

        public OverlayItemPosition() { }

        public OverlayItemPosition(OverlayEffectPositionType positionType, int horizontal, int vertical)
        {
            this.PositionType = positionType;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
        }
    }

    [Obsolete]
    public enum LeaderboardTypeEnum
    {
        Subscribers,
        Donations,
        [Name("Currency/Rank")]
        CurrencyRank,
        Sparks,
        Embers,
    }

    [Obsolete]
    public enum LeaderboardSparksEmbersDateEnum
    {
        Weekly,
        Monthly,
        Yearly,
        [Name("All Time")]
        AllTime,
    }

    [Obsolete]
    [DataContract]
    public class OverlayLeaderboard : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 35%; left: 5%; width: 50%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TOP_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{USERNAME}</p>
          <p style=""position: absolute; top: 80%; right: 5%; width: 50%; text-align: right; font-family: '{TEXT_FONT}'; font-size: {BOTTOM_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{DETAILS}</p>
        </div>";

        public const string LeaderboardItemType = "leaderboard";

        [DataMember]
        public LeaderboardTypeEnum LeaderboardType { get; set; }
        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum RemoveEventAnimation { get; set; }
        [DataMember]
        public string RemoveEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.RemoveEventAnimation); } set { } }

        [DataMember]
        public Guid CurrencyID { get; set; }

        [DataMember]
        public LeaderboardSparksEmbersDateEnum DateRange { get; set; }

        [DataMember]
        public List<string> LeaderboardEntries = new List<string>();

        private Dictionary<UserViewModel, DateTimeOffset> userSubDates = new Dictionary<UserViewModel, DateTimeOffset>();
        private bool refreshSubscribers = true;

        private Dictionary<string, UserDonationModel> userDonations = new Dictionary<string, UserDonationModel>();
        private bool refreshDonations = true;

        private DateTimeOffset lastRefresh = DateTimeOffset.MinValue;
        private List<UserDataModel> currencyUsersToShow = new List<UserDataModel>();
        private IEnumerable<SparksLeaderboardModel> sparkLeaders;
        private IEnumerable<EmbersLeaderboardModel> emberLeaders;

        public OverlayLeaderboard() : base(LeaderboardItemType, HTMLTemplate) { }

        public OverlayLeaderboard(string htmlText, LeaderboardTypeEnum leaderboardType, int totalToShow, string borderColor, string backgroundColor, string textColor,
            string textFont, int width, int height, OverlayEffectEntranceAnimationTypeEnum addEvent, OverlayEffectExitAnimationTypeEnum removeEvent, UserCurrencyModel currency)
            : this(htmlText, leaderboardType, totalToShow, borderColor, backgroundColor, textColor, textFont, width, height, addEvent, removeEvent)
        {
            this.CurrencyID = currency.ID;
        }

        public OverlayLeaderboard(string htmlText, LeaderboardTypeEnum leaderboardType, int totalToShow, string borderColor, string backgroundColor, string textColor,
            string textFont, int width, int height, OverlayEffectEntranceAnimationTypeEnum addEvent, OverlayEffectExitAnimationTypeEnum removeEvent, LeaderboardSparksEmbersDateEnum date)
            : this(htmlText, leaderboardType, totalToShow, borderColor, backgroundColor, textColor, textFont, width, height, addEvent, removeEvent)
        {
            this.DateRange = date;
        }

        public OverlayLeaderboard(string htmlText, LeaderboardTypeEnum leaderboardType, int totalToShow, string borderColor, string backgroundColor, string textColor,
            string textFont, int width, int height, OverlayEffectEntranceAnimationTypeEnum addEvent, OverlayEffectExitAnimationTypeEnum removeEvent)
            : base(LeaderboardItemType, htmlText)
        {
            this.LeaderboardType = leaderboardType;
            this.TotalToShow = totalToShow;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.AddEventAnimation = addEvent;
            this.RemoveEventAnimation = removeEvent;
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;

            if (this.LeaderboardType == LeaderboardTypeEnum.Subscribers)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            else if (this.LeaderboardType == LeaderboardTypeEnum.Donations)
            {
                if (ChannelSession.Services.Streamlabs.IsConnected)
                {
                    foreach (StreamlabsDonation donation in await ChannelSession.Services.Streamlabs.GetDonations(int.MaxValue))
                    {
                        if (!this.userDonations.ContainsKey(donation.UserName))
                        {
                            this.userDonations[donation.UserName] = donation.ToGenericDonation();
                            this.userDonations[donation.UserName].Amount = 0.0;
                        }
                        this.userDonations[donation.UserName].Amount += donation.Amount;
                    }
                }

                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }

            await base.Initialize();
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayLeaderboard>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayLeaderboard copy = (OverlayLeaderboard)this.GetCopy();
            if (this.LeaderboardType == LeaderboardTypeEnum.Subscribers && this.refreshSubscribers)
            {
                this.refreshSubscribers = false;

                List<KeyValuePair<UserViewModel, DateTimeOffset>> usersToShow = new List<KeyValuePair<UserViewModel, DateTimeOffset>>();

                var orderedUsers = userSubDates.OrderByDescending(kvp => kvp.Value.TotalDaysFromNow());
                for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                {
                    usersToShow.Add(orderedUsers.ElementAt(i));
                }

                foreach (KeyValuePair<UserViewModel, DateTimeOffset> userToShow in usersToShow)
                {
                    extraSpecialIdentifiers["DETAILS"] = userToShow.Value.GetAge();
                    OverlayCustomHTMLItem htmlItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(userToShow.Key, arguments, extraSpecialIdentifiers);
                    copy.LeaderboardEntries.Add(htmlItem.HTMLText);
                }
                return copy;
            }
            else if (this.LeaderboardType == LeaderboardTypeEnum.Donations && this.refreshDonations)
            {
                this.refreshDonations = false;

                List<UserDonationModel> topDonators = new List<UserDonationModel>();

                var orderedUsers = this.userDonations.OrderByDescending(kvp => kvp.Value.Amount);
                for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                {
                    topDonators.Add(orderedUsers.ElementAt(i).Value);
                }

                foreach (UserDonationModel topDonator in topDonators)
                {
                    extraSpecialIdentifiers["DETAILS"] = topDonator.AmountText;
                    OverlayCustomHTMLItem htmlItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(topDonator.User, arguments, extraSpecialIdentifiers);
                    copy.LeaderboardEntries.Add(htmlItem.HTMLText);
                }
                return copy;
            }
            else if (this.LeaderboardType == LeaderboardTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
                {
                    UserCurrencyModel currency = ChannelSession.Settings.Currencies[this.CurrencyID];
                    if (this.lastRefresh < DateTimeOffset.Now)
                    {
                        this.lastRefresh = DateTimeOffset.Now.AddMinutes(1);

                        Dictionary<uint, int> currencyAmounts = new Dictionary<uint, int>();
                        //foreach (UserDataModel userData in ChannelSession.Settings.UserData.Values)
                        //{
                        //    currencyAmounts[userData.MixerID] = currency.GetAmount(userData);
                        //}

                        this.currencyUsersToShow.Clear();
                        for (int i = 0; i < this.TotalToShow && i < currencyAmounts.Count; i++)
                        {
                            try
                            {
                                KeyValuePair<uint, int> top = currencyAmounts.Aggregate((current, highest) => (current.Key <= 0 || current.Value < highest.Value) ? highest : current);
                                if (!top.Equals(default(KeyValuePair<uint, int>)))
                                {
                                    //this.currencyUsersToShow.Add(ChannelSession.Settings.UserData[top.Key]);
                                    currencyAmounts.Remove(top.Key);
                                }
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                    }

                    foreach (UserDataModel userToShow in this.currencyUsersToShow)
                    {
                        extraSpecialIdentifiers["DETAILS"] = currency.GetAmount(userToShow).ToString();
                        OverlayCustomHTMLItem htmlItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(new UserViewModel(userToShow), arguments, extraSpecialIdentifiers);
                        copy.LeaderboardEntries.Add(htmlItem.HTMLText);
                    }
                    return copy;
                }
            }
            else if (this.LeaderboardType == LeaderboardTypeEnum.Sparks)
            {
                if (this.lastRefresh < DateTimeOffset.Now)
                {
                    this.lastRefresh = DateTimeOffset.Now.AddMinutes(1);
                    switch (this.DateRange)
                    {
                        case LeaderboardSparksEmbersDateEnum.Weekly:
                            this.sparkLeaders = await ChannelSession.MixerUserConnection.GetWeeklySparksLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                            break;
                        case LeaderboardSparksEmbersDateEnum.Monthly:
                            this.sparkLeaders = await ChannelSession.MixerUserConnection.GetMonthlySparksLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                            break;
                        case LeaderboardSparksEmbersDateEnum.Yearly:
                            this.sparkLeaders = await ChannelSession.MixerUserConnection.GetYearlySparksLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                            break;
                        case LeaderboardSparksEmbersDateEnum.AllTime:
                            this.sparkLeaders = await ChannelSession.MixerUserConnection.GetAllTimeSparksLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                            break;
                    }
                }
                if (this.sparkLeaders != null)
                {
                    foreach (SparksLeaderboardModel sparkLeader in this.sparkLeaders)
                    {
                        //extraSpecialIdentifiers["DETAILS"] = sparkLeader.statValue.ToString();
                        //OverlayCustomHTMLItem htmlItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(new UserViewModel(sparkLeader.username), arguments, extraSpecialIdentifiers);
                        //copy.LeaderboardEntries.Add(htmlItem.HTMLText);
                    }
                    return copy;
                }
            }
            else if (this.LeaderboardType == LeaderboardTypeEnum.Embers)
            {
                if (this.lastRefresh < DateTimeOffset.Now)
                {
                    this.lastRefresh = DateTimeOffset.Now.AddMinutes(1);
                    switch (this.DateRange)
                    {
                        case LeaderboardSparksEmbersDateEnum.Weekly:
                            this.emberLeaders = await ChannelSession.MixerUserConnection.GetWeeklyEmbersLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                            break;
                        case LeaderboardSparksEmbersDateEnum.Monthly:
                            this.emberLeaders = await ChannelSession.MixerUserConnection.GetMonthlyEmbersLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                            break;
                        case LeaderboardSparksEmbersDateEnum.Yearly:
                            this.emberLeaders = await ChannelSession.MixerUserConnection.GetYearlyEmbersLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                            break;
                        case LeaderboardSparksEmbersDateEnum.AllTime:
                            this.emberLeaders = await ChannelSession.MixerUserConnection.GetAllTimeEmbersLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                            break;
                    }
                }

                if (this.emberLeaders != null)
                {
                    foreach (EmbersLeaderboardModel emberLeader in this.emberLeaders)
                    {
                        //extraSpecialIdentifiers["DETAILS"] = emberLeader.statValue.ToString();
                        //OverlayCustomHTMLItem htmlItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(new UserViewModel(emberLeader.username), arguments, extraSpecialIdentifiers);
                        //copy.LeaderboardEntries.Add(htmlItem.HTMLText);
                    }
                    return copy;
                }
            }
            return null;
        }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TOP_TEXT_HEIGHT"] = ((int)(0.4 * ((double)this.Height))).ToString();
            replacementSets["BOTTOM_TEXT_HEIGHT"] = ((int)(0.2 * ((double)this.Height))).ToString();

            replacementSets["USERNAME"] = user.Username;
            if (extraSpecialIdentifiers.ContainsKey("DETAILS"))
            {
                replacementSets["DETAILS"] = extraSpecialIdentifiers["DETAILS"];
            }

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            userSubDates[user] = DateTimeOffset.Now;
            this.refreshSubscribers = true;
        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { this.refreshSubscribers = true; }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            if (!this.userDonations.ContainsKey(donation.Username))
            {
                this.userDonations[donation.Username] = donation.Copy();
                this.userDonations[donation.Username].Amount = 0.0;
            }
            this.userDonations[donation.Username].Amount += donation.Amount;
            this.refreshDonations = true;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayMixerClip : OverlayItemBase
    {
        public const string MixerClipItemType = "mixerclip";

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum EntranceAnimation { get; set; }
        [DataMember]
        public string EntranceAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.EntranceAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum ExitAnimation { get; set; }
        [DataMember]
        public string ExitAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.ExitAnimation); } set { } }

        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public double Duration { get; set; }

        private ClipModel lastClip = null;

        public OverlayMixerClip() : base(OverlayMixerClip.MixerClipItemType) { }

        public OverlayMixerClip(int width, int height, int volume, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectExitAnimationTypeEnum exit)
            : base(OverlayMixerClip.MixerClipItemType)
        {
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.EntranceAnimation = entrance;
            this.ExitAnimation = exit;
        }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } set { } }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            this.lastClip = new ClipModel()
            {
                contentLocators = new List<ClipLocatorModel>()
                {
                    new ClipLocatorModel()
                    {
                        locatorType = MixerClipsAction.VideoFileContentLocatorType,
                        uri = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Wiki/MixerTestClip/manifest.m3u8"
                    }
                },
                durationInSeconds = 5
            };
            await Task.Delay(5000);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnMixerClipCreated += GlobalEvents_OnMixerClipCreated;

            await base.Initialize();
        }

        public override Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.lastClip != null)
            {
                ClipModel clip = this.lastClip;
                this.lastClip = null;

                OverlayMixerClip item = this.Copy<OverlayMixerClip>();
                ClipLocatorModel clipLocator = clip.contentLocators.FirstOrDefault(cl => cl.locatorType.Equals(MixerClipsAction.VideoFileContentLocatorType));
                if (clipLocator != null)
                {
                    item.URL = clipLocator.uri;
                    item.Duration = Math.Max(0, clip.durationInSeconds - 1);
                    return Task.FromResult<OverlayItemBase>(item);
                }
            }
            return Task.FromResult<OverlayItemBase>(null);
        }

        private void GlobalEvents_OnMixerClipCreated(object sender, ClipModel clip) { this.lastClip = clip; }
    }

    [Obsolete]
    public enum ProgressBarTypeEnum
    {
        Followers,
        Subscribers,
        Donations,
        Sparks,
        Milestones,
        Custom,
        Embers,
    }

    [Obsolete]
    [DataContract]
    public class OverlayProgressBar : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
            @"<div style=""position: absolute; background-color: {BACKGROUND_COLOR}; width: {BAR_WIDTH}px; height: {BAR_HEIGHT}px; transform: translate(-50%, -50%);"">
    <div style=""position: absolute; background-color: {PROGRESS_COLOR}; width: {PROGRESS_WIDTH}px; height: {BAR_HEIGHT}px;""></div>
</div>
<p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{AMOUNT} ({PERCENTAGE}%)</p>";

        public const string GoalReachedCommandName = "On Goal Reached";

        [DataMember]
        public ProgressBarTypeEnum ProgressBarType { get; set; }

        [DataMember]
        public double CurrentAmountNumber { get; set; }
        [DataMember]
        public double GoalAmountNumber { get; set; }

        [DataMember]
        public string CurrentAmountCustom { get; set; }
        [DataMember]
        public string GoalAmountCustom { get; set; }

        [DataMember]
        public int ResetAfterDays { get; set; }

        [DataMember]
        public string ProgressColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public CustomCommand GoalReachedCommand { get; set; }

        [DataMember]
        private DateTimeOffset LastReset { get; set; }
        [DataMember]
        private bool GoalReached { get; set; }

        private int totalFollowers = 0;

        private bool refreshMilestone;

        public OverlayProgressBar() : base(CustomItemType, HTMLTemplate) { }

        public OverlayProgressBar(string htmlText, ProgressBarTypeEnum progressBarType, double currentAmount, double goalAmount, int resetAfterDays, string progressColor,
            string backgroundColor, string textColor, string textFont, int width, int height, CustomCommand goalReachedCommand)
            : this(htmlText, progressBarType, resetAfterDays, progressColor, backgroundColor, textColor, textFont, width, height, goalReachedCommand)
        {
            this.CurrentAmountNumber = currentAmount;
            this.GoalAmountNumber = goalAmount;
        }

        public OverlayProgressBar(string htmlText, ProgressBarTypeEnum progressBarType, string currentAmount, string goalAmount, int resetAfterDays, string progressColor,
            string backgroundColor, string textColor, string textFont, int width, int height, CustomCommand goalReachedCommand)
            : this(htmlText, progressBarType, resetAfterDays, progressColor, backgroundColor, textColor, textFont, width, height, goalReachedCommand)
        {
            this.CurrentAmountCustom = currentAmount;
            this.GoalAmountCustom = goalAmount;
        }

        private OverlayProgressBar(string htmlText, ProgressBarTypeEnum progressBarType, int resetAfterDays, string progressColor, string backgroundColor, string textColor,
            string textFont, int width, int height, CustomCommand goalReachedCommand)
            : base(CustomItemType, htmlText)
        {
            this.ProgressBarType = progressBarType;
            this.ResetAfterDays = resetAfterDays;
            this.ProgressColor = progressColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.GoalReachedCommand = goalReachedCommand;
            this.LastReset = DateTimeOffset.Now;
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnUnfollowOccurred -= GlobalEvents_OnUnfollowOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;
            GlobalEvents.OnPatronageUpdateOccurred -= GlobalEvents_OnPatronageUpdateOccurred;
            GlobalEvents.OnPatronageMilestoneReachedOccurred -= GlobalEvents_OnPatronageMilestoneReachedOccurred;

            if (this.ProgressBarType == ProgressBarTypeEnum.Followers)
            {
                totalFollowers = (int)ChannelSession.MixerChannel.numFollowers;

                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
                GlobalEvents.OnUnfollowOccurred += GlobalEvents_OnUnfollowOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Subscribers)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Donations)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Sparks)
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Embers)
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Milestones)
            {
                PatronageStatusModel patronageStatus = await ChannelSession.MixerUserConnection.GetPatronageStatus(ChannelSession.MixerChannel);
                if (patronageStatus != null)
                {
                    this.CurrentAmountNumber = patronageStatus.patronageEarned;
                }

                PatronageMilestoneModel currentMilestone = await ChannelSession.MixerUserConnection.GetCurrentPatronageMilestone();
                if (currentMilestone != null)
                {
                    this.GoalAmountNumber = currentMilestone.target;
                }

                GlobalEvents.OnPatronageUpdateOccurred += GlobalEvents_OnPatronageUpdateOccurred;
                GlobalEvents.OnPatronageMilestoneReachedOccurred += GlobalEvents_OnPatronageMilestoneReachedOccurred;
            }

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.ResetAfterDays > 0 && this.LastReset.TotalDaysFromNow() > this.ResetAfterDays)
            {
                if (this.CurrentAmountNumber > 0)
                {
                    this.CurrentAmountNumber = 0;
                }
                this.LastReset = DateTimeOffset.Now;
            }
            return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
        }

        public override OverlayCustomHTMLItem GetCopy()
        {
            OverlayProgressBar copy = this.Copy<OverlayProgressBar>();
            copy.GoalReachedCommand = null;
            return copy;
        }

        protected override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["PROGRESS_COLOR"] = this.ProgressColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["BAR_WIDTH"] = this.Width.ToString();
            replacementSets["BAR_HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = ((3 * this.Height) / 4).ToString();

            double amount = this.CurrentAmountNumber;
            double goal = this.GoalAmountNumber;

            if (this.ProgressBarType == ProgressBarTypeEnum.Followers)
            {
                amount = this.totalFollowers;
                if (this.CurrentAmountNumber >= 0)
                {
                    amount = this.totalFollowers - this.CurrentAmountNumber;
                }
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Milestones)
            {
                if (this.refreshMilestone)
                {
                    this.refreshMilestone = false;
                    PatronageMilestoneModel currentMilestone = await ChannelSession.MixerUserConnection.GetCurrentPatronageMilestone();
                    if (currentMilestone != null)
                    {
                        goal = this.GoalAmountNumber = currentMilestone.target;
                    }
                }
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Custom)
            {
                if (!string.IsNullOrEmpty(this.CurrentAmountCustom))
                {
                    string customAmount = await this.ReplaceStringWithSpecialModifiers(this.CurrentAmountCustom, user, arguments, extraSpecialIdentifiers);
                    double.TryParse(customAmount, out amount);
                }
                if (!string.IsNullOrEmpty(this.GoalAmountCustom))
                {
                    string customGoal = await this.ReplaceStringWithSpecialModifiers(this.GoalAmountCustom, user, arguments, extraSpecialIdentifiers);
                    double.TryParse(customGoal, out goal);
                }
            }

            double percentage = (amount / goal);

            if (!this.GoalReached && percentage >= 1.0)
            {
                this.GoalReached = true;
                if (this.GoalReachedCommand != null)
                {
                    await this.GoalReachedCommand.Perform();
                }
            }

            replacementSets["AMOUNT"] = amount.ToString();
            replacementSets["GOAL"] = goal.ToString();
            replacementSets["PERCENTAGE"] = ((int)(percentage * 100)).ToString();
            if (goal > 0)
            {
                int progressWidth = (int)(((double)this.Width) * percentage);
                progressWidth = MathHelper.Clamp(progressWidth, 0, this.Width);
                replacementSets["PROGRESS_WIDTH"] = progressWidth.ToString();
            }

            return replacementSets;
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user) { this.totalFollowers++; }

        private void GlobalEvents_OnUnfollowOccurred(object sender, UserViewModel user) { this.totalFollowers--; }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user) { this.CurrentAmountNumber++; }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { this.CurrentAmountNumber++; }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.CurrentAmountNumber += donation.Amount; }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> user) { this.CurrentAmountNumber += user.Item2; }

        private void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage) { this.CurrentAmountNumber += emberUsage.Amount; }

        private void GlobalEvents_OnPatronageUpdateOccurred(object sender, PatronageStatusModel patronageStatus) { this.CurrentAmountNumber = patronageStatus.patronageEarned; }

        private void GlobalEvents_OnPatronageMilestoneReachedOccurred(object sender, PatronageMilestoneModel patronageMilestone) { this.refreshMilestone = true; }
    }

    [Obsolete]
    [DataContract]
    public class OverlaySongRequestItem
    {
        [DataMember]
        public string HTMLText { get; set; }
    }

    [Obsolete]
    [DataContract]
    public class OverlaySongRequests : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
            <img src=""{SONG_IMAGE}"" width=""{SONG_IMAGE_SIZE}"" height=""{SONG_IMAGE_SIZE}"" style=""position: absolute; top: 50%; transform: translate(0%, -50%); margin-left: 10px;"">
            <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; position: absolute; top: 50%; left: 28%; transform: translate(0%, -50%);"">{SONG_NAME}</span>
        </div>";

        public const string SongRequestsItemType = "songrequestsqueue";

        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum RemoveEventAnimation { get; set; }
        [DataMember]
        public string RemoveEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.RemoveEventAnimation); } set { } }

        [DataMember]
        public List<OverlaySongRequestItem> SongRequestUpdates = new List<OverlaySongRequestItem>();

        public OverlaySongRequests() : base(SongRequestsItemType, HTMLTemplate) { }

        public OverlaySongRequests(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayEffectEntranceAnimationTypeEnum addEventAnimation, OverlayEffectExitAnimationTypeEnum removeEventAnimation)
            : base(SongRequestsItemType, htmlText)
        {
            this.TotalToShow = totalToShow;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.AddEventAnimation = addEventAnimation;
            this.RemoveEventAnimation = removeEventAnimation;
        }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(1500);
            }
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnSongRequestsChangedOccurred += GlobalEvents_OnSongRequestsChangedOccurred;

            await base.Initialize();
        }

        public override Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult<OverlayItemBase>(null);
            // TODO: Remove?
            //if (ChannelSession.Services.SongRequestService != null && this.songRequestsUpdated)
            //{
            //    this.songRequestsUpdated = false;

            //    List<SongRequestModel> songRequests = new List<SongRequestModel>();

            //    SongRequestModel currentlyPlaying = await ChannelSession.Services.SongRequestService.GetCurrent();
            //    if (currentlyPlaying != null)
            //    {
            //        songRequests.Add(currentlyPlaying);
            //    }

            //    IEnumerable<SongRequestModel> allSongRequests = this.testSongRequestsList;
            //    if (this.testSongRequestsList.Count == 0)
            //    {
            //        allSongRequests = ChannelSession.Services.SongRequestService.RequestSongs.ToList();
            //    }

            //    foreach (SongRequestModel songRequest in allSongRequests)
            //    {
            //        if (!songRequests.Any(sr => sr.Equals(songRequest)))
            //        {
            //            songRequests.Add(songRequest);
            //        }
            //    }

            //    this.SongRequestUpdates.Clear();
            //    this.currentSongRequests.Clear();

            //    OverlaySongRequests copy = this.Copy<OverlaySongRequests>();
            //    for (int i = 0; i < songRequests.Count() && i < this.TotalToShow; i++)
            //    {
            //        this.currentSongRequests.Add(songRequests.ElementAt(i));
            //    }

            //    while (this.currentSongRequests.Count > 0)
            //    {
            //        OverlayCustomHTMLItem overlayItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
            //        copy.SongRequestUpdates.Add(new OverlaySongRequestItem() { HTMLText = overlayItem.HTMLText });
            //        this.currentSongRequests.RemoveAt(0);
            //    }

            //    return copy;
            //}
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlaySongRequests>(); }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_SIZE"] = ((int)(0.2 * ((double)this.Height))).ToString();

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnSongRequestsChangedOccurred(object sender, System.EventArgs e) { }
    }

    [Obsolete]
    [DataContract]
    public class OverlayStreamBoss : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
        @"<table cellpadding=""10"" style=""border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px;"">
          <tbody>
            <tr>
              <td rowspan=""2"">
                <img src=""{USER_IMAGE}"" width=""{USER_IMAGE_SIZE}"" height=""{USER_IMAGE_SIZE}"" style=""vertical-align: middle;"">
              </td>
              <td style=""padding-bottom: 0px;"">
                <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR};"">{USERNAME}</span>
              </td>
              <td style=""padding-bottom: 0px;"">
                <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right; margin-right: 10px"">{HEALTH_REMAINING} / {MAXIMUM_HEALTH}</span>
              </td>
            </tr>
            <tr>
              <td colspan=""2"" style=""padding-top: 0px;"">
                <div style=""background-color: black; height: {TEXT_SIZE}px; margin-right: 10px"">
                  <div style=""background-color: {PROGRESS_COLOR}; width: {PROGRESS_WIDTH}%; height: {TEXT_SIZE}px;""></div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>";

        public const string StreamBossItemType = "streamboss";

        public const string NewStreamBossCommandName = "On New Stream Boss";

        [DataMember]
        public int StartingHealth { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public string ProgressColor { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public double FollowBonus { get; set; }
        [DataMember]
        public double HostBonus { get; set; }
        [DataMember]
        public double SubscriberBonus { get; set; }
        [DataMember]
        public double DonationBonus { get; set; }
        [DataMember]
        public double SparkBonus { get; set; }
        [DataMember]
        public double EmberBonus { get; set; }

        [DataMember]
        public OverlayEffectVisibleAnimationTypeEnum DamageAnimation { get; set; }
        [DataMember]
        public string DamageAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.DamageAnimation); } set { } }
        [DataMember]
        public OverlayEffectVisibleAnimationTypeEnum NewBossAnimation { get; set; }
        [DataMember]
        public string NewBossAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.NewBossAnimation); } set { } }

        [DataMember]
        public uint CurrentBossUserID { get; set; }
        [DataMember]
        public int CurrentHealth { get; set; }
        [DataMember]
        public bool NewBoss { get; set; }
        [DataMember]
        public bool DamageTaken { get; set; }

        [DataMember]
        public CustomCommand NewStreamBossCommand { get; set; }

        [DataMember]
        public UserViewModel CurrentBoss { get; set; }

        private SemaphoreSlim HealthSemaphore = new SemaphoreSlim(1);

        private HashSet<uint> follows = new HashSet<uint>();
        private HashSet<uint> hosts = new HashSet<uint>();
        private HashSet<uint> subs = new HashSet<uint>();

        public OverlayStreamBoss() : base(StreamBossItemType, HTMLTemplate) { }

        public OverlayStreamBoss(string htmlText, int startingHealth, int width, int height, string textColor, string textFont, string borderColor, string backgroundColor,
            string progressColor, double followBonus, double hostBonus, double subscriberBonus, double donationBonus, double sparkBonus, double emberBonus,
            OverlayEffectVisibleAnimationTypeEnum damageAnimation, OverlayEffectVisibleAnimationTypeEnum newBossAnimation, CustomCommand newStreamBossCommand)
            : base(StreamBossItemType, htmlText)
        {
            this.StartingHealth = startingHealth;
            this.Width = width;
            this.Height = height;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.ProgressColor = progressColor;
            this.FollowBonus = followBonus;
            this.HostBonus = hostBonus;
            this.SubscriberBonus = subscriberBonus;
            this.DonationBonus = donationBonus;
            this.SparkBonus = sparkBonus;
            this.EmberBonus = emberBonus;
            this.DamageAnimation = damageAnimation;
            this.NewBossAnimation = newBossAnimation;
            this.NewStreamBossCommand = newStreamBossCommand;
        }

        public override async Task Initialize()
        {
            if (this.CurrentBossUserID > 0)
            {
                UserModel user = await ChannelSession.MixerUserConnection.GetUser(this.CurrentBossUserID);
                if (user != null)
                {
                    //this.CurrentBoss = new UserViewModel(user);
                }
                else
                {
                    this.CurrentBossUserID = 0;
                }
            }

            if (this.CurrentBossUserID == 0)
            {
                this.CurrentBoss = await ChannelSession.GetCurrentUser();
                this.CurrentHealth = this.StartingHealth;
            }
            this.CurrentBossUserID = this.CurrentBoss.MixerID;

            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;

            if (this.FollowBonus > 0.0)
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.HostBonus > 0.0)
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.SubscriberBonus > 0.0)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.DonationBonus > 0.0)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.SparkBonus > 0.0)
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.EmberBonus > 0.0)
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }

            await base.Initialize();
        }

        public override OverlayCustomHTMLItem GetCopy()
        {
            OverlayStreamBoss copy = this.Copy<OverlayStreamBoss>();
            copy.NewStreamBossCommand = null;
            return copy;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.DamageTaken)
            {
                if (this.NewBoss && this.NewStreamBossCommand != null)
                {
                    await this.NewStreamBossCommand.Perform();
                }

                OverlayItemBase copy = await base.GetProcessedItem(this.CurrentBoss, arguments, extraSpecialIdentifiers);
                this.DamageTaken = false;
                this.NewBoss = false;
                return copy;
            }
            return await base.GetProcessedItem(this.CurrentBoss, arguments, extraSpecialIdentifiers);
        }

        protected override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            UserViewModel boss = null;
            int health = 0;

            await this.HealthSemaphore.WaitAndRelease(() =>
            {
                boss = this.CurrentBoss;
                health = this.CurrentHealth;
                return Task.FromResult(0);
            });

            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_SIZE"] = ((int)(0.2 * ((double)this.Height))).ToString();

            replacementSets["USERNAME"] = boss.Username;
            replacementSets["USER_IMAGE"] = boss.AvatarLink;
            replacementSets["USER_IMAGE_SIZE"] = ((int)(0.8 * ((double)this.Height))).ToString();

            replacementSets["HEALTH_REMAINING"] = health.ToString();
            replacementSets["MAXIMUM_HEALTH"] = this.StartingHealth.ToString();

            replacementSets["PROGRESS_COLOR"] = this.ProgressColor;
            replacementSets["PROGRESS_WIDTH"] = ((((double)health) / ((double)this.StartingHealth)) * 100.0).ToString();

            return replacementSets;
        }

        private async Task ReduceHealth(UserViewModel user, double amount)
        {
            await this.HealthSemaphore.WaitAndRelease(() =>
            {
                this.DamageTaken = true;
                if (this.CurrentBoss.Equals(user))
                {
                    this.CurrentHealth = Math.Min(this.CurrentHealth, this.CurrentHealth + (int)amount);
                }
                else
                {
                    this.CurrentHealth -= (int)amount;
                }

                if (this.CurrentHealth <= 0)
                {
                    this.CurrentBoss = user;
                    this.CurrentBossUserID = user.MixerID;
                    this.CurrentHealth = this.StartingHealth;
                    this.NewBoss = true;
                }
                return Task.FromResult(0);
            });
        }

        private async void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.MixerID))
            {
                this.follows.Add(user.MixerID);
                await this.ReduceHealth(user, this.FollowBonus);
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.MixerID))
            {
                this.hosts.Add(host.Item1.MixerID);
                await this.ReduceHealth(host.Item1, (Math.Max(host.Item2, 1) * this.HostBonus));
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.MixerID))
            {
                this.subs.Add(user.MixerID);
                await this.ReduceHealth(user, this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.MixerID))
            {
                this.subs.Add(user.Item1.MixerID);
                await this.ReduceHealth(user.Item1, this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { await this.ReduceHealth(donation.User, (donation.Amount * this.DonationBonus)); }

        private async void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> sparkUsage) { await this.ReduceHealth(sparkUsage.Item1, (sparkUsage.Item2 * this.SparkBonus)); }

        private async void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage) { await this.ReduceHealth(emberUsage.User, (emberUsage.Amount * this.EmberBonus)); }
    }

    [Obsolete]
    [DataContract]
    public class OverlayTextItem : OverlayItemBase
    {
        public const string TextItemType = "text";

        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public int Size { get; set; }
        [DataMember]
        public string Font { get; set; }
        [DataMember]
        public bool Bold { get; set; }
        [DataMember]
        public bool Underline { get; set; }
        [DataMember]
        public bool Italic { get; set; }
        [DataMember]
        public string ShadowColor { get; set; }

        public OverlayTextItem() : base(TextItemType) { }

        public OverlayTextItem(string text, string color, int size, string font, bool bold, bool italic, bool underline, string shadowColor)
            : base(TextItemType)
        {
            this.Text = text;
            this.Color = color;
            this.Size = size;
            this.Font = font;
            this.Bold = bold;
            this.Underline = underline;
            this.Italic = italic;
            this.ShadowColor = shadowColor;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayTextItem item = this.Copy<OverlayTextItem>();
            item.Text = await this.ReplaceStringWithSpecialModifiers(item.Text, user, arguments, extraSpecialIdentifiers);
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayTimer : OverlayCustomHTMLItem, IDisposable
    {
        public const string HTMLTemplate =
            @"<p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{TIME}</p>";

        public const string TimerItemType = "timer";

        public const string TimerCompleteCommandName = "On Timer Reached";

        [DataMember]
        public int TotalLength { get; set; }

        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public int TextSize { get; set; }

        [DataMember]
        public CustomCommand TimerCompleteCommand { get; set; }

        [JsonIgnore]
        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public OverlayTimer() : base(TimerItemType, HTMLTemplate) { }

        public OverlayTimer(string htmlText, int totalLength, string textColor, string textFont, int textSize, CustomCommand timerCompleteCommand)
            : base(TimerItemType, htmlText)
        {
            this.TotalLength = totalLength;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
            this.TimerCompleteCommand = timerCompleteCommand;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.TimerBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public override async Task Disable()
        {
            if (this.backgroundThreadCancellationTokenSource != null)
            {
                this.backgroundThreadCancellationTokenSource.Cancel();
                this.backgroundThreadCancellationTokenSource = null;
            }
            await base.Disable();
        }

        public override OverlayCustomHTMLItem GetCopy()
        {
            OverlayTimer copy = this.Copy<OverlayTimer>();
            copy.TimerCompleteCommand = null;
            return copy;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
        }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();

            return Task.FromResult(replacementSets);
        }

        private async Task TimerBackground()
        {
            try
            {
                await Task.Delay(this.TotalLength * 1000);

                if (this.IsInitialized && !this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (this.TimerCompleteCommand != null)
                    {
                        await this.TimerCompleteCommand.Perform();
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.backgroundThreadCancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }

    [Obsolete]
    [DataContract]
    public class OverlayTimerTrain : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
            @"<p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{TIME}</p>";

        public const string TimerTrainItemType = "timertrain";

        [DataMember]
        public int MinimumSecondsToShow { get; set; }
        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public int TextSize { get; set; }

        [DataMember]
        public double FollowBonus { get; set; }
        [DataMember]
        public double HostBonus { get; set; }
        [DataMember]
        public double SubscriberBonus { get; set; }
        [DataMember]
        public double DonationBonus { get; set; }
        [DataMember]
        public double SparkBonus { get; set; }
        [DataMember]
        public double EmberBonus { get; set; }

        [DataMember]
        public double SecondsToAdd { get; set; }

        private HashSet<uint> follows = new HashSet<uint>();
        private HashSet<uint> hosts = new HashSet<uint>();
        private HashSet<uint> subs = new HashSet<uint>();

        public OverlayTimerTrain() : base(TimerTrainItemType, HTMLTemplate) { }

        public OverlayTimerTrain(string htmlText, int minimumSecondsToShow, string textColor, string textFont, int textSize, double followBonus,
            double hostBonus, double subscriberBonus, double donationBonus, double sparkBonus, double emberBonus)
            : base(TimerTrainItemType, htmlText)
        {
            this.MinimumSecondsToShow = minimumSecondsToShow;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
            this.FollowBonus = followBonus;
            this.HostBonus = hostBonus;
            this.SubscriberBonus = subscriberBonus;
            this.DonationBonus = donationBonus;
            this.SparkBonus = sparkBonus;
            this.EmberBonus = emberBonus;
        }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            this.SecondsToAdd = (double)this.MinimumSecondsToShow * 1.5;
            await Task.Delay((int)this.SecondsToAdd * 1000);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;

            if (this.FollowBonus > 0.0)
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.HostBonus > 0.0)
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.SubscriberBonus > 0.0)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.DonationBonus > 0.0)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.SparkBonus > 0.0)
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.EmberBonus > 0.0)
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }

            await base.Initialize();
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayTimerTrain>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.SecondsToAdd >= this.MinimumSecondsToShow)
            {
                OverlayTimerTrain copy = (OverlayTimerTrain)await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
                this.SecondsToAdd = 0;
                return copy;
            }
            return null;
        }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.MixerID))
            {
                this.follows.Add(user.MixerID);
                this.SecondsToAdd += this.FollowBonus;
            }
        }

        private void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.MixerID))
            {
                this.hosts.Add(host.Item1.MixerID);
                this.SecondsToAdd += (Math.Max(host.Item2, 1) * this.HostBonus);
            }
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.MixerID))
            {
                this.subs.Add(user.MixerID);
                this.SecondsToAdd += this.SubscriberBonus;
            }
        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.MixerID))
            {
                this.subs.Add(user.Item1.MixerID);
                this.SecondsToAdd += this.SubscriberBonus;
            }
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.SecondsToAdd += (donation.Amount * this.DonationBonus); }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> sparkUsage) { this.SecondsToAdd += (sparkUsage.Item2 * this.SparkBonus); }

        private void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage) { this.SecondsToAdd += (emberUsage.Amount * this.EmberBonus); }
    }

    [Obsolete]
    [DataContract]
    public class OverlayVideoItem : OverlayItemBase
    {
        public const int DefaultHeight = 315;
        public const int DefaultWidth = 560;

        public const string VideoItemType = "video";

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public string FileID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}", this.FileID);
                }
                return this.FilePath;
            }
            set { }
        }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlayVideoItem() : base(VideoItemType) { this.Volume = 100; }

        public OverlayVideoItem(string filepath, int width, int height, int volume)
            : base(VideoItemType)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.FileID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayVideoItem item = this.Copy<OverlayVideoItem>();
            item.FilePath = await this.ReplaceStringWithSpecialModifiers(item.FilePath, user, arguments, extraSpecialIdentifiers);
            if (!Uri.IsWellFormedUriString(item.FilePath, UriKind.RelativeOrAbsolute))
            {
                item.FilePath = item.FilePath.ToFilePathString();
            }
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayWebPageItem : OverlayItemBase
    {
        public const string WebPageItemType = "webpage";

        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayWebPageItem() : base(WebPageItemType) { }

        public OverlayWebPageItem(string url, int width, int height)
            : base(WebPageItemType)
        {
            this.URL = url;
            this.Width = width;
            this.Height = height;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayWebPageItem item = this.Copy<OverlayWebPageItem>();
            item.URL = await this.ReplaceStringWithSpecialModifiers(item.URL, user, arguments, extraSpecialIdentifiers, encode: true);
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayYouTubeItem : OverlayItemBase
    {
        private const string YouTubeItemType = "youtube";

        [DataMember]
        public string VideoID { get; set; }
        [DataMember]
        public int StartTime { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        public OverlayYouTubeItem() : base(YouTubeItemType) { this.Volume = 100; }

        public OverlayYouTubeItem(string id, int startTime, int width, int height, int volume)
            : base(YouTubeItemType)
        {
            this.VideoID = id;
            this.StartTime = startTime;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayYouTubeItem item = this.Copy<OverlayYouTubeItem>();
            item.VideoID = await this.ReplaceStringWithSpecialModifiers(item.VideoID, user, arguments, extraSpecialIdentifiers, encode: true);
            return item;
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlayWidget
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string OverlayName { get; set; }

        [DataMember]
        public OverlayItemBase Item { get; set; }

        [DataMember]
        public OverlayItemPosition Position { get; set; }

        [DataMember]
        public bool DontRefresh { get; set; }

        public OverlayWidget()
        {
            this.IsEnabled = true;
        }

        public OverlayWidget(string name, string overlayName, OverlayItemBase item, OverlayItemPosition position, bool dontRefresh)
            : this()
        {
            this.Name = name;
            this.OverlayName = overlayName;
            this.Item = item;
            this.Position = position;
            this.DontRefresh = dontRefresh;
        }

        [JsonIgnore]
        public virtual bool SupportsTestButton { get { return (this.Item != null) ? this.Item.SupportsTestButton : false; } }

        public async Task LoadTestData()
        {
            if (this.SupportsTestButton)
            {
                await this.Item.LoadTestData();
            }
        }
    }

    [Obsolete]
    [DataContract]
    public class OverlaySongRequestsListItemModel : OverlayListItemModelBase
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
            <img src=""{SONG_IMAGE}"" width=""{SONG_IMAGE_SIZE}"" height=""{SONG_IMAGE_SIZE}"" style=""position: absolute; top: 50%; transform: translate(0%, -50%); margin-left: 10px;"">
            <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; position: absolute; top: 50%; left: 28%; transform: translate(0%, -50%);"">{SONG_NAME}</span>
        </div>";

        public bool IncludeCurrentSong { get; set; }

        public OverlaySongRequestsListItemModel()
            : base()
        {
            this.IncludeCurrentSong = true;
        }

        public OverlaySongRequestsListItemModel(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            bool includeCurrentSong, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.SongRequests, htmlText, totalToShow, 0, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation)
        {
            this.IncludeCurrentSong = includeCurrentSong;
        }
    }
}
