using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
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
        public Guid ConnectionID { get; set; }

        [DataMember]
        [Obsolete]
        public string ModelID { get; set; }
        [DataMember]
        public string ModelName { get; set; }

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
        [Obsolete]
        public string HotKeyID { get; set; }
        [DataMember]
        public string HotKeyName { get; set; }

        public VTubeStudioActionModel(VTubeStudioActionTypeEnum actionType)
            : base(ActionTypeEnum.VTubeStudio)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public VTubeStudioActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<VTubeStudioService>().IsEnabled)
            {
                if (!ServiceManager.Get<VTubeStudioService>().IsClientConnected(this.ConnectionID))
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(1000 * 5);
                    Result result = await ServiceManager.Get<VTubeStudioService>().ConnectClient(this.ConnectionID, cancellationTokenSource.Token);
                    if (!result.Success)
                    {
                        return;
                    }
                }

                if (ServiceManager.Get<VTubeStudioService>().IsClientConnected(this.ConnectionID))
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    VTubeStudioModel swapModel = null;

                    if (!string.IsNullOrEmpty(this.ModelID) && string.IsNullOrEmpty(this.ModelName))
                    {
                        IEnumerable<VTubeStudioModel> ms = await ServiceManager.Get<VTubeStudioService>().GetAllModels(this.ConnectionID);
                        if (ms != null)
                        {
                            foreach (VTubeStudioModel model in ms)
                            {
                                if (string.Equals(model.modelID, this.ModelID, StringComparison.OrdinalIgnoreCase))
                                {
                                    swapModel = model;
                                    this.ModelName = model.modelName;
                                    this.ModelID = null;
                                    break;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(this.HotKeyID) && string.IsNullOrEmpty(this.HotKeyName))
                    {
                        IEnumerable<VTubeStudioHotKey> hks = await ServiceManager.Get<VTubeStudioService>().GetHotKeys(this.ConnectionID, swapModel?.modelID);
                        if (hks != null)
                        {
                            foreach (VTubeStudioHotKey hotkey in hks)
                            {
                                if (string.Equals(hotkey.hotkeyID, this.HotKeyID, StringComparison.OrdinalIgnoreCase))
                                {
                                    this.HotKeyName = hotkey.name;
                                    this.HotKeyID = null;
                                    break;
                                }
                            }
                        }
                    }
#pragma warning restore CS0612 // Type or member is obsolete

                    if (this.ActionType == VTubeStudioActionTypeEnum.LoadModel)
                    {
                        IEnumerable<VTubeStudioModel> models = await ServiceManager.Get<VTubeStudioService>().GetAllModels(this.ConnectionID);
                        if (models != null)
                        {
                            foreach (VTubeStudioModel model in models)
                            {
                                if (string.Equals(model.modelName, this.ModelName, StringComparison.OrdinalIgnoreCase))
                                {
                                    await ServiceManager.Get<VTubeStudioService>().LoadModel(this.ConnectionID, model.modelID);
                                    return;
                                }
                            }
                        }
                    }
                    else if (this.ActionType == VTubeStudioActionTypeEnum.MoveModel)
                    {
                        await ServiceManager.Get<VTubeStudioService>().MoveModel(this.ConnectionID, this.MovementTimeInSeconds, this.MovementRelative, this.MovementX, this.MovementY, this.Rotation, this.Size);
                    }
                    else if (this.ActionType == VTubeStudioActionTypeEnum.RunHotKey)
                    {
                        string modelID = null;
                        if (!string.IsNullOrEmpty(this.ModelName))
                        {
                            IEnumerable<VTubeStudioModel> models = await ServiceManager.Get<VTubeStudioService>().GetAllModels(this.ConnectionID);
                            if (models != null)
                            {
                                foreach (VTubeStudioModel model in models)
                                {
                                    if (string.Equals(model.modelName, this.ModelName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        modelID = model.modelID;
                                        break;
                                    }
                                }
                            }
                        }

                        IEnumerable<VTubeStudioHotKey> hotKeys = await ServiceManager.Get<VTubeStudioService>().GetHotKeys(this.ConnectionID, modelID);
                        if (hotKeys != null)
                        {
                            foreach (VTubeStudioHotKey hotKey in hotKeys)
                            {
                                if (string.Equals(hotKey.name, this.HotKeyName, StringComparison.OrdinalIgnoreCase))
                                {
                                    await ServiceManager.Get<VTubeStudioService>().RunHotKey(this.ConnectionID, hotKey.hotkeyID);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}