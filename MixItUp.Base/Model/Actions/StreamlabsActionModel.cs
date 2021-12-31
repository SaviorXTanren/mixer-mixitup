using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum StreamlabsActionTypeEnum
    {
        SpinWheel,
        EmptyJar,
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

        [Obsolete]
        public StreamlabsActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<StreamlabsService>().IsConnected)
            {
                switch (this.ActionType)
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
