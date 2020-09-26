using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public class SoundActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Sound; } }

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

        public IEnumerable<string> AudioSources
        {
            get
            {
                List<string> devices = new List<string>();
                devices.Add(SoundActionModel.DefaultAudioDevice);
                devices.Add(SoundActionModel.MixItUpOverlay);
                devices.AddRange(ChannelSession.Services.AudioService.GetOutputDevices());
                return devices;
            }
        }

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
        private int volume;

        public SoundActionEditorControlViewModel(SoundActionModel action)
            : base(action)
        {
            this.FilePath = action.FilePath;
            this.SelectedAudioDevice = action.OutputDevice;
            this.Volume = action.VolumeScale;
        }

        public SoundActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.SoundActionMissingFilePath));
            }
            return Task.FromResult(new Result());
        }

        public override Task<ActionModelBase> GetAction()
        {
            if (!string.Equals(this.SelectedAudioDevice, SoundActionModel.DefaultAudioDevice))
            {
                return Task.FromResult<ActionModelBase>(new SoundActionModel(this.FilePath, this.Volume, this.SelectedAudioDevice));
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new SoundActionModel(this.FilePath, this.Volume));
            }
        }
    }
}
