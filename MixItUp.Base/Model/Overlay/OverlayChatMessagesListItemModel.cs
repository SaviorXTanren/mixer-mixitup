using Mixer.Base.Model.Chat;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayChatMessagesListItemModel : OverlayListItemModelBase
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

        private static readonly Dictionary<string, string> userColors = new Dictionary<string, string>()
        {
            { "UserStreamerRoleColor", "#FFFFFF" },
            { "UserStaffRoleColor", "#FFD700" },
            { "UserModRoleColor", "#008000" },
            { "UserGlobalModRoleColor", "#07FDC6" },
            { "UserProRoleColor", "#800080" },
            { "UserDefaultRoleColor", "#0000FF" },
        };

        private const string TextMessageHTMLTemplate = @"<span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; word-wrap: break-word; color: {TEXT_COLOR}; vertical-align: middle; margin-left: 10px;"">{TEXT}</span>";
        private const string EmoticonMessageHTMLTemplate = @"<span style=""height: {TEXT_SIZE}px; width: {TEXT_SIZE}px; background-repeat: no-repeat; display: inline-block; background-image: url({EMOTICON}); background-position: {EMOTICON_X}px {EMOTICON_Y}px;""></span>";
        private const string SkillImageMessageHTMLTemplate = @"<img src=""{IMAGE}"" style=""vertical-align: middle; margin-left: 10px; max-height: 80px;""></img>";

        public OverlayChatMessagesListItemModel() : base() { }

        public OverlayChatMessagesListItemModel(string htmlText, int totalToShow, int fadeOut, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.ChatMessages, htmlText, totalToShow, fadeOut, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        { }

        public override Task LoadTestData()
        {
            ChatMessageEventModel messageEvent = new ChatMessageEventModel()
            {
                id = Guid.NewGuid(),
                user_id = ChannelSession.MixerStreamerUser.id,
                user_name = ChannelSession.MixerStreamerUser.username,
                channel = ChannelSession.MixerChannel.id,
                message = new ChatMessageContentsModel() { message = new ChatMessageDataModel[] { new ChatMessageDataModel() { type = "text", text = "Test Message" } } }
            };
            this.GlobalEvents_OnChatMessageReceived(this, new ChatMessageViewModel(messageEvent));
            return Task.FromResult(0);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;
            GlobalEvents.OnChatMessageDeleted += GlobalEvents_OnChatMessageDeleted;

            await base.Initialize();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnChatMessageReceived -= GlobalEvents_OnChatMessageReceived;
            GlobalEvents.OnChatMessageDeleted -= GlobalEvents_OnChatMessageDeleted;

            await base.Disable();
        }

        private async void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (!message.IsAlert && !message.IsWhisper)
            {
                OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(message.ID.ToString(), message.User, -1, this.HTML);

                string text = string.Empty;
                string messageTemplate = string.Empty;
                if (message.Skill != null || message.ChatSkill != null)
                {
                    item.TemplateReplacements.Add("MESSAGE", OverlayChatMessagesListItemModel.SkillImageMessageHTMLTemplate);
                    if (message.Skill != null)
                    {
                        item.TemplateReplacements.Add("IMAGE", message.Skill.ImageUrl);
                    }
                    else if (message.ChatSkill != null)
                    {
                        item.TemplateReplacements.Add("IMAGE", message.ChatSkill.icon_url);
                    }
                }
                else
                {
                    item.TemplateReplacements.Add("MESSAGE", OverlayChatMessagesListItemModel.TextMessageHTMLTemplate);

                    text = message.PlainTextMessage;
                    foreach (ChatMessageDataModel messageData in message.MessageComponents)
                    {
                        MixerChatEmoteModel emoticon = MixerChatEmoteModel.GetEmoteForMessageData(messageData);
                        if (emoticon != null)
                        {
                            string emoticonText = OverlayChatMessagesListItemModel.EmoticonMessageHTMLTemplate;
                            emoticonText = emoticonText.Replace("{EMOTICON}", emoticon.Uri);
                            emoticonText = emoticonText.Replace("{EMOTICON_X}", (-emoticon.X).ToString());
                            emoticonText = emoticonText.Replace("{EMOTICON_Y}", (-emoticon.Y).ToString());
                            text = text.Replace(messageData.text, emoticonText);
                        }
                    }
                    item.TemplateReplacements.Add("TEXT", text);
                }

                item.TemplateReplacements.Add("USERNAME", item.User.UserName);
                item.TemplateReplacements.Add("USER_IMAGE", item.User.AvatarLink);
                item.TemplateReplacements.Add("USER_COLOR", OverlayChatMessagesListItemModel.userColors[item.User.PrimaryRoleColorName]);
                item.TemplateReplacements.Add("SUB_IMAGE", (item.User.IsMixerSubscriber && ChannelSession.MixerChannel.badge != null) ? ChannelSession.MixerChannel.badge.url : string.Empty);
                item.TemplateReplacements.Add("TEXT_SIZE", this.Height.ToString());

                await this.listSemaphore.WaitAndRelease(() =>
                {
                    this.Items.Add(item);
                    this.SendUpdateRequired();
                    return Task.FromResult(0);
                });
            }
        }

        private async void GlobalEvents_OnChatMessageDeleted(object sender, Guid id)
        {
            await this.listSemaphore.WaitAndRelease(() =>
            {
                OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateRemoveItem(id.ToString());
                this.Items.Add(item);
                this.SendUpdateRequired();
                return Task.FromResult(0);
            });
        }
    }
}
