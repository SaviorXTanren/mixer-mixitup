using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    [DataContract]
    public class OverlayChatMessagesListItemModel : OverlayListItemModelBase
    {
        public const string HTMLTemplate =
        @"<div style=""border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px;"">
          <p style=""padding: 10px; margin: auto;"">
            <img src=""{USER_IMAGE}"" width=""{TEXT_SIZE}"" height=""{TEXT_SIZE}"" style=""vertical-align: middle; padding-right: 2px"">
            <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; word-wrap: break-word; color: {USER_COLOR}; vertical-align: middle;"">{USERNAME}</span>
            <img src=""{USER_SUB_IMAGE}"" style=""vertical-align: middle; padding-right: 5px"" onerror=""this.style.display='none'"">
            {MESSAGE}
          </p>
        </div>";

        private const string TextMessageHTMLTemplate = @"<span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; word-wrap: break-word; color: {TEXT_COLOR}; vertical-align: middle; margin-left: 10px;"">{TEXT}</span>";
        private const string ImageMessageHTMLTemplate = @"<img src=""{IMAGE}"" style=""vertical-align: middle; margin-left: 10px; width: auto; height: {TEXT_SIZE}px;""></img>";

        public OverlayChatMessagesListItemModel() : base() { }

        public OverlayChatMessagesListItemModel(string htmlText, int totalToShow, int fadeOut, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.ChatMessages, htmlText, totalToShow, fadeOut, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation)
        { }

        public override Task LoadTestData()
        {
            UserChatMessageViewModel message = new UserChatMessageViewModel(Guid.NewGuid().ToString(), StreamingPlatformTypeEnum.None, ChannelSession.User);
            message.AddStringMessagePart("Test Message");
            this.GlobalEvents_OnChatMessageReceived(this, message);
            return Task.CompletedTask;
        }

        public override async Task Enable()
        {
            ChatService.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted += GlobalEvents_OnChatMessageDeleted;

            await base.Enable();
        }

        public override async Task Disable()
        {
            ChatService.OnChatMessageReceived -= GlobalEvents_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted -= GlobalEvents_OnChatMessageDeleted;

            await base.Disable();
        }

        private async void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (!message.IsWhisper)
            {
                if (message is UserChatMessageViewModel)
                {
                    OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(message.ID.ToString(), message.User, -1, this.HTML);

                    List<string> textParts = new List<string>();
                    foreach (object messagePart in message.MessageParts)
                    {
                        string imageURL = null;
                        if (messagePart is string)
                        {
                            textParts.Add(HttpUtility.HtmlEncode((string)messagePart));
                        }
                        else if (messagePart is ChatEmoteViewModelBase)
                        {
                            imageURL = ((ChatEmoteViewModelBase)messagePart).ImageURL;
                        }

                        if (!string.IsNullOrEmpty(imageURL))
                        {
                            string imageText = OverlayChatMessagesListItemModel.ImageMessageHTMLTemplate;
                            imageText = imageText.Replace("{IMAGE}", imageURL);
                            textParts.Add(imageText);
                        }
                    }

                    UserV2ViewModel user = await item.GetUser();
                    if (user != null)
                    {
                        item.TemplateReplacements.Add("MESSAGE", OverlayChatMessagesListItemModel.TextMessageHTMLTemplate);
                        item.TemplateReplacements.Add("TEXT", string.Join(" ", textParts));
                        item.TemplateReplacements.Add("USERNAME", user.DisplayName);
                        item.TemplateReplacements.Add("USER_IMAGE", user.AvatarLink);
                        item.TemplateReplacements.Add("USER_COLOR", user.Color);
                        item.TemplateReplacements.Add("SUB_IMAGE", user.PlatformSubscriberBadgeLink);
                        item.TemplateReplacements.Add("USER_SUB_IMAGE", user.PlatformSubscriberBadgeLink);
                        item.TemplateReplacements.Add("TEXT_SIZE", this.Height.ToString());
                    }

                    await this.listSemaphore.WaitAsync();

                    this.Items.Add(item);
                    this.SendUpdateRequired();

                    this.listSemaphore.Release();
                }
            }
        }

        private async void GlobalEvents_OnChatMessageDeleted(object sender, string id)
        {
            await this.listSemaphore.WaitAsync();

            OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateRemoveItem(id);
            this.Items.Add(item);
            this.SendUpdateRequired();

            this.listSemaphore.Release();
        }
    }
}
