using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
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

        private VTubeStudioActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.VTubeStudioOAuthToken != null && !ChannelSession.Services.VTubeStudio.IsConnected)
            {
                Result result = await ChannelSession.Services.VTubeStudio.Connect(ChannelSession.Settings.VTubeStudioOAuthToken);
                if (!result.Success)
                {
                    return;
                }
            }

            if (ChannelSession.Services.VTubeStudio.IsConnected)
            {
                if (this.ActionType == VTubeStudioActionTypeEnum.LoadModel)
                {
                    await ChannelSession.Services.VTubeStudio.LoadModel(this.ModelID);
                }
                else if (this.ActionType == VTubeStudioActionTypeEnum.MoveModel)
                {
                    await ChannelSession.Services.VTubeStudio.MoveModel(this.MovementTimeInSeconds, this.MovementRelative, this.MovementX, this.MovementY, this.Rotation, this.Size);
                }
                else if (this.ActionType == VTubeStudioActionTypeEnum.RunHotKey)
                {
                    await ChannelSession.Services.VTubeStudio.RunHotKey(this.HotKeyID);
                }
            }
        }
    }
}