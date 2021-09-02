using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
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

        protected override Task PerformInternal(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}
