using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum VTubeStudioActionTypeEnum
    {
        LoadModel,
        RunHotKey,
    }

    [DataContract]
    public class VTubeStudioActionModel : ActionModelBase
    {
        public static VTubeStudioActionModel CreateForModelLoad(string modelID) { return new VTubeStudioActionModel(VTubeStudioActionTypeEnum.LoadModel) { ModelID = modelID }; }

        public static VTubeStudioActionModel CreateForRunHotKey(string modelID, string hotKeyID) { return new VTubeStudioActionModel(VTubeStudioActionTypeEnum.RunHotKey) { ModelID = modelID, HotKeyID = hotKeyID }; }

        [DataMember]
        public VTubeStudioActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string ModelID { get; set; }

        [DataMember]
        public string HotKeyID { get; set; }

        public VTubeStudioActionModel(VTubeStudioActionTypeEnum actionType)
            : base(ActionTypeEnum.VTubeStudio)
        {
            this.ActionType = actionType;
        }

        private VTubeStudioActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<VTubeStudioService>().IsConnected)
            {
                if (this.ActionType == VTubeStudioActionTypeEnum.LoadModel)
                {
                    await ServiceManager.Get<VTubeStudioService>().LoadModel(this.ModelID);
                }
                else if (this.ActionType == VTubeStudioActionTypeEnum.RunHotKey)
                {
                    await ServiceManager.Get<VTubeStudioService>().RunHotKey(this.HotKeyID);
                }
            }
        }
    }
}