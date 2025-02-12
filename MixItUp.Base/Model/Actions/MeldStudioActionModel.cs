using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum MeldStudioActionTypeEnum
    {
        ShowScene,
        ShowHideLayer,
        ShowHideEffect,
        MuteUnmuteAudioTrack,
        MonitorUnmonitorAudioTrack,
        SetAudioTrackGain,
        TakeScreenshot,
        StartStopStream,
        StartStopRecording,
        RecordClip,
    }

    [DataContract]
    public class MeldStudioActionModel : ActionModelBase
    {
        [DataMember]
        public MeldStudioActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string SceneName { get; set; }
        [DataMember]
        public string LayerName { get; set; }
        [DataMember]
        public string EffectName { get; set; }
        [DataMember]
        public string AudioTrackName { get; set; }

        [DataMember]
        public bool? State { get; set; }

        [DataMember]
        public int AudioTrackGain { get; set; } = MeldStudioService.AudioTrackGainMaximum;

        public MeldStudioActionModel(MeldStudioActionTypeEnum actionType)
            : base(ActionTypeEnum.MeldStudio)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public MeldStudioActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<MeldStudioService>().IsEnabled)
            {
                if (!ServiceManager.Get<MeldStudioService>().IsConnected)
                {
                    Result result = await ServiceManager.Get<MeldStudioService>().AutomaticConnect();
                    if (!result.Success)
                    {
                        return;
                    }
                }

                if (this.ActionType == MeldStudioActionTypeEnum.ShowScene)
                {
                    string sceneName = await ReplaceStringWithSpecialModifiers(this.SceneName, parameters);
                    await ServiceManager.Get<MeldStudioService>().ShowScene(sceneName);
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.ShowHideLayer)
                {
                    string sceneName = await ReplaceStringWithSpecialModifiers(this.SceneName, parameters);
                    string layerName = await ReplaceStringWithSpecialModifiers(this.LayerName, parameters);
                    await ServiceManager.Get<MeldStudioService>().ChangeLayerState(sceneName, layerName, this.State);
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.ShowHideEffect)
                {
                    string sceneName = await ReplaceStringWithSpecialModifiers(this.SceneName, parameters);
                    string layerName = await ReplaceStringWithSpecialModifiers(this.LayerName, parameters);
                    string effectName = await ReplaceStringWithSpecialModifiers(this.EffectName, parameters);
                    await ServiceManager.Get<MeldStudioService>().ChangeEffectState(sceneName, layerName, effectName, this.State);
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.MuteUnmuteAudioTrack)
                {
                    string audioTrackName = await ReplaceStringWithSpecialModifiers(this.AudioTrackName, parameters);
                    await ServiceManager.Get<MeldStudioService>().ChangeMuteState(audioTrackName, this.State);
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.MonitorUnmonitorAudioTrack)
                {
                    string audioTrackName = await ReplaceStringWithSpecialModifiers(this.AudioTrackName, parameters);
                    await ServiceManager.Get<MeldStudioService>().ChangeMonitorState(audioTrackName, this.State);
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.SetAudioTrackGain)
                {
                    string audioTrackName = await ReplaceStringWithSpecialModifiers(this.AudioTrackName, parameters);
                    await ServiceManager.Get<MeldStudioService>().SetGain(audioTrackName, this.AudioTrackGain);
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.TakeScreenshot)
                {
                    await ServiceManager.Get<MeldStudioService>().TakeScreenshot();
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.RecordClip)
                {
                    await ServiceManager.Get<MeldStudioService>().RecordClip();
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.StartStopStream)
                {
                    await ServiceManager.Get<MeldStudioService>().ChangeStreamState(this.State);
                }
                else if (this.ActionType == MeldStudioActionTypeEnum.StartStopRecording)
                {
                    await ServiceManager.Get<MeldStudioService>().ChangeRecordState(this.State);
                }
            }
        }
    }
}
