using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
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
    public class CurrencyActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CurrencyActionModel.asyncSemaphore; } }

        [DataMember]
        public Guid CurrencyID { get; set; }
        [DataMember]
        public Guid InventoryID { get; set; }
        [DataMember]
        public Guid StreamPassID { get; set; }

        [DataMember]
        public CurrencyActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string ItemName { get; set; }
        [DataMember]
        public string Amount { get; set; }
        [DataMember]
        public bool DeductFromUser { get; set; }
        [DataMember]
        public UserRoleEnum MinimumRole { get; set; }

        public CurrencyActionModel(CurrencyModel currency, CurrencyActionTypeEnum actionType, string amount, string username = null, UserRoleEnum minimumRole = UserRoleEnum.User, bool deductFromUser = false)
            : base(ActionTypeEnum.Currency)
        {
            this.CurrencyID = currency.ID;
            this.ActionType = actionType;
            this.Amount = amount;
            this.Username = username;
            this.MinimumRole = minimumRole;
            this.DeductFromUser = deductFromUser;
        }

        public CurrencyActionModel(InventoryModel inventory, CurrencyActionTypeEnum actionType, string itemName = null, string amount = null, string username = null, UserRoleEnum minimumRole = UserRoleEnum.User, bool deductFromUser = false)
            : base(ActionTypeEnum.Currency)
        {
            this.InventoryID = inventory.ID;
            this.ActionType = actionType;
            this.ItemName = itemName;
            this.Amount = amount;
            this.Username = username;
            this.MinimumRole = minimumRole;
            this.DeductFromUser = deductFromUser;
        }

        public CurrencyActionModel(StreamPassModel streamPass, CurrencyActionTypeEnum actionType, string amount, string username = null, UserRoleEnum minimumRole = UserRoleEnum.User, bool deductFromUser = false)
            : base(ActionTypeEnum.Currency)
        {
            this.StreamPassID = streamPass.ID;
            this.ActionType = actionType;
            this.Amount = amount;
            this.Username = username;
            this.MinimumRole = minimumRole;
            this.DeductFromUser = deductFromUser;
        }

        internal CurrencyActionModel(MixItUp.Base.Actions.CurrencyAction action)
            : base(ActionTypeEnum.Currency)
        {
            this.CurrencyID = action.CurrencyID;
            this.InventoryID = action.InventoryID;
            this.ItemName = action.ItemName;
            this.StreamPassID = action.StreamPassID;
            this.ActionType = (CurrencyActionTypeEnum)(int)action.CurrencyActionType;
            this.Amount = action.Amount;
            this.Username = action.Username;
            this.MinimumRole = action.RoleRequirement;
            this.DeductFromUser = action.DeductFromUser;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            CurrencyModel currency = null;

            InventoryModel inventory = null;
            InventoryItemModel item = null;

            StreamPassModel streamPass = null;

            string systemName = null;

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
                if (!ChannelSession.Settings.Inventory.ContainsKey(this.InventoryID))
                {
                    return;
                }
                inventory = ChannelSession.Settings.Inventory[this.InventoryID];
                systemName = inventory.Name;

                if (!string.IsNullOrEmpty(this.ItemName))
                {
                    string itemName = await this.ReplaceStringWithSpecialModifiers(this.ItemName, user, platform, arguments, specialIdentifiers);
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

            if (this.ActionType == CurrencyActionTypeEnum.ResetForAllUsers)
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
            else if (this.ActionType == CurrencyActionTypeEnum.ResetForUser)
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
                string amountTextValue = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, platform, arguments, specialIdentifiers);
                if (!double.TryParse(amountTextValue, out double doubleAmount))
                {
                    await ChannelSession.Services.Chat.SendMessage(string.Format("{0} is not a valid amount of {1}", amountTextValue, systemName));
                    return;
                }

                int amountValue = (int)Math.Ceiling(doubleAmount);
                if (amountValue <= 0)
                {
                    await ChannelSession.Services.Chat.SendMessage("The amount specified must be greater than 0");
                    return;
                }

                HashSet<UserDataModel> receiverUserData = new HashSet<UserDataModel>();
                if (this.ActionType == CurrencyActionTypeEnum.AddToUser)
                {
                    receiverUserData.Add(user.Data);
                }
                else if (this.ActionType == CurrencyActionTypeEnum.AddToSpecificUser || this.ActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser)
                {
                    if (!string.IsNullOrEmpty(this.Username))
                    {
                        string usernameString = await this.ReplaceStringWithSpecialModifiers(this.Username, user, platform, arguments, specialIdentifiers);
                        UserViewModel receivingUser = ChannelSession.Services.User.GetUserByUsername(usernameString, platform);
                        if (receivingUser != null)
                        {
                            receiverUserData.Add(receivingUser.Data);
                        }
                        else
                        {
                            await ChannelSession.Services.Chat.SendMessage("The user could not be found");
                            return;
                        }
                    }
                }
                else if (this.ActionType == CurrencyActionTypeEnum.AddToAllChatUsers || this.ActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
                {
                    foreach (UserViewModel chatUser in ChannelSession.Services.User.GetAllWorkableUsers())
                    {
                        if (chatUser.HasPermissionsTo(this.MinimumRole))
                        {
                            receiverUserData.Add(chatUser.Data);
                        }
                    }
                    receiverUserData.Add(ChannelSession.GetCurrentUser().Data);
                }

                if ((this.DeductFromUser && receiverUserData.Count > 0) || this.ActionType == CurrencyActionTypeEnum.SubtractFromUser)
                {
                    if (currency != null)
                    {
                        if (!currency.HasAmount(user.Data, amountValue))
                        {
                            await ChannelSession.Services.Chat.SendMessage(string.Format("You do not have the required {0} {1} to do this", amountValue, systemName));
                            return;
                        }
                        currency.SubtractAmount(user.Data, amountValue);
                    }
                    else if (inventory != null)
                    {
                        if (!inventory.HasAmount(user.Data, item, amountValue))
                        {
                            await ChannelSession.Services.Chat.SendMessage(string.Format("You do not have the required {0} {1} to do this", amountValue, item));
                            return;
                        }
                        inventory.SubtractAmount(user.Data, item, amountValue);
                    }
                    else if (streamPass != null)
                    {
                        if (!streamPass.HasAmount(user.Data, amountValue))
                        {
                            await ChannelSession.Services.Chat.SendMessage(string.Format("You do not have the required {0} {1} to do this", amountValue, systemName));
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
                            if (this.ActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.ActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
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
                            if (this.ActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.ActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
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
                            if (this.ActionType == CurrencyActionTypeEnum.SubtractFromSpecificUser || this.ActionType == CurrencyActionTypeEnum.SubtractFromAllChatUsers)
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
