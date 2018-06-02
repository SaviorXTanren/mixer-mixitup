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
        public bool GiveToAllUsers { get; set; }

        [DataMember]
        public string Amount { get; set; }
        [DataMember]
        public bool DeductFromUser { get; set; }

        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }

        public CurrencyAction() : base(ActionTypeEnum.Currency) { }

        public CurrencyAction(UserCurrencyViewModel currency, string username, bool giveToAllUsers, string amount, bool deductFromUser, string chatText, bool isWhisper)
            : this()
        {
            this.CurrencyID = currency.ID;
            this.Username = username;
            this.GiveToAllUsers = giveToAllUsers;
            this.Amount = amount;
            this.DeductFromUser = deductFromUser;
            this.ChatText = chatText;
            this.IsWhisper = isWhisper;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                UserCurrencyDataViewModel senderCurrencyData = user.Data.GetCurrency(this.CurrencyID);
                List<UserCurrencyDataViewModel> receiverCurrencyDatas = new List<UserCurrencyDataViewModel>();
                if (!string.IsNullOrEmpty(this.Username))
                {
                    string usernameString = await this.ReplaceStringWithSpecialModifiers(this.Username, user, arguments);

                    UserModel receivingUser = await ChannelSession.Connection.GetUser(usernameString);
                    if (receivingUser != null)
                    {
                        UserDataViewModel userData = ChannelSession.Settings.UserData.GetValueIfExists(receivingUser.id, new UserDataViewModel(new UserViewModel(receivingUser)));
                        receiverCurrencyDatas.Add(userData.GetCurrency(this.CurrencyID));
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "The user could not be found");
                        return;
                    }
                }
                else if (this.GiveToAllUsers)
                {
                    foreach (UserViewModel chatUser in await ChannelSession.ChannelUsers.GetAllWorkableUsers())
                    {
                        receiverCurrencyDatas.Add(chatUser.Data.GetCurrency(this.CurrencyID));
                    }
                    receiverCurrencyDatas.Remove(senderCurrencyData);
                }
                else
                {
                    receiverCurrencyDatas.Add(senderCurrencyData);
                }

                if (receiverCurrencyDatas.Count > 0)
                {
                    string amountTextValue = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, arguments);
                    if (int.TryParse(amountTextValue, out int amountValue))
                    {
                        if (this.DeductFromUser)
                        {
                            if (senderCurrencyData.Amount < amountValue)
                            {
                                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to do this", amountValue, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                                return;
                            }
                            senderCurrencyData.Amount -= amountValue;
                        }

                        foreach (UserCurrencyDataViewModel receiverCurrencyData in receiverCurrencyDatas)
                        {
                            receiverCurrencyData.Amount += amountValue;
                        }

                        if (!string.IsNullOrEmpty(this.ChatText))
                        {
                            string message = await this.ReplaceStringWithSpecialModifiers(this.ChatText, user, arguments);
                            if (this.IsWhisper)
                            {
                                foreach (UserCurrencyDataViewModel receiverCurrencyData in receiverCurrencyDatas)
                                {
                                    await ChannelSession.Chat.Whisper(receiverCurrencyData.User.UserName, message);
                                }
                            }
                            else
                            {
                                await ChannelSession.Chat.SendMessage(message);
                            }
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, string.Format("{0} is not a valid amount of {1}", amountTextValue, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                        return;
                    }
                }
            }
        }
    }
}
