using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum CurrencyActionTypeEnum
    {
        [Name("Add to user")]
        AddToUser,
        [Name("Subtract from user")]
        SubtractFromUser,

        [Name("Add to specific user")]
        AddToSpecificUser,
        [Name("Add to all chat users")]
        AddToAllChatUsers,

        [Name("Subtract from specific user")]
        SubtractFromSpecificUser,
        [Name("Subtract from all chat users")]
        SubtractFromAllChatUsers,
    }

    [DataContract]
    public class CurrencyAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CurrencyAction.asyncSemaphore; } }

        [DataMember]
        public Guid CurrencyID { get; set; }

        [DataMember]
        public CurrencyActionTypeEnum CurrencyActionType { get; set; }

        [DataMember]
        public string Username { get; set; }
        [DataMember]
        [Obsolete]
        public bool GiveToAllUsers { get; set; }

        [DataMember]
        public string Amount { get; set; }
        [DataMember]
        public bool DeductFromUser { get; set; }

        [DataMember]
        [Obsolete]
        public string ChatText { get; set; }

        [DataMember]
        [Obsolete]
        public bool IsWhisper { get; set; }

        public CurrencyAction() : base(ActionTypeEnum.Currency) { }

        public CurrencyAction(UserCurrencyViewModel currency, CurrencyActionTypeEnum currencyActionType, string amount, string username = null, bool deductFromUser = false)
            : this()
        {
            this.CurrencyID = currency.ID;
            this.CurrencyActionType = currencyActionType;
            this.Amount = amount;
            this.Username = username;
            this.DeductFromUser = deductFromUser;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                string amountTextValue = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, arguments);
                if (!int.TryParse(amountTextValue, out int amountValue))
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("{0} is not a valid amount of {1}", amountTextValue, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                    return;
                }

                if (amountValue <= 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "The amount specified must be greater than 0");
                    return;
                }

                UserCurrencyDataViewModel senderCurrencyData = user.Data.GetCurrency(this.CurrencyID);
                HashSet<UserCurrencyDataViewModel> receiverCurrencyDatas = new HashSet<UserCurrencyDataViewModel>();
                if (this.CurrencyActionType == CurrencyActionTypeEnum.AddToUser)
                {
                    receiverCurrencyDatas.Add(senderCurrencyData);
                }
                else if (this.CurrencyActionType == CurrencyActionTypeEnum.AddToSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser)
                {
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
                }
                else if (this.CurrencyActionType == CurrencyActionTypeEnum.AddToAllChatUsers || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                {
                    foreach (UserViewModel chatUser in await ChannelSession.ActiveUsers.GetAllWorkableUsers())
                    {
                        receiverCurrencyDatas.Add(chatUser.Data.GetCurrency(this.CurrencyID));
                    }
                    receiverCurrencyDatas.Add(ChannelSession.GetCurrentUser().Data.GetCurrency(this.CurrencyID));
                }

                if ((this.DeductFromUser && receiverCurrencyDatas.Count > 0) || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromUser)
                {
                    if (this.CurrencyActionType != CurrencyActionTypeEnum.SubtractFromUser && senderCurrencyData.Amount < amountValue)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to do this", amountValue, ChannelSession.Settings.Currencies[this.CurrencyID].Name));
                        return;
                    }
                    senderCurrencyData.Amount = Math.Max(senderCurrencyData.Amount - amountValue, 0);
                }

                if (receiverCurrencyDatas.Count > 0)
                {
                    foreach (UserCurrencyDataViewModel receiverCurrencyData in receiverCurrencyDatas)
                    {
                        if (this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                        {
                            receiverCurrencyData.Amount = Math.Max(receiverCurrencyData.Amount - amountValue, 0);
                        }
                        else
                        {
                            receiverCurrencyData.Amount += amountValue;
                        }
                    }
                }
            }
        }
    }
}
