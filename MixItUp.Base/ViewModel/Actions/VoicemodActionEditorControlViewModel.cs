using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class VoicemodActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Voicemod; } }

        public bool VoicemodConnected { get { return ChannelSession.Services.Voicemod.IsConnected; } }
        public bool VoicemodNotConnected { get { return !this.VoicemodConnected; } }

        public IEnumerable<VoicemodActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<VoicemodActionTypeEnum>(); } }

        public VoicemodActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowStateGrid");
                this.NotifyPropertyChanged("ShowVoiceGrid");
                this.NotifyPropertyChanged("ShowRandomVoiceGrid");
                this.NotifyPropertyChanged("ShowPlaySoundGrid");
            }
        }
        private VoicemodActionTypeEnum selectedActionType;

        public bool ShowStateGrid { get { return this.SelectedActionType == VoicemodActionTypeEnum.VoiceChangerOnOff || this.SelectedActionType == VoicemodActionTypeEnum.BeepSoundOnOff; } }

        public bool State
        {
            get { return this.state; }
            set
            {
                this.state = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool state;

        public bool ShowVoiceGrid { get { return this.SelectedActionType == VoicemodActionTypeEnum.SelectVoice; } }

        public ObservableCollection<VoicemodVoiceModel> Voices { get; set; } = new ObservableCollection<VoicemodVoiceModel>();

        public VoicemodVoiceModel SelectedVoice
        {
            get { return this.selectedVoice; }
            set
            {
                this.selectedVoice = value;
                this.NotifyPropertyChanged();
            }
        }
        private VoicemodVoiceModel selectedVoice;

        public bool ShowRandomVoiceGrid { get { return this.SelectedActionType == VoicemodActionTypeEnum.RandomVoice; } }

        public IEnumerable<VoicemodRandomVoiceType> RandomVoiceTypes { get; set; } = EnumHelper.GetEnumList<VoicemodRandomVoiceType>();

        public VoicemodRandomVoiceType SelectedRandomVoiceType
        {
            get { return this.selectedRandomVoiceType; }
            set
            {
                this.selectedRandomVoiceType = value;
                this.NotifyPropertyChanged();
            }
        }
        private VoicemodRandomVoiceType selectedRandomVoiceType;

        public bool ShowPlaySoundGrid { get { return this.SelectedActionType == VoicemodActionTypeEnum.PlaySound; } }

        public ObservableCollection<VoicemodMemeModel> Sounds { get; set; } = new ObservableCollection<VoicemodMemeModel>();

        public VoicemodMemeModel SelectedSound
        {
            get { return this.selectedSound; }
            set
            {
                this.selectedSound = value;
                this.NotifyPropertyChanged();
            }
        }
        private VoicemodMemeModel selectedSound;

        private string voiceID;
        private string soundFileName;

        public VoicemodActionEditorControlViewModel(VoicemodActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowStateGrid)
            {
                this.State = action.State;
            }
            else if (this.ShowVoiceGrid)
            {
                this.voiceID = action.VoiceID;
            }
            else if (this.ShowRandomVoiceGrid)
            {
                this.SelectedRandomVoiceType = action.RandomVoiceType;
            }
            else if (this.ShowPlaySoundGrid)
            {
                this.soundFileName = action.SoundFileName;
            }
        }

        public VoicemodActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.SelectedActionType == VoicemodActionTypeEnum.SelectVoice)
            {
                if (this.SelectedVoice == null)
                {
                    if (this.VoicemodConnected || string.IsNullOrEmpty(this.voiceID))
                    {
                        return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.VoicemodActionMissingVoice));
                    }
                }
            }
            else if (this.SelectedActionType == VoicemodActionTypeEnum.PlaySound)
            {
                if (this.SelectedSound == null)
                {
                    if (this.VoicemodConnected || string.IsNullOrEmpty(this.soundFileName))
                    {
                        return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.VoicemodActionMissingVoice));
                    }
                }
            }
            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.SelectedActionType == VoicemodActionTypeEnum.VoiceChangerOnOff)
            {
                return Task.FromResult<ActionModelBase>(VoicemodActionModel.CreateForVoiceChangerOnOff(this.State));
            }
            else if (this.SelectedActionType == VoicemodActionTypeEnum.SelectVoice)
            {
                return Task.FromResult<ActionModelBase>(VoicemodActionModel.CreateForSelectVoice(this.SelectedVoice != null ? this.SelectedVoice.voiceID : this.voiceID));
            }
            else if (this.SelectedActionType == VoicemodActionTypeEnum.RandomVoice)
            {
                return Task.FromResult<ActionModelBase>(VoicemodActionModel.CreateForRandomVoice(this.SelectedRandomVoiceType));
            }
            else if (this.SelectedActionType == VoicemodActionTypeEnum.BeepSoundOnOff)
            {
                return Task.FromResult<ActionModelBase>(VoicemodActionModel.CreateForBeepSoundOnOff(this.State));
            }
            else if (this.SelectedActionType == VoicemodActionTypeEnum.PlaySound)
            {
                return Task.FromResult<ActionModelBase>(VoicemodActionModel.CreateForPlaySound(this.SelectedSound != null ? this.SelectedSound.FileName : this.soundFileName));
            }
            else if (this.SelectedActionType == VoicemodActionTypeEnum.StopAllSounds)
            {
                return Task.FromResult<ActionModelBase>(VoicemodActionModel.CreateForStopAllSounds());
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        protected override async Task OnLoadedInternal()
        {
            if (ChannelSession.Settings.EnableVoicemodStudio && !ChannelSession.Services.Voicemod.IsConnected)
            {
                Result result = await ChannelSession.Services.Voicemod.Connect();
                if (!result.Success)
                {
                    return;
                }
            }

            if (this.VoicemodConnected)
            {
                foreach (VoicemodVoiceModel voice in (await ChannelSession.Services.Voicemod.GetVoices()).OrderBy(v => v.friendlyName))
                {
                    this.Voices.Add(voice);
                }

                if (!string.IsNullOrEmpty(this.voiceID))
                {
                    this.SelectedVoice = this.Voices.FirstOrDefault(v => string.Equals(this.voiceID, v.voiceID));
                }

                foreach (VoicemodMemeModel sound in (await ChannelSession.Services.Voicemod.GetMemeSounds()).OrderBy(v => v.Name))
                {
                    this.Sounds.Add(sound);
                }

                if (!string.IsNullOrEmpty(this.soundFileName))
                {
                    this.SelectedSound = this.Sounds.FirstOrDefault(v => string.Equals(this.soundFileName, v.FileName));
                }
            }
            await base.OnLoadedInternal();
        }
    }
}