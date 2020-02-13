using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class IFTTTAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return IFTTTAction.asyncSemaphore; } }

        [DataMember]
        public string EventName { get; set; }

        [DataMember]
        public string EventValue1 { get; set; }
        [DataMember]
        public string EventValue2 { get; set; }
        [DataMember]
        public string EventValue3 { get; set; }

        public IFTTTAction() : base(ActionTypeEnum.IFTTT) { }

        public IFTTTAction(string eventName, string eventValue1, string eventValue2, string eventValue3)
            : this()
        {
            this.EventName = eventName;
            this.EventValue1 = eventValue1;
            this.EventValue2 = eventValue2;
            this.EventValue3 = eventValue3;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.IFTTT.IsConnected)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(this.EventValue1))
                {
                    values["value1"] = await this.ReplaceStringWithSpecialModifiers(this.EventValue1, user, arguments);
                }
                if (!string.IsNullOrEmpty(this.EventValue2))
                {
                    values["value2"] = await this.ReplaceStringWithSpecialModifiers(this.EventValue2, user, arguments);
                }
                if (!string.IsNullOrEmpty(this.EventValue3))
                {
                    values["value3"] = await this.ReplaceStringWithSpecialModifiers(this.EventValue3, user, arguments);
                }
                await ChannelSession.Services.IFTTT.SendTrigger(this.EventName, values);
            }
        }
    }
}
