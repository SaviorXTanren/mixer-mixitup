using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum StreamlabsActionTypeEnum
    {
        [Name("Spin the Wheel")]
        SpinWheel,
        [Name("Empty the Jar")]
        EmptyJar,
        [Name("Roll Credits")]
        RollCredits,
    }

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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.Streamlabs != null)
            {
                switch (this.StreamlabType)
                {
                    case StreamlabsActionTypeEnum.SpinWheel:
                        await ChannelSession.Services.Streamlabs.SpinWheel();
                        break;
                    case StreamlabsActionTypeEnum.EmptyJar:
                        await ChannelSession.Services.Streamlabs.EmptyJar();
                        break;
                    case StreamlabsActionTypeEnum.RollCredits:
                        await ChannelSession.Services.Streamlabs.RollCredits();
                        break;
                }
            }
        }
    }
}
