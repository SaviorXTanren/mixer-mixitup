using Mixer.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System;
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
        public Guid CurrencyID { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }

        public CurrencyAction() : base(ActionTypeEnum.Currency) { }

        public CurrencyAction(UserCurrencyViewModel currency, string username, string amount, string chatText, bool isWhisper)
            : this()
        {
            this.CurrencyID = currency.ID;
            this.Username = username;
            this.Amount = amount;
            this.ChatText = chatText;
            this.IsWhisper = isWhisper;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                UserCurrencyDataViewModel currencyData = user.Data.GetCurrency(this.CurrencyID);
                if (!string.IsNullOrEmpty(this.Username))
                {
                    string usernameString = await this.ReplaceStringWithSpecialModifiers(this.Username, user, arguments);

                    UserModel receivingUser = await ChannelSession.Connection.GetUser(usernameString);
                    if (receivingUser != null)
                    {
                        UserDataViewModel userData = ChannelSession.Settings.UserData.GetValueIfExists(receivingUser.id, new UserDataViewModel(new UserViewModel(receivingUser)));
                        currencyData = userData.GetCurrency(this.CurrencyID);
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "The user could not be found");
                        return;
                    }
                }

                if (currencyData != null)
                {
                    string amountTextValue = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, arguments);
                    if (int.TryParse(amountTextValue, out int amountValue))
                    {
                        currencyData.Amount += amountValue;
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
}
