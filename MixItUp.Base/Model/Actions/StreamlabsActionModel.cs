using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
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
    public class StreamlabsActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return StreamlabsActionModel.asyncSemaphore; } }

        [DataMember]
        public StreamlabsActionTypeEnum ActionType { get; set; }

        public StreamlabsActionModel(StreamlabsActionTypeEnum type)
            : base(ActionTypeEnum.Streamlabs)
        {
            this.ActionType = type;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (ChannelSession.Services.Streamlabs.IsConnected)
            {
                switch (this.ActionType)
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
