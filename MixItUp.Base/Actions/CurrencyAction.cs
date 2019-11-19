using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum CurrencyActionTypeEnum
    {
        AddToUser,
        SubtractFromUser,

        AddToSpecificUser,
        AddToAllChatUsers,

        SubtractFromSpecificUser,
        SubtractFromAllChatUsers,

        ResetForAllUsers,
        ResetForUser
    }

    [DataContract]
    public class CurrencyAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CurrencyAction.asyncSemaphore; } }

        [DataMember]
        public Guid CurrencyID { get; set; }
        [DataMember]
        public Guid InventoryID { get; set; }

        [DataMember]
        public CurrencyActionTypeEnum CurrencyActionType { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string ItemName { get; set; }
        [DataMember]
        public string Amount { get; set; }
        [DataMember]
        public bool DeductFromUser { get; set; }

        [DataMember]
        public MixerRoleEnum RoleRequirement { get; set; }

        [DataMember]
        [Obsolete]
        public string ChatText { get; set; }
        [DataMember]
        [Obsolete]
        public bool IsWhisper { get; set; }
        [DataMember]
        [Obsolete]
        public bool GiveToAllUsers { get; set; }

        public CurrencyAction() : base(ActionTypeEnum.Currency) { this.RoleRequirement = MixerRoleEnum.User; }

        public CurrencyAction(UserCurrencyViewModel currency, CurrencyActionTypeEnum currencyActionType, string amount, string username = null,
            MixerRoleEnum roleRequirement = MixerRoleEnum.User, bool deductFromUser = false)
            : this()
        {
            this.CurrencyID = currency.ID;
            this.CurrencyActionType = currencyActionType;
            this.Amount = amount;
            this.Username = username;
            this.RoleRequirement = roleRequirement;
            this.DeductFromUser = deductFromUser;
        }

        public CurrencyAction(UserInventoryViewModel inventory, CurrencyActionTypeEnum currencyActionType, string itemName = null, string amount = null, string username = null,
            MixerRoleEnum roleRequirement = MixerRoleEnum.User, bool deductFromUser = false)
            : this()
        {
            this.InventoryID = inventory.ID;
            this.CurrencyActionType = currencyActionType;
            this.ItemName = itemName;
            this.Amount = amount;
            this.Username = username;
            this.RoleRequirement = roleRequirement;
            this.DeductFromUser = deductFromUser;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.Chat != null)
            {
                UserCurrencyViewModel currency = null;
                UserInventoryViewModel inventory = null;
                string systemName = null;
                string itemName = null;

                if (this.CurrencyID != Guid.Empty)
                {
                    if (!ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
                    {
                        return;
                    }
                    currency = ChannelSession.Settings.Currencies[this.CurrencyID];
                    systemName = currency.Name;
                }

                if (this.InventoryID != Guid.Empty)
                {
                    if (!ChannelSession.Settings.Inventories.ContainsKey(this.InventoryID))
                    {
                        return;
                    }
                    inventory = ChannelSession.Settings.Inventories[this.InventoryID];
                    systemName = inventory.Name;

                    if (!string.IsNullOrEmpty(this.ItemName))
                    {
                        itemName = await this.ReplaceStringWithSpecialModifiers(this.ItemName, user, arguments);
                        if (!inventory.Items.ContainsKey(itemName))
                        {
                            return;
                        }
                    }
                }

                if (this.CurrencyActionType == CurrencyActionTypeEnum.ResetForAllUsers)
                {
                    if (currency != null)
                    {
                        await currency.Reset();
                    }
                    else if (inventory != null)
                    {
                        await inventory.Reset();
                    }
                }
                else if (this.CurrencyActionType == CurrencyActionTypeEnum.ResetForUser)
                {
                    if (currency != null)
                    {
                        user.Data.ResetCurrencyAmount(currency);
                    }
                    else if (inventory != null)
                    {
                        user.Data.ResetInventoryAmount(inventory);
                    }
                }
                else
                {
                    string amountTextValue = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, arguments);
                    if (!double.TryParse(amountTextValue, out double doubleAmount))
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format("{0} is not a valid amount of {1}", amountTextValue, systemName));
                        return;
                    }

                    int amountValue = (int)Math.Ceiling(doubleAmount);
                    if (amountValue <= 0)
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "The amount specified must be greater than 0");
                        return;
                    }

                    HashSet<UserDataViewModel> receiverUserData = new HashSet<UserDataViewModel>();
                    if (this.CurrencyActionType == CurrencyActionTypeEnum.AddToUser)
                    {
                        receiverUserData.Add(user.Data);
                    }
                    else if (this.CurrencyActionType == CurrencyActionTypeEnum.AddToSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser)
                    {
                        if (!string.IsNullOrEmpty(this.Username))
                        {
                            string usernameString = await this.ReplaceStringWithSpecialModifiers(this.Username, user, arguments);

                            UserModel receivingUser = await ChannelSession.MixerStreamerConnection.GetUser(usernameString);
                            if (receivingUser != null)
                            {
                                receiverUserData.Add(ChannelSession.Settings.UserData.GetValueIfExists(receivingUser.id, new UserDataViewModel(new UserViewModel(receivingUser))));
                            }
                            else
                            {
                                await ChannelSession.Services.Chat.Whisper(user.UserName, "The user could not be found");
                                return;
                            }
                        }
                    }
                    else if (this.CurrencyActionType == CurrencyActionTypeEnum.AddToAllChatUsers || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                    {
                        foreach (UserViewModel chatUser in ChannelSession.Services.User.GetAllWorkableUsers())
                        {
                            if (chatUser.HasPermissionsTo(this.RoleRequirement))
                            {
                                receiverUserData.Add(chatUser.Data);
                            }
                        }
                        receiverUserData.Add((await ChannelSession.GetCurrentUser()).Data);
                    }

                    if ((this.DeductFromUser && receiverUserData.Count > 0) || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromUser)
                    {
                        if (currency != null)
                        {
                            if (!user.Data.HasCurrencyAmount(currency, amountValue))
                            {
                                await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to do this", amountValue, systemName));
                                return;
                            }
                            user.Data.SubtractCurrencyAmount(currency, amountValue);
                        }
                        else if (inventory != null)
                        {
                            if (!user.Data.HasInventoryAmount(inventory, itemName, amountValue))
                            {
                                await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to do this", amountValue, itemName));
                                return;
                            }
                            user.Data.SubtractInventoryAmount(inventory, itemName, amountValue);
                        }
                    }

                    if (receiverUserData.Count > 0)
                    {
                        foreach (UserDataViewModel receiverUser in receiverUserData)
                        {
                            if (currency != null)
                            {
                                if (this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                                {
                                    receiverUser.SubtractCurrencyAmount(currency, amountValue);
                                }
                                else
                                {
                                    receiverUser.AddCurrencyAmount(currency, amountValue);
                                }
                            }
                            else if (inventory != null)
                            {
                                if (this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                                {
                                    receiverUser.SubtractInventoryAmount(inventory, itemName, amountValue);
                                }
                                else
                                {
                                    receiverUser.AddInventoryAmount(inventory, itemName, amountValue);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
