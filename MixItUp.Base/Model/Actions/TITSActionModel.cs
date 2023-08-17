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
        public int ThrowAmount { get; set; }

        [DataMember]
        public string TriggerID { get; set; }

        public TITSActionModel(TITSActionTypeEnum actionType, string throwItemID, double throwDelayTime, int throwAmount)
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
            if (ChannelSession.Settings.TITSOAuthToken != null && !ServiceManager.Get<TITSService>().IsConnected)
            {
                Result result = await ServiceManager.Get<TITSService>().Connect(ChannelSession.Settings.TITSOAuthToken);
                if (!result.Success)
                {
                    return;
                }
            }

            if (ServiceManager.Get<TITSService>().IsConnected)
            {
                if (this.ActionType == TITSActionTypeEnum.ThrowItem)
                {
                    await ServiceManager.Get<TITSService>().ThrowItem(this.ThrowItemID, this.ThrowDelayTime, this.ThrowAmount);
                }
                else if (this.ActionType == TITSActionTypeEnum.ActivateTrigger)
                {
                    await ServiceManager.Get<TITSService>().ActivateTrigger(this.TriggerID);
                }
            }
        }
    }
}