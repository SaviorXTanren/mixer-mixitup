using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class CurrencyAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CurrencyAction.asyncSemaphore; } }

        [DataMember]
        public string CurrencyName { get; set; }

        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }

        public CurrencyAction() : base(ActionTypeEnum.Currency) { }

        public CurrencyAction(int amount, string chatText, bool isWhisper)
            : this()
        {
            this.Amount = amount;
            this.ChatText = chatText;
            this.IsWhisper = isWhisper;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                UserCurrencyDataViewModel currencyData = user.Data.GetCurrency(this.CurrencyName);
                if (currencyData != null)
                {
                    currencyData.Amount += this.Amount;
                    if (!string.IsNullOrEmpty(this.ChatText))
                    {
                        string message = await this.ReplaceStringWithSpecialModifiers(this.ChatText, user, arguments);
                        if (this.IsWhisper)
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, message);
                        }
                        else
                        {
                            await ChannelSession.Chat.SendMessage(message);
                        }
                    }
                }
            }
        }
    }
}
