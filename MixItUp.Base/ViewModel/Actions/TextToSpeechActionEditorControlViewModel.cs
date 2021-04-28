using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TextToSpeechActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.TextToSpeech; } }

        public bool OverlayNotEnabled { get { return !ServiceManager.Get<OverlayService>().IsConnected; } }

        public IEnumerable<string> Voices { get { return TextToSpeechActionModel.AvailableVoices; } }

        public string SelectedVoice
        {
            get { return this.selectedVoice; }
            set
            {
                this.selectedVoice = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedVoice;

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

        public int Pitch
        {
            get { return this.pitch; }
            set
            {
                this.pitch = value;
                this.NotifyPropertyChanged();
            }
        }
        private int pitch = 100;

        public int Rate
        {
            get { return this.rate; }
            set
            {
                this.rate = value;
                this.NotifyPropertyChanged();
            }
        }
        private int rate = 100;

        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                this.NotifyPropertyChanged();
            }
        }
        private string text;

        public TextToSpeechActionEditorControlViewModel(TextToSpeechActionModel action)
            : base(action)
        {
            this.SelectedVoice = action.Voice;
            this.Text = action.Text;
            this.Volume = action.Volume;
            this.Pitch = action.Pitch;
            this.Rate = action.Rate;
        }

        public TextToSpeechActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.SelectedVoice))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionMissingVoice));
            }

            if (string.IsNullOrEmpty(this.Text))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionMissingText));
            }

            if (this.Volume < 0 || this.Volume > 100)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionInvalidVolume));
            }

            if (this.Pitch < 0 || this.Pitch > 200)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionInvalidPitch));
            }

            if (this.Rate < 0 || this.Rate > 150)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionInvalidRate));
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal() { return Task.FromResult<ActionModelBase>(new TextToSpeechActionModel(this.Text, this.SelectedVoice, this.Volume, this.Pitch, this.Rate)); }
    }
}
