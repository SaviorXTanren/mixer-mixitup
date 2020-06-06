using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
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
        public Guid StreamPassID { get; set; }

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
        public UserRoleEnum RoleRequirement { get; set; }

        [DataMember]
        [Obsolete]
        public string ChatText { get; set; }
        [DataMember]
        [Obsolete]
        public bool IsWhisper { get; set; }
        [DataMember]
        [Obsolete]
        public bool GiveToAllUsers { get; set; }

        public CurrencyAction() : base(ActionTypeEnum.Currency) { this.RoleRequirement = UserRoleEnum.User; }

        public CurrencyAction(CurrencyModel currency, CurrencyActionTypeEnum currencyActionType, string amount, string username = null,
            UserRoleEnum roleRequirement = UserRoleEnum.User, bool deductFromUser = false)
            : this()
        {
            this.CurrencyID = currency.ID;
            this.CurrencyActionType = currencyActionType;
            this.Amount = amount;
            this.Username = username;
            this.RoleRequirement = roleRequirement;
            this.DeductFromUser = deductFromUser;
        }

        public CurrencyAction(UserInventoryModel inventory, CurrencyActionTypeEnum currencyActionType, string itemName = null, string amount = null, string username = null,
            UserRoleEnum roleRequirement = UserRoleEnum.User, bool deductFromUser = false)
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

        public CurrencyAction(StreamPassModel streamPass, CurrencyActionTypeEnum currencyActionType, string amount, string username = null,
            UserRoleEnum roleRequirement = UserRoleEnum.User, bool deductFromUser = false)
            : this()
        {
            this.StreamPassID = streamPass.ID;
            this.CurrencyActionType = currencyActionType;
            this.Amount = amount;
            this.Username = username;
            this.RoleRequirement = roleRequirement;
            this.DeductFromUser = deductFromUser;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.Chat != null)
            {
                CurrencyModel currency = null;
                UserInventoryModel inventory = null;
                StreamPassModel streamPass = null;
                string systemName = null;
                UserInventoryItemModel item = null;

                if (this.CurrencyID != Guid.Empty)
                {
                    if (!ChannelSession.Settings.Currency.ContainsKey(this.CurrencyID))
                    {
                        return;
                    }
                    currency = ChannelSession.Settings.Currency[this.CurrencyID];
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
                        string itemName = await this.ReplaceStringWithSpecialModifiers(this.ItemName, user, arguments);
                        item = inventory.GetItem(itemName);
                        if (item == null)
                        {
                            return;
                        }
                    }
                }

                if (this.StreamPassID != Guid.Empty)
                {
                    if (!ChannelSession.Settings.StreamPass.ContainsKey(this.StreamPassID))
                    {
                        return;
                    }
                    streamPass = ChannelSession.Settings.StreamPass[this.StreamPassID];
                    systemName = streamPass.Name;
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
                    else if (streamPass != null)
                    {
                        await streamPass.Reset();
                    }
                }
                else if (this.CurrencyActionType == CurrencyActionTypeEnum.ResetForUser)
                {
                    if (currency != null)
                    {
                        currency.ResetAmount(user.Data);
                    }
                    else if (inventory != null)
                    {
                        inventory.ResetAmount(user.Data);
                    }
                    else if (streamPass != null)
                    {
                        streamPass.ResetAmount(user.Data);
                    }
                }
                else
                {
                    string amountTextValue = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, arguments);
                    if (!double.TryParse(amountTextValue, out double doubleAmount))
                    {
                        await ChannelSession.Services.Chat.Whisper(user, string.Format("{0} is not a valid amount of {1}", amountTextValue, systemName));
                        return;
                    }

                    int amountValue = (int)Math.Ceiling(doubleAmount);
                    if (amountValue <= 0)
                    {
                        await ChannelSession.Services.Chat.Whisper(user, "The amount specified must be greater than 0");
                        return;
                    }

                    HashSet<UserDataModel> receiverUserData = new HashSet<UserDataModel>();
                    if (this.CurrencyActionType == CurrencyActionTypeEnum.AddToUser)
                    {
                        receiverUserData.Add(user.Data);
                    }
                    else if (this.CurrencyActionType == CurrencyActionTypeEnum.AddToSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser)
                    {
                        if (!string.IsNullOrEmpty(this.Username))
                        {
                            string usernameString = await this.ReplaceStringWithSpecialModifiers(this.Username, user, arguments);
                            UserViewModel receivingUser = ChannelSession.Services.User.GetUserByUsername(usernameString, this.platform);
                            if (receivingUser != null)
                            {
                                receiverUserData.Add(receivingUser.Data);
                            }
                            else
                            {
                                await ChannelSession.Services.Chat.Whisper(user, "The user could not be found");
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
                        receiverUserData.Add(ChannelSession.GetCurrentUser().Data);
                    }

                    if ((this.DeductFromUser && receiverUserData.Count > 0) || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromUser)
                    {
                        if (currency != null)
                        {
                            if (!currency.HasAmount(user.Data, amountValue))
                            {
                                await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} {1} to do this", amountValue, systemName));
                                return;
                            }
                            currency.SubtractAmount(user.Data, amountValue);
                        }
                        else if (inventory != null)
                        {
                            if (!inventory.HasAmount(user.Data, item, amountValue))
                            {
                                await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} {1} to do this", amountValue, item));
                                return;
                            }
                            inventory.SubtractAmount(user.Data, item, amountValue);
                        }
                        else if (streamPass != null)
                        {
                            if (!streamPass.HasAmount(user.Data, amountValue))
                            {
                                await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} {1} to do this", amountValue, systemName));
                                return;
                            }
                            streamPass.SubtractAmount(user.Data, amountValue);
                        }
                    }

                    if (receiverUserData.Count > 0)
                    {
                        foreach (UserDataModel receiverUser in receiverUserData)
                        {
                            if (currency != null)
                            {
                                if (this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                                {
                                    currency.SubtractAmount(receiverUser, amountValue);
                                }
                                else
                                {
                                    currency.AddAmount(receiverUser, amountValue);
                                }
                            }
                            else if (inventory != null)
                            {
                                if (this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                                {
                                    inventory.SubtractAmount(receiverUser, item, amountValue);
                                }
                                else
                                {
                                    inventory.AddAmount(receiverUser, item, amountValue);
                                }
                            }
                            else if (streamPass != null)
                            {
                                if (this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.CurrencyActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                                {
                                    streamPass.SubtractAmount(receiverUser, amountValue);
                                }
                                else
                                {
                                    streamPass.AddAmount(receiverUser, amountValue);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
