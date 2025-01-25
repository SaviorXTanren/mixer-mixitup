using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class MeldStudioActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.MeldStudio; } }

        public IEnumerable<MeldStudioActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<MeldStudioActionTypeEnum>(); } }

        public MeldStudioActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowScene));
                this.NotifyPropertyChanged(nameof(this.ShowHideLayer));
                this.NotifyPropertyChanged(nameof(this.ShowHideEffect));
                this.NotifyPropertyChanged(nameof(this.MuteUnmuteAudioTrack));
                this.NotifyPropertyChanged(nameof(this.MonitorUnmonitorAudioTrack));
                this.NotifyPropertyChanged(nameof(this.SetAudioTrackGain));
                this.NotifyPropertyChanged(nameof(this.StartStopStream));
                this.NotifyPropertyChanged(nameof(this.StartStopRecording));
            }
        }
        private MeldStudioActionTypeEnum selectedActionType;

        public bool ShowScene { get { return this.SelectedActionType == MeldStudioActionTypeEnum.ShowScene; } }
        public bool ShowHideLayer { get { return this.SelectedActionType == MeldStudioActionTypeEnum.ShowHideLayer; } }
        public bool ShowHideEffect { get { return this.SelectedActionType == MeldStudioActionTypeEnum.ShowHideEffect; } }
        public bool MuteUnmuteAudioTrack { get { return this.SelectedActionType == MeldStudioActionTypeEnum.MuteUnmuteAudioTrack; } }
        public bool MonitorUnmonitorAudioTrack { get { return this.SelectedActionType == MeldStudioActionTypeEnum.MonitorUnmonitorAudioTrack; } }
        public bool SetAudioTrackGain { get { return this.SelectedActionType == MeldStudioActionTypeEnum.SetAudioTrackGain; } }
        public bool StartStopStream { get { return this.SelectedActionType == MeldStudioActionTypeEnum.StartStopStream; } }
        public bool StartStopRecording { get { return this.SelectedActionType == MeldStudioActionTypeEnum.StartStopRecording; } }

        public string SceneName
        {
            get { return this.sceneName; }
            set
            {
                this.sceneName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sceneName;

        public string LayerName
        {
            get { return this.layerName; }
            set
            {
                this.layerName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string layerName;

        public string EffectName
        {
            get { return this.effectName; }
            set
            {
                this.effectName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string effectName;

        public string AudioTrackName
        {
            get { return this.audioTrackName; }
            set
            {
                this.audioTrackName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string audioTrackName;

        public bool? State
        {
            get { return this.state; }
            set
            {
                this.state = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool? state = true;

        public int AudioTrackGain
        {
            get { return this.audioTrackGain; }
            set
            {
                this.audioTrackGain = value;
                this.NotifyPropertyChanged();
            }
        }
        private int audioTrackGain = MeldStudioService.AudioTrackGainMaximum;

        public MeldStudioActionEditorControlViewModel(MeldStudioActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            this.SceneName = action.SceneName;
            this.LayerName = action.LayerName;
            this.EffectName = action.EffectName;
            this.AudioTrackName = action.AudioTrackName;
            this.State = action.State;
            this.AudioTrackGain = action.AudioTrackGain;
        }

        public MeldStudioActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.SelectedActionType == MeldStudioActionTypeEnum.ShowScene)
            {
                if (string.IsNullOrEmpty(this.SceneName))
                {
                    return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.MeldStudioActionMissingSceneName));
                }
            }
            else if (this.SelectedActionType == MeldStudioActionTypeEnum.ShowHideLayer)
            {
                if (string.IsNullOrEmpty(this.LayerName))
                {
                    return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.MeldStudioActionMissingLayerName));
                }
            }
            else if (this.SelectedActionType == MeldStudioActionTypeEnum.ShowHideEffect)
            {
                if (string.IsNullOrEmpty(this.LayerName))
                {
                    return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.MeldStudioActionMissingLayerName));
                }

                if (string.IsNullOrEmpty(this.EffectName))
                {
                    return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.MeldStudioActionMissingEffectName));
                }
            }
            else if (this.SelectedActionType == MeldStudioActionTypeEnum.MuteUnmuteAudioTrack || this.SelectedActionType == MeldStudioActionTypeEnum.MonitorUnmonitorAudioTrack)
            {
                if (string.IsNullOrEmpty(this.AudioTrackName))
                {
                    return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.MeldStudioActionMissingAudioTrackName));
                }
            }

            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            return Task.FromResult<ActionModelBase>(new MeldStudioActionModel(this.SelectedActionType)
            {
                SceneName = this.SceneName,
                LayerName = this.LayerName,
                EffectName = this.EffectName,
                AudioTrackName = this.AudioTrackName,
                State = this.State,
                AudioTrackGain = this.AudioTrackGain,
            });
        }
    }
}
