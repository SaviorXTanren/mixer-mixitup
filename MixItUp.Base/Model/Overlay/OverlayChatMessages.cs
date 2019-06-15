using Mixer.Base.Model.Chat;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayChatMessage
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

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
            for (int i = 0; i < 5; i++)
            {
                ChatMessageEventModel messageEvent = new ChatMessageEventModel()
                {
                    id = Guid.NewGuid(),
                    user_id = ChannelSession.User.id,
                    user_name = ChannelSession.User.username,
                    channel = ChannelSession.Channel.id,
                    message = new ChatMessageContentsModel() { message = new ChatMessageDataModel[] { new ChatMessageDataModel() { type = "text", text = "Test Message" } } }
                };
                this.GlobalEvents_OnChatMessageReceived(this, new ChatMessageViewModel(messageEvent));

                await Task.Delay(1000);
            }
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
                                ID = this.messagesToProcess.ElementAt(0).ID,
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
            ChatMessageViewModel message = this.messagesToProcess.First();
            if (message.Skill != null || message.ChatSkill != null)
            {
                text = text.Replace("{MESSAGE}", OverlayChatMessages.SkillImageMessageHTMLTemplate);
            }
            else
            {
                text = text.Replace("{MESSAGE}", OverlayChatMessages.TextMessageHTMLTemplate);
            }

            foreach (var kvp in await this.GetReplacementSets(user, arguments, extraSpecialIdentifiers))
            {
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
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
            replacementSets["USERNAME"] = message.User.UserName;
            replacementSets["USER_COLOR"] = OverlayChatMessages.userColors[message.User.PrimaryRoleColorName];

            replacementSets["SUB_IMAGE"] = "";
            if (message.User.IsMixerSubscriber && ChannelSession.Channel.badge != null)
            {
                replacementSets["SUB_IMAGE"] = ChannelSession.Channel.badge.url;
            }

            if (message.Skill != null)
            {
                replacementSets["IMAGE"] = message.Skill.ImageUrl;
            }
            else if (message.ChatSkill != null)
            {
                replacementSets["IMAGE"] = message.ChatSkill.icon_url;
            }
            else
            {
                StringBuilder text = new StringBuilder();
                foreach (ChatMessageDataModel messageData in message.MessageComponents)
                {
                    EmoticonImage emoticon = ChannelSession.GetEmoticonForMessage(messageData);
                    if (emoticon != null)
                    {
                        string emoticonText = OverlayChatMessages.EmoticonMessageHTMLTemplate;
                        emoticonText = emoticonText.Replace("{EMOTICON}", emoticon.Uri);
                        emoticonText = emoticonText.Replace("{TEXT_SIZE}", this.TextSize.ToString());
                        emoticonText = emoticonText.Replace("{EMOTICON_SIZE}", emoticon.Width.ToString());
                        emoticonText = emoticonText.Replace("{EMOTICON_X}", (-emoticon.X).ToString());
                        emoticonText = emoticonText.Replace("{EMOTICON_Y}", (-emoticon.Y).ToString());
                        text.Append(emoticonText + " ");
                    }
                    else
                    {
                        text.Append(messageData.text + " ");
                    }
                }
                replacementSets["TEXT"] = text.ToString().Trim();
            }

            return Task.FromResult(replacementSets);
        }

        private async void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (!message.IsAlert && !message.IsWhisper)
            {
                await this.semaphore.WaitAndRelease(() =>
                {
                    this.allMessages.Add(message);
                    return Task.FromResult(0);
                });
            }
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
}
