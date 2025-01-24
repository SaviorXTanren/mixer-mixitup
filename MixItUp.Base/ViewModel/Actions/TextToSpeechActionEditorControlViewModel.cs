using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TextToSpeechActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.TextToSpeech; } }

        public IEnumerable<TextToSpeechProviderType> ProviderTypes { get; private set; } = EnumHelper.GetEnumList<TextToSpeechProviderType>();

        public TextToSpeechProviderType SelectedProviderType
        {
            get { return this.selectedProviderType; }
            set
            {
                this.selectedProviderType = value;
                this.NotifyPropertyChanged();
                this.UpdateTextToSpeechProvider();

                this.NotifyPropertyChanged(nameof(this.NoCustomAmazonPollyAccount));
                this.NotifyPropertyChanged(nameof(this.NoCustomMicrosoftAzureSpeechAccount));

                this.NotifyPropertyChanged(nameof(this.PitchHintText));
                this.NotifyPropertyChanged(nameof(this.RateHintText));

                this.NotifyPropertyChanged(nameof(this.SupportsSSML));
            }
        }
        private TextToSpeechProviderType selectedProviderType = TextToSpeechProviderType.WindowsTextToSpeech;

        public bool UsesAudioDevices
        {
            get
            {
                return this.SelectedProviderType == TextToSpeechProviderType.WindowsTextToSpeech ||
                    this.SelectedProviderType == TextToSpeechProviderType.AmazonPolly ||
                    this.SelectedProviderType == TextToSpeechProviderType.MicrosoftAzureSpeech ||
                    this.SelectedProviderType == TextToSpeechProviderType.TTSMonster ||
                    this.SelectedProviderType == TextToSpeechProviderType.TikTokTTS;
            }
        }
        public bool AudioDeviceServiceConnected
        {
            get
            {
                if (this.SelectedProviderType == TextToSpeechProviderType.TTSMonster)
                {
                    return !this.TTSMonsterNotEnabled;
                }
                return true;
            }
        }
        public ThreadSafeObservableCollection<string> AudioDevices { get; set; } = new ThreadSafeObservableCollection<string>();
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

        public bool UsesOverlay { get { return this.SelectedProviderType == TextToSpeechProviderType.ResponsiveVoice; } }
        public bool OverlayEnabled { get { return this.UsesOverlay && ServiceManager.Get<OverlayV3Service>().IsConnected; } }
        public bool OverlayNotEnabled { get { return this.UsesOverlay && !ServiceManager.Get<OverlayV3Service>().IsConnected; } }

        public IEnumerable<OverlayEndpointV3Model> OverlayEndpoints { get { return ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints(); } }

        public OverlayEndpointV3Model SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                var overlays = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints();
                if (overlays.Contains(value))
                {
                    this.selectedOverlayEndpoint = value;
                }
                else
                {
                    this.selectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
                }
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndpointV3Model selectedOverlayEndpoint;

        public bool TTSMonsterNotEnabled { get { return this.SelectedProviderType == TextToSpeechProviderType.TTSMonster && !ServiceManager.Get<ITTSMonsterService>().IsConnected; } }

        public ThreadSafeObservableCollection<TextToSpeechVoice> Voices { get; private set; } = new ThreadSafeObservableCollection<TextToSpeechVoice>();

        public TextToSpeechVoice SelectedVoice
        {
            get { return this.selectedVoice; }
            set
            {
                this.selectedVoice = value;
                this.NotifyPropertyChanged();
            }
        }
        private TextToSpeechVoice selectedVoice;

        public int VolumeMinimum { get; private set; }
        public int VolumeMaximum { get; private set; }
        public bool VolumeChangable { get { return this.VolumeMinimum != this.VolumeMaximum; } }
        public int Volume
        {
            get { return this.volume; }
            set
            {
                this.volume = MathHelper.Clamp(value, this.VolumeMinimum, this.VolumeMaximum);
                this.NotifyPropertyChanged();
            }
        }
        private int volume = 0;

        public int PitchMinimum { get; private set; }
        public int PitchMaximum { get; private set; }
        public bool PitchChangable { get { return this.PitchMinimum != this.PitchMaximum; } }
        public int Pitch
        {
            get { return this.pitch; }
            set
            {
                this.pitch = MathHelper.Clamp(value, this.PitchMinimum, this.PitchMaximum);
                this.NotifyPropertyChanged();
            }
        }
        private int pitch = 0;

        public string PitchHintText
        {
            get
            {
                if (this.SelectedProviderType == TextToSpeechProviderType.ResponsiveVoice) { return Resources.TextToSpeechActionResponsiveVoicePitch; }
                return Resources.Pitch;
            }
        }

        public int RateMinimum { get; private set; }
        public int RateMaximum { get; private set; }
        public bool RateChangable { get { return this.RateMinimum != this.RateMaximum; } }
        public int Rate
        {
            get { return this.rate; }
            set
            {
                this.rate = MathHelper.Clamp(value, this.RateMinimum, this.RateMaximum);
                this.NotifyPropertyChanged();
            }
        }
        private int rate = 0;

        public string RateHintText
        {
            get
            {
                if (this.SelectedProviderType == TextToSpeechProviderType.WindowsTextToSpeech) { return Resources.TextToSpeechActionWindowsTextToSpeechRate; }
                else if (this.SelectedProviderType == TextToSpeechProviderType.ResponsiveVoice) { return Resources.TextToSpeechActionResponsiveVoiceRate; }
                return Resources.Rate;
            }
        }

        public bool SSML
        {
            get { return this.ssml; }
            set
            {
                this.ssml = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool ssml;
        public bool SupportsSSML
        {
            get
            {
                return this.SelectedProviderType == TextToSpeechProviderType.MicrosoftAzureSpeech || this.SelectedProviderType == TextToSpeechProviderType.AmazonPolly ||
                    this.SelectedProviderType == TextToSpeechProviderType.WindowsTextToSpeech;
            }
        }

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

        public bool WaitForFinish
        {
            get { return this.waitForFinish; }
            set
            {
                this.waitForFinish = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool waitForFinish;

        public bool NoCustomAmazonPollyAccount
        {
            get { return this.SelectedProviderType == TextToSpeechProviderType.AmazonPolly && string.IsNullOrEmpty(ChannelSession.Settings.AmazonPollyCustomAccessKey); }
        }

        public bool NoCustomMicrosoftAzureSpeechAccount
        {
            get { return this.SelectedProviderType == TextToSpeechProviderType.MicrosoftAzureSpeech && string.IsNullOrEmpty(ChannelSession.Settings.MicrosoftAzureSpeechCustomSubscriptionKey); }
        }

        public TextToSpeechActionEditorControlViewModel(TextToSpeechActionModel action)
            : base(action)
        {
            this.AudioDevices.AddRange(ServiceManager.Get<IAudioService>().GetSelectableAudioDevices());
            this.SelectedAudioDevice = (action.OutputDevice != null) ? action.OutputDevice : ServiceManager.Get<IAudioService>().DefaultAudioDevice;

            this.selectedProviderType = action.ProviderType;
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(action.OverlayEndpointID);
            if (this.SelectedOverlayEndpoint == null)
            {
                this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
            }

            this.UpdateTextToSpeechProvider();
            this.SelectedVoice = this.Voices.FirstOrDefault(v => string.Equals(v.ID, action.Voice, StringComparison.OrdinalIgnoreCase));

            this.Text = action.Text;
            this.Volume = action.Volume;
            this.Pitch = action.Pitch;
            this.Rate = action.Rate;
            this.SSML = action.SSML;
            this.WaitForFinish = action.WaitForFinish;
        }

        public TextToSpeechActionEditorControlViewModel()
            : base()
        {
            this.AudioDevices.AddRange(ServiceManager.Get<IAudioService>().GetSelectableAudioDevices());
            this.SelectedAudioDevice = ServiceManager.Get<IAudioService>().DefaultAudioDevice;

            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();

            this.SelectedProviderType = TextToSpeechProviderType.WindowsTextToSpeech;

            this.UpdateTextToSpeechProvider();
        }

        public override Task<Result> Validate()
        {
            if (this.SelectedVoice == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionMissingVoice));
            }

            if (string.IsNullOrEmpty(this.Text))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionMissingText));
            }

            if (this.Volume < this.VolumeMinimum || this.Volume > this.VolumeMaximum)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionInvalidVolume));
            }

            if (this.Pitch < this.PitchMinimum || this.Pitch > this.PitchMaximum)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionInvalidPitch));
            }

            if (this.Rate < this.RateMinimum || this.Rate > this.RateMaximum)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.TextToSpeechActionInvalidRate));
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            string audioDevice = null;
            if (this.UsesAudioDevices && !string.Equals(this.SelectedAudioDevice, ServiceManager.Get<IAudioService>().DefaultAudioDevice))
            {
                audioDevice = this.SelectedAudioDevice;
            }

            return Task.FromResult<ActionModelBase>(new TextToSpeechActionModel(this.SelectedProviderType, audioDevice, this.SelectedOverlayEndpoint.ID,
                this.Text, this.SelectedVoice.ID, this.Volume, this.Pitch, this.Rate, this.SSML, this.WaitForFinish));
        }

        private void UpdateTextToSpeechProvider()
        {
            foreach (ITextToSpeechService service in ServiceManager.GetAll<ITextToSpeechService>())
            {
                if (service.ProviderType == this.SelectedProviderType)
                {
                    this.NotifyPropertyChanged(nameof(this.UsesAudioDevices));
                    this.NotifyPropertyChanged(nameof(this.AudioDeviceServiceConnected));

                    this.NotifyPropertyChanged(nameof(this.UsesOverlay));
                    this.NotifyPropertyChanged(nameof(this.OverlayEnabled));
                    this.NotifyPropertyChanged(nameof(this.OverlayNotEnabled));

                    this.NotifyPropertyChanged(nameof(this.TTSMonsterNotEnabled));

                    string voiceID = (this.SelectedVoice != null) ? this.SelectedVoice.ID : null;
                    this.Voices.ClearAndAddRange(service.GetVoices());
                    if (voiceID != null)
                    {
                        this.SelectedVoice = this.Voices.FirstOrDefault(v => string.Equals(v.ID, voiceID, StringComparison.OrdinalIgnoreCase));
                    }

                    this.VolumeMinimum = service.VolumeMinimum;
                    this.VolumeMaximum = service.VolumeMaximum;
                    this.Volume = this.VolumeChangable ? service.VolumeDefault : 0;
                    this.NotifyPropertyChanged(nameof(this.VolumeMinimum));
                    this.NotifyPropertyChanged(nameof(this.VolumeMaximum));
                    this.NotifyPropertyChanged(nameof(this.VolumeChangable));
                    this.NotifyPropertyChanged(nameof(this.Volume));

                    this.PitchMinimum = service.PitchMinimum;
                    this.PitchMaximum = service.PitchMaximum;
                    this.Pitch = this.PitchChangable ? service.PitchDefault : 0;
                    this.NotifyPropertyChanged(nameof(this.PitchMinimum));
                    this.NotifyPropertyChanged(nameof(this.PitchMaximum));
                    this.NotifyPropertyChanged(nameof(this.PitchChangable));
                    this.NotifyPropertyChanged(nameof(this.Pitch));

                    this.RateMinimum = service.RateMinimum;
                    this.RateMaximum = service.RateMaximum;
                    this.Rate = this.RateChangable ? service.RateDefault : 0;
                    this.NotifyPropertyChanged(nameof(this.RateMinimum));
                    this.NotifyPropertyChanged(nameof(this.RateMaximum));
                    this.NotifyPropertyChanged(nameof(this.RateChangable));
                    this.NotifyPropertyChanged(nameof(this.Rate));
                    break;
                }
            }
        }
    }
}
