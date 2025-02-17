using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.ViewModel.Chat.YouTube
{
    public enum YouTubeSuperChatTypeEnum
    {
        ChatMessage,
        Sticker
    }

    public class YouTubeSuperChatViewModel
    {
        public YouTubeSuperChatTypeEnum Type { get; set; }

        public UserV2ViewModel User { get; set; }

        public double Amount { get; set; }
        public string AmountDisplay { get; set; }

        public string CurrencyType { get; set; }

        public long Tier { get; set; }

        public string Message { get; set; }

        public YouTubeSuperChatViewModel(LiveChatSuperChatDetails superChat, UserV2ViewModel user)
        {
            this.Type = YouTubeSuperChatTypeEnum.ChatMessage;

            this.User = user;

            this.SetAmountFromMicros(superChat.AmountMicros);
            this.AmountDisplay = superChat.AmountDisplayString;
            this.CurrencyType = superChat.Currency;
            this.Tier = superChat.Tier.GetValueOrDefault();
            this.Message = superChat.UserComment;
        }

        public YouTubeSuperChatViewModel(LiveChatSuperStickerDetails sticker, UserV2ViewModel user)
        {
            this.Type = YouTubeSuperChatTypeEnum.Sticker;

            this.User = user;

            this.SetAmountFromMicros(sticker.AmountMicros);
            this.AmountDisplay = sticker.AmountDisplayString;
            this.CurrencyType = sticker.Currency;
            this.Tier = sticker.Tier.GetValueOrDefault();
            this.Message = sticker.SuperStickerMetadata.AltText;
        }

        public void SetCommandParameterData(CommandParametersModel parameters)
        {
            parameters.SetArguments(this.Message);

            parameters.SpecialIdentifiers["amountnumberdigits"] = ((int)this.Amount * 100).ToString();
            parameters.SpecialIdentifiers["amountnumber"] = this.Amount.ToString();
            parameters.SpecialIdentifiers["amount"] = this.AmountDisplay;
            parameters.SpecialIdentifiers["tier"] = this.Tier.ToString();
            parameters.SpecialIdentifiers["currencytype"] = this.CurrencyType;
            parameters.SpecialIdentifiers["message"] = this.Message;
        }

        private void SetAmountFromMicros(ulong? amount)
        {
            this.Amount = Math.Round((double)amount.GetValueOrDefault() / 1000000.0, 2);
        }
    }
}
