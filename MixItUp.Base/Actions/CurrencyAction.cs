using MixItUp.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class CurrencyAction : ActionBase
    {
        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }

        public CurrencyAction() { }

        public CurrencyAction(int amount, string chatText, bool isWhisper)
            : base(ActionTypeEnum.Currency)
        {
            this.Amount = amount;
            this.ChatText = chatText;
            this.IsWhisper = isWhisper;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.BotChatClient != null)
            {
                if (!ChannelSession.Settings.UserData.ContainsKey(user.ID))
                {
                    ChannelSession.Settings.UserData.Add(user.ID, new UserDataViewModel(user.ID, user.UserName));
                }
                ChannelSession.Settings.UserData[user.ID].CurrencyAmount += this.Amount;

                string message = await this.ReplaceStringWithSpecialModifiers(this.ChatText, user, arguments);
                if (this.IsWhisper)
                {
                    await ChannelSession.BotChatClient.Whisper(user.UserName, message);
                }
                else
                {
                    await ChannelSession.BotChatClient.SendMessage(message);
                }
            }
        }
    }
}
