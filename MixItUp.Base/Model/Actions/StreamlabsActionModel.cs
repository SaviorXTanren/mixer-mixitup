using MixItUp.Base.Model.Commands;
using StreamingClient.Base.Util;
using System.Runtime.Serialization;
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
        [DataMember]
        public StreamlabsActionTypeEnum ActionType { get; set; }

        public StreamlabsActionModel(StreamlabsActionTypeEnum type)
            : base(ActionTypeEnum.Streamlabs)
        {
            this.ActionType = type;
        }

        internal StreamlabsActionModel(MixItUp.Base.Actions.StreamlabsAction action)
            : base(ActionTypeEnum.Streamlabs)
        {
            this.ActionType = (StreamlabsActionTypeEnum)(int)action.StreamlabType;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
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
