using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum ConsumablesActionTypeEnum
    {
        AddToUser,
        SubtractFromUser,

        AddToSpecificUser,
        AddToAllChatUsers,

        SubtractFromSpecificUser,
        SubtractFromAllChatUsers,

        ResetForAllUsers,
        ResetForUser,
    }

    [DataContract]
    public class ConsumablesActionModel : ActionModelBase
    {
        [DataMember]
        public Guid CurrencyID { get; set; }
        [DataMember]
        public Guid InventoryID { get; set; }
        [DataMember]
        public Guid StreamPassID { get; set; }

        [DataMember]
        public ConsumablesActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string ItemName { get; set; }
        [DataMember]
        public string Amount { get; set; }
        [DataMember]
        public bool DeductFromUser { get; set; }
        [DataMember]
        public UserRoleEnum UserRoleToApplyTo { get; set; }

        [DataMember]
        public bool UsersMustBePresent { get; set; } = true;

        public ConsumablesActionModel(CurrencyModel currency, ConsumablesActionTypeEnum actionType, bool usersMustBePresent, string amount, string username = null, UserRoleEnum userRoleToApplyTo = UserRoleEnum.User, bool deductFromUser = false)
            : this(actionType, usersMustBePresent, amount, username, userRoleToApplyTo, deductFromUser)
        {
            this.CurrencyID = currency.ID;
        }

        public ConsumablesActionModel(InventoryModel inventory, string itemName, ConsumablesActionTypeEnum actionType, bool usersMustBePresent, string amount, string username = null, UserRoleEnum userRoleToApplyTo = UserRoleEnum.User, bool deductFromUser = false)
            : this(actionType, usersMustBePresent, amount, username, userRoleToApplyTo, deductFromUser)
        {
            this.InventoryID = inventory.ID;
            this.ItemName = itemName;
        }

        public ConsumablesActionModel(StreamPassModel streamPass, ConsumablesActionTypeEnum actionType, bool usersMustBePresent, string amount, string username = null, UserRoleEnum userRoleToApplyTo = UserRoleEnum.User, bool deductFromUser = false)
            : this(actionType, usersMustBePresent, amount, username, userRoleToApplyTo, deductFromUser)
        {
            this.StreamPassID = streamPass.ID;
        }

        private ConsumablesActionModel(ConsumablesActionTypeEnum actionType, bool usersMustBePresent, string amount = null, string username = null, UserRoleEnum userRoleToApplyTo = UserRoleEnum.User, bool deductFromUser = false)
            : base(ActionTypeEnum.Consumables)
        {
            this.ActionType = actionType;
            this.Amount = amount;
            this.UsersMustBePresent = usersMustBePresent;
            this.Username = username;
            this.UserRoleToApplyTo = userRoleToApplyTo;
            this.DeductFromUser = deductFromUser;
        }

        [Obsolete]
        public ConsumablesActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
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
                    string itemName = await ReplaceStringWithSpecialModifiers(this.ItemName, parameters);
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

            if (this.ActionType == ConsumablesActionTypeEnum.ResetForAllUsers)
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
            else if (this.ActionType == ConsumablesActionTypeEnum.ResetForUser)
            {
                if (currency != null)
                {
                    currency.ResetAmount(parameters.User);
                }
                else if (inventory != null)
                {
                    inventory.ResetAmount(parameters.User);
                }
                else if (streamPass != null)
                {
                    streamPass.ResetAmount(parameters.User);
                }
            }
            else
            {
                string amountTextValue = await ReplaceStringWithSpecialModifiers(this.Amount, parameters);
                amountTextValue = MathHelper.ProcessMathEquation(amountTextValue).ToString();

                if (!double.TryParse(amountTextValue, out double doubleAmount))
                {
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.CounterActionNotAValidAmount, amountTextValue, systemName), parameters);
                    return;
                }

                int amountValue = (int)Math.Floor(doubleAmount);
                if (amountValue < 0)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountGreaterThan, amountTextValue, systemName), parameters);
                    return;
                }

                HashSet<UserV2ViewModel> receiverUserData = new HashSet<UserV2ViewModel>();
                if (this.ActionType == ConsumablesActionTypeEnum.AddToUser)
                {
                    receiverUserData.Add(parameters.User);
                }
                else if (this.ActionType == ConsumablesActionTypeEnum.AddToSpecificUser || this.ActionType == ConsumablesActionTypeEnum.SubtractFromSpecificUser)
                {
                    UserV2ViewModel receivingUser = parameters.TargetUser;
                    if (!string.IsNullOrEmpty(this.Username))
                    {
                        string usernameString = await ReplaceStringWithSpecialModifiers(this.Username, parameters);
                        receivingUser = await ServiceManager.Get<UserService>().GetUserByPlatform(parameters.Platform, platformUsername: usernameString, performPlatformSearch: true);
                        if (this.UsersMustBePresent && receivingUser != null)
                        {
                            if (!ServiceManager.Get<UserService>().IsUserActive(receivingUser.ID))
                            {
                                receivingUser = null;
                            }
                        }
                    }

                    if (receivingUser != null)
                    {
                        receiverUserData.Add(receivingUser);
                    }
                    else
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.UserNotFound, parameters);
                        return;
                    }
                }
                else if (this.ActionType == ConsumablesActionTypeEnum.AddToAllChatUsers || this.ActionType == ConsumablesActionTypeEnum.SubtractFromAllChatUsers)
                {
                    foreach (UserV2ViewModel chatUser in ServiceManager.Get<UserService>().GetActiveUsers())
                    {
                        if (chatUser.HasRole(this.UserRoleToApplyTo))
                        {
                            receiverUserData.Add(chatUser);
                        }
                    }
                    receiverUserData.Add(ChannelSession.User);
                }

                if ((this.DeductFromUser && receiverUserData.Count > 0) || this.ActionType == ConsumablesActionTypeEnum.SubtractFromUser)
                {
                    if (currency != null)
                    {
                        if (!currency.HasAmount(parameters.User, amountValue))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, amountValue, systemName), parameters);
                            return;
                        }
                        currency.SubtractAmount(parameters.User, amountValue);
                    }
                    else if (inventory != null)
                    {
                        if (!inventory.HasAmount(parameters.User, item, amountValue))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, amountValue, item.Name), parameters);
                            return;
                        }
                        inventory.SubtractAmount(parameters.User, item, amountValue);
                    }
                    else if (streamPass != null)
                    {
                        if (!streamPass.HasAmount(parameters.User, amountValue))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, amountValue, systemName), parameters);
                            return;
                        }
                        streamPass.SubtractAmount(parameters.User, amountValue);
                    }
                }

                if (receiverUserData.Count > 0)
                {
                    foreach (UserV2ViewModel receiverUser in receiverUserData)
                    {
                        if (currency != null)
                        {
                            if (this.ActionType == ConsumablesActionTypeEnum.SubtractFromSpecificUser || this.ActionType == ConsumablesActionTypeEnum.SubtractFromAllChatUsers)
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
                            if (this.ActionType == ConsumablesActionTypeEnum.SubtractFromSpecificUser || this.ActionType == ConsumablesActionTypeEnum.SubtractFromAllChatUsers)
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
                            if (this.ActionType == ConsumablesActionTypeEnum.SubtractFromSpecificUser || this.ActionType == ConsumablesActionTypeEnum.SubtractFromAllChatUsers)
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
