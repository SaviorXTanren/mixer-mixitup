using MixItUp.Base.Model.Commands;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class IFTTTActionModel : ActionModelBase
    {
        [DataMember]
        public string EventName { get; set; }

        [DataMember]
        public string EventValue1 { get; set; }
        [DataMember]
        public string EventValue2 { get; set; }
        [DataMember]
        public string EventValue3 { get; set; }

        public IFTTTActionModel(string eventName, string eventValue1, string eventValue2, string eventValue3)
            : base(ActionTypeEnum.IFTTT)
        {
            this.EventName = eventName;
            this.EventValue1 = eventValue1;
            this.EventValue2 = eventValue2;
            this.EventValue3 = eventValue3;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal IFTTTActionModel(MixItUp.Base.Actions.IFTTTAction action)
            : base(ActionTypeEnum.IFTTT)
        {
            this.EventName = action.EventName;
            this.EventValue1 = action.EventValue1;
            this.EventValue2 = action.EventValue2;
            this.EventValue3 = action.EventValue3;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private IFTTTActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Services.IFTTT.IsConnected)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(this.EventValue1))
                {
                    values["value1"] = await ReplaceStringWithSpecialModifiers(this.EventValue1, parameters);
                }
                if (!string.IsNullOrEmpty(this.EventValue2))
                {
                    values["value2"] = await ReplaceStringWithSpecialModifiers(this.EventValue2, parameters);
                }
                if (!string.IsNullOrEmpty(this.EventValue3))
                {
                    values["value3"] = await ReplaceStringWithSpecialModifiers(this.EventValue3, parameters);
                }
                await ChannelSession.Services.IFTTT.SendTrigger(this.EventName, values);
            }
        }
    }
}
