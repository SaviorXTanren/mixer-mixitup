using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum TITSActionTypeEnum
    {
        ThrowItem,
        ActivateTrigger,
    }

    [DataContract]
    public class TITSActionModel : ActionModelBase
    {
        [DataMember]
        public TITSActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string ThrowItemID { get; set; }
        [DataMember]
        public double ThrowDelayTime { get; set; }
        [DataMember]
        public string ThrowAmount { get; set; }

        [DataMember]
        public string TriggerID { get; set; }

        public TITSActionModel(TITSActionTypeEnum actionType, string throwItemID, double throwDelayTime, string throwAmount)
            : this(actionType)
        {
            this.ThrowItemID = throwItemID;
            this.ThrowDelayTime = throwDelayTime;
            this.ThrowAmount = throwAmount;
        }

        public TITSActionModel(TITSActionTypeEnum actionType, string triggerID)
            : this(actionType)
        {
            this.TriggerID = triggerID;
        }

        private TITSActionModel(TITSActionTypeEnum actionType)
            : base(ActionTypeEnum.TITS)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public TITSActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<TITSService>().IsEnabled && !ServiceManager.Get<TITSService>().IsConnected)
            {
                await ServiceManager.Get<TITSService>().Connect();
            }

            if (ServiceManager.Get<TITSService>().IsConnected)
            {
                if (this.ActionType == TITSActionTypeEnum.ThrowItem)
                {
                    int.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.ThrowAmount, parameters), out int amount);
                    if (amount <= 0)
                    {
                        amount = 1;
                    }
                    await ServiceManager.Get<TITSService>().ThrowItem(this.ThrowItemID, this.ThrowDelayTime, amount);
                }
                else if (this.ActionType == TITSActionTypeEnum.ActivateTrigger)
                {
                    await ServiceManager.Get<TITSService>().ActivateTrigger(this.TriggerID);
                }
            }
        }
    }
}