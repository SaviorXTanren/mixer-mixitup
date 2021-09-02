using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum CurrencyActionTypeEnum
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

    [Obsolete]
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

        public CurrencyAction(InventoryModel inventory, CurrencyActionTypeEnum currencyActionType, string itemName = null, string amount = null, string username = null,
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

        protected override Task PerformInternal(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}
