using MixItUp.Base.Model.Twitch.Chat;
using MixItUp.Base.Model.Twitch.EventSub;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchChatEmoteViewModel : ChatEmoteViewModelBase
    {
        public TwitchChatEmoteViewModel(ChatEmoteModel emote)
        {
            this.ID = emote.id;
            this.Name = emote.name;

            this.ImageURL = emote.BuildImageURL(ChatEmoteModel.StaticFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale3Name);
            if (emote.HasAnimated)
            {
                this.IsAnimated = true;
                this.AnimatedImageURL = emote.BuildImageURL(ChatEmoteModel.AnimatedFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale3Name);
            }
        }

        public TwitchChatEmoteViewModel(string emoteID, string emoteCode)
        {
            this.ID = emoteID;
            this.Name = emoteCode;

            this.ImageURL = this.BuildV2EmoteURL(ChatEmoteModel.StaticFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale3Name);
        }

        public TwitchChatEmoteViewModel(string text, ChatMessageNotificationFragmentEmote emote)
        {
            this.ID = emote.id;
            this.Name = text;

            this.ImageURL = this.BuildV2EmoteURL("default", ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale3Name);
        }

        private string BuildV2EmoteURL(string type, string theme, string size) { return $"https://static-cdn.jtvnw.net/emoticons/v2/{this.ID}/{type}/{theme}/{size}"; }
    }
}
