using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum VTubeStudioActionTypeEnum
    {
        LoadModel,
        MoveModel,
        RunHotKey,
    }

    [DataContract]
    public class VTubeStudioActionModel : ActionModelBase
    {
        public static VTubeStudioActionModel CreateForModelLoad(string modelID) { return new VTubeStudioActionModel(VTubeStudioActionTypeEnum.LoadModel) { ModelID = modelID }; }

        public static VTubeStudioActionModel CreateForMoveModel(double timeInSeconds, bool relative, double? x, double? y, double? rotation, double? size)
        {
            return new VTubeStudioActionModel(VTubeStudioActionTypeEnum.MoveModel)
            {
                MovementTimeInSeconds = timeInSeconds,
                MovementRelative = relative,
                MovementX = x,
                MovementY = y,
                Rotation = rotation,
                Size = size,
            };
        }

        public static VTubeStudioActionModel CreateForRunHotKey(string modelID, string hotKeyID) { return new VTubeStudioActionModel(VTubeStudioActionTypeEnum.RunHotKey) { ModelID = modelID, HotKeyID = hotKeyID }; }

        [DataMember]
        public VTubeStudioActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string ModelID { get; set; }

        [DataMember]
        public double MovementTimeInSeconds { get; set; }
        [DataMember]
        public bool MovementRelative { get; set; }
        [DataMember]
        public double? MovementX { get; set; }
        [DataMember]
        public double? MovementY { get; set; }
        [DataMember]
        public double? Rotation { get; set; }
        [DataMember]
        public double? Size { get; set; }

        [DataMember]
        public string HotKeyID { get; set; }

        public VTubeStudioActionModel(VTubeStudioActionTypeEnum actionType)
            : base(ActionTypeEnum.VTubeStudio)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public VTubeStudioActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.VTubeStudioOAuthToken != null && !ServiceManager.Get<VTubeStudioService>().IsConnected)
            {
                Result result = await ServiceManager.Get<VTubeStudioService>().Connect(ChannelSession.Settings.VTubeStudioOAuthToken);
                if (!result.Success)
                {
                    return;
                }
            }

            if (ServiceManager.Get<VTubeStudioService>().IsConnected)
            {
                if (this.ActionType == VTubeStudioActionTypeEnum.LoadModel)
                {
                    await ServiceManager.Get<VTubeStudioService>().LoadModel(this.ModelID);
                }
                else if (this.ActionType == VTubeStudioActionTypeEnum.MoveModel)
                {
                    await ServiceManager.Get<VTubeStudioService>().MoveModel(this.MovementTimeInSeconds, this.MovementRelative, this.MovementX, this.MovementY, this.Rotation, this.Size);
                }
                else if (this.ActionType == VTubeStudioActionTypeEnum.RunHotKey)
                {
                    await ServiceManager.Get<VTubeStudioService>().RunHotKey(this.HotKeyID);
                }
            }
        }
    }
}