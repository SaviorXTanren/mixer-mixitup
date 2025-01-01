using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class SoundActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Sound; } }

        public IEnumerable<SoundActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<SoundActionTypeEnum>(); } }

        public SoundActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowPlaySoundGrid));
            }
        }
        private SoundActionTypeEnum selectedActionType;

        public bool ShowPlaySoundGrid { get { return this.SelectedActionType == SoundActionTypeEnum.PlaySound; } }

        public string FilePath
        {
            get { return this.filePath; }
            set
            {
                this.filePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string filePath;

        public ObservableCollection<string> AudioDevices { get; set; } = new ObservableCollection<string>();

        public string SelectedAudioDevice
        {
            get { return this.selectedAudioDevice; }
            set
            {
                this.selectedAudioDevice = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedAudioDevice;

        public int Volume
        {
            get { return this.volume; }
            set
            {
                this.volume = value;
                this.NotifyPropertyChanged();
            }
        }
        private int volume = 100;

        public SoundActionEditorControlViewModel(SoundActionModel action)
            : base(action)
        {
            this.LoadSoundDevices();

            this.SelectedActionType = action.ActionType;
            if (this.ShowPlaySoundGrid)
            {
                this.FilePath = action.FilePath;
                this.SelectedAudioDevice = (action.OutputDevice != null) ? action.OutputDevice : ServiceManager.Get<IAudioService>().DefaultAudioDevice;
                this.Volume = action.VolumeScale;
            }
        }

        public SoundActionEditorControlViewModel()
            : base()
        {
            this.LoadSoundDevices();
            this.SelectedAudioDevice = ServiceManager.Get<IAudioService>().DefaultAudioDevice;
        }

        public override Task<Result> Validate()
        {
            if (this.ShowPlaySoundGrid)
            {
                if (string.IsNullOrEmpty(this.FilePath))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.SoundActionMissingFilePath));
                }
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowPlaySoundGrid)
            {
                if (!string.Equals(this.SelectedAudioDevice, ServiceManager.Get<IAudioService>().DefaultAudioDevice))
                {
                    return Task.FromResult<ActionModelBase>(new SoundActionModel(this.FilePath, this.Volume, this.SelectedAudioDevice));
                }
                else
                {
                    return Task.FromResult<ActionModelBase>(new SoundActionModel(this.FilePath, this.Volume));
                }
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new SoundActionModel(this.SelectedActionType));
            }
        }

        private void LoadSoundDevices()
        {
            this.AudioDevices.AddRange(ServiceManager.Get<IAudioService>().GetSelectableAudioDevices(includeOverlay: true));
        }
    }
}
