using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum StreamlabsActionTypeEnum
    {
        SpinWheel,
        EmptyJar,
        RollCredits,
    }

    [Obsolete]
    [DataContract]
    public class StreamlabsAction : ActionBase
    {
        public static StreamlabsAction CreateForSpinWheel() { return new StreamlabsAction(StreamlabsActionTypeEnum.SpinWheel); }

        public static StreamlabsAction CreateForEmptyJar() { return new StreamlabsAction(StreamlabsActionTypeEnum.EmptyJar); }

        public static StreamlabsAction CreateForRollCredits() { return new StreamlabsAction(StreamlabsActionTypeEnum.RollCredits); }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return StreamlabsAction.asyncSemaphore; } }

        [DataMember]
        public StreamlabsActionTypeEnum StreamlabType { get; set; }

        public StreamlabsAction() : base(ActionTypeEnum.Streamlabs) { }

        public StreamlabsAction(StreamlabsActionTypeEnum type)
            : this()
        {
            this.StreamlabType = type;
        }

        protected override async Task PerformInternal(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            if (ServiceManager.Get<StreamlabsService>().IsConnected)
            {
                switch (this.StreamlabType)
                {
                    case StreamlabsActionTypeEnum.SpinWheel:
                        await ServiceManager.Get<StreamlabsService>().SpinWheel();
                        break;
                    case StreamlabsActionTypeEnum.EmptyJar:
                        await ServiceManager.Get<StreamlabsService>().EmptyJar();
                        break;
                    case StreamlabsActionTypeEnum.RollCredits:
                        await ServiceManager.Get<StreamlabsService>().RollCredits();
                        break;
                }
            }
        }
    }
}
