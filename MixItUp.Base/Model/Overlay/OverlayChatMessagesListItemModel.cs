using Mixer.Base.Model.Chat;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayChatMessagesListItemModel : OverlayListItemModelBase
    {
        [DataContract]
        public class OverlayChatMessageItemModel
        {
            [DataMember]
            public Guid ID { get; set; }

            [DataMember]
            public string Username { get; set; }

            [DataMember]
            public string UserAvatar { get; set; }

            [DataMember]
            public string UserColor { get; set; }

            [DataMember]
            public string SubBadgeImage { get; set; }

            [DataMember]
            public string Image { get; set; }

            [DataMember]
            public string Message { get; set; }

            public OverlayChatMessageItemModel() { }

            public OverlayChatMessageItemModel(Guid id, UserViewModel user, string image, string message)
            {
                this.ID = id;
                this.Username = user.UserName;
                this.UserAvatar = user.AvatarLink;
                this.UserColor = OverlayChatMessagesListItemModel.userColors[user.PrimaryRoleColorName];
                if (user.IsMixerSubscriber && ChannelSession.Channel.badge != null)
                {
                    this.SubBadgeImage = ChannelSession.Channel.badge.url;
                }
                this.Image = image;
                this.Message = message;
            }
        }

        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 50%; left: 5%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">#{POSITION} {USERNAME}</p>
        </div>";

        private static readonly Dictionary<string, string> userColors = new Dictionary<string, string>()
        {
            { "UserStreamerRoleColor", "#FFFFFF" },
            { "UserStaffRoleColor", "#FFD700" },
            { "UserModRoleColor", "#008000" },
            { "UserGlobalModRoleColor", "#07FDC6" },
            { "UserProRoleColor", "#800080" },
            { "UserDefaultRoleColor", "#0000FF" },
        };

        private const string TextMessageHTMLTemplate = @"<span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_HEIGHT}px; font-weight: bold; word-wrap: break-word; color: {TEXT_COLOR}; vertical-align: middle; margin-left: 10px;"">{TEXT}</span>";
        private const string EmoticonMessageHTMLTemplate = @"<span role=""img"" style=""height: {TEXT_HEIGHT}px; width: {TEXT_HEIGHT}px; background-repeat: no-repeat; display: inline-block; background-image: url({EMOTICON}); background-position: {EMOTICON_X}px {EMOTICON_Y}px;""></span>";
        private const string SkillImageMessageHTMLTemplate = @"<img src=""{IMAGE}"" style=""vertical-align: middle; margin-left: 10px; max-height: 80px;""></img>";

        [DataMember]
        public List<OverlayChatMessageItemModel> MessagesToAdd { get; set; } = new List<OverlayChatMessageItemModel>();

        [DataMember]
        public List<Guid> MessagesToDelete { get; set; } = new List<Guid>();

        public OverlayChatMessagesListItemModel() : base() { }

        public OverlayChatMessagesListItemModel(string htmlText, int totalToShow, string textFont, int width, int height,
            string borderColor, string backgroundColor, string textColor, OverlayEffectEntranceAnimationTypeEnum addEventAnimation, OverlayEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.ChatMessages, htmlText, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        { }

        public override async Task LoadTestData()
        {
            UserViewModel user = await ChannelSession.GetCurrentUser();
            string message = TextMessageHTMLTemplate.Replace("{TEXT}", "TEST MESSAGE");

            this.MessagesToAdd.Clear();
            for (int i = 0; i < this.TotalToShow; i++)
            {
                this.MessagesToAdd.Add(new OverlayChatMessageItemModel(Guid.NewGuid(), user, string.Empty, message));
            }
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;
            GlobalEvents.OnChatMessageDeleted += GlobalEvents_OnChatMessageDeleted;

            this.MessagesToAdd.Clear();
            this.MessagesToDelete.Clear();

            await base.Initialize();
        }

        private void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (!message.IsAlert && !message.IsWhisper)
            {
                string image = string.Empty;
                string text = string.Empty;
                if (message.Skill != null)
                {
                    image = message.Skill.ImageUrl;
                }
                else if (message.ChatSkill != null)
                {
                    image = message.ChatSkill.icon_url;
                }
                else
                {
                    StringBuilder messageTextBuilder = new StringBuilder();
                    foreach (ChatMessageDataModel messageData in message.MessageComponents)
                    {
                        EmoticonImage emoticon = ChannelSession.GetEmoticonForMessage(messageData);
                        if (emoticon != null)
                        {
                            string emoticonText = OverlayChatMessagesListItemModel.EmoticonMessageHTMLTemplate;
                            emoticonText = emoticonText.Replace("{EMOTICON}", emoticon.Uri);
                            emoticonText = emoticonText.Replace("{EMOTICON_X}", (-emoticon.X).ToString());
                            emoticonText = emoticonText.Replace("{EMOTICON_Y}", (-emoticon.Y).ToString());
                            messageTextBuilder.Append(emoticonText + " ");
                        }
                        else
                        {
                            messageTextBuilder.Append(messageData.text + " ");
                        }
                    }
                    text = messageTextBuilder.ToString().Trim();
                }

                this.MessagesToAdd.Add(new OverlayChatMessageItemModel(message.ID, message.User, image, text));
                this.SendUpdateRequired();
                this.MessagesToAdd.Clear();
            }
        }

        private void GlobalEvents_OnChatMessageDeleted(object sender, Guid id)
        {
            this.MessagesToDelete.Add(id);
            this.SendUpdateRequired();
            this.MessagesToDelete.Clear();
        }
    }
}
