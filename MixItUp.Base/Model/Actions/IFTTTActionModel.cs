using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
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

        [Obsolete]
        public IFTTTActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<IFTTTService>().IsConnected)
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
                await ServiceManager.Get<IFTTTService>().SendTrigger(this.EventName, values);
            }
        }
    }
}
