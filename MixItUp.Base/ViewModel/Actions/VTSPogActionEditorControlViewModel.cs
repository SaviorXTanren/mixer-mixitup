using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class VTSPogActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.VTSPog; } }

        public bool VTSPogConnected { get { return ServiceManager.Get<VTSPogService>().IsConnected; } }
        public bool VTSPogNotConnected { get { return !this.VTSPogConnected; } }

        public IEnumerable<VTSPogActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<VTSPogActionTypeEnum>(); } }

        public VTSPogActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowTextToSpeechGrid));
                this.NotifyPropertyChanged(nameof(this.ShowAITextToSpeechGrid));
                this.NotifyPropertyChanged(nameof(this.ShowPlayAudioFileGrid));
                this.NotifyPropertyChanged(nameof(this.ShowEnableDisableTextToSpeechQueueGrid));
            }
        }
        private VTSPogActionTypeEnum selectedActionType;

        public string TextToSpeechText
        {
            get { return this.textToSpeechText; }
            set
            {
                this.textToSpeechText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string textToSpeechText;

        public bool ShowTextToSpeechGrid { get { return this.SelectedActionType == VTSPogActionTypeEnum.TextToSpeech; } }

        public IEnumerable<VTSPogTextToSpeechProvider> TextToSpeechProviders { get; set; } = EnumHelper.GetEnumList<VTSPogTextToSpeechProvider>();

        public VTSPogTextToSpeechProvider SelectedTextToSpeechProvider
        {
            get { return this.selectedTextToSpeechProvider; }
            set
            {
                this.selectedTextToSpeechProvider = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.CanTextToSpeechVoiceBeSpecified));
                if (!this.CanTextToSpeechVoiceBeSpecified)
                {
                    this.TextToSpeechVoice = string.Empty;
                }
            }
        }
        private VTSPogTextToSpeechProvider selectedTextToSpeechProvider;

        public bool CanTextToSpeechVoiceBeSpecified { get { return this.SelectedTextToSpeechProvider != VTSPogTextToSpeechProvider.Random; } }

        public string TextToSpeechVoice
        {
            get { return this.textToSpeechVoice; }
            set
            {
                this.textToSpeechVoice = value;
                this.NotifyPropertyChanged();
            }
        }
        private string textToSpeechVoice;

        public string TextToSpeechCharacterLimit
        {
            get { return (this.textToSpeechCharacterLimit > 0) ? this.textToSpeechCharacterLimit.ToString() : string.Empty; }
            set
            {
                this.textToSpeechCharacterLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int textToSpeechCharacterLimit;

        public bool ShowAITextToSpeechGrid { get { return this.SelectedActionType == VTSPogActionTypeEnum.AITextToSpeech; } }

        public IEnumerable<VTSPogAITextToSpeechPromptTypeEnum> AITextToSpeechPromptTypes { get; set; } = EnumHelper.GetEnumList<VTSPogAITextToSpeechPromptTypeEnum>();

        public VTSPogAITextToSpeechPromptTypeEnum SelectedAITextToSpeechPromptType
        {
            get { return this.selectedAITextToSpeechPromptType; }
            set
            {
                this.selectedAITextToSpeechPromptType = value;
                this.NotifyPropertyChanged();
            }
        }
        private VTSPogAITextToSpeechPromptTypeEnum selectedAITextToSpeechPromptType = VTSPogAITextToSpeechPromptTypeEnum.Default;

        public bool AITextToSpeechStoreInMemory
        {
            get { return this.aiTextToSpeechStoreInMemory; }
            set
            {
                this.aiTextToSpeechStoreInMemory = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool aiTextToSpeechStoreInMemory;

        public bool ShowPlayAudioFileGrid { get { return this.SelectedActionType == VTSPogActionTypeEnum.PlayAudioFile; } }

        public string AudioFilePath
        {
            get { return this.audioFilePath; }
            set
            {
                this.audioFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string audioFilePath;

        public IEnumerable<VTSPogAudioFileOutputType> AudioOutputTypes { get; set; } = EnumHelper.GetEnumList<VTSPogAudioFileOutputType>();

        public VTSPogAudioFileOutputType SelectedAudioOutputType
        {
            get { return this.selectedAudioOutputType; }
            set
            {
                this.selectedAudioOutputType = value;
                this.NotifyPropertyChanged();
            }
        }
        private VTSPogAudioFileOutputType selectedAudioOutputType;

        public bool ShowEnableDisableTextToSpeechQueueGrid { get { return this.SelectedActionType == VTSPogActionTypeEnum.EnableDisableTextToSpeechQueue; } }

        public bool EnableDisableTextToSpeechQueue
        {
            get { return this.enableDisableTextToSpeechQueue; }
            set
            {
                this.enableDisableTextToSpeechQueue = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool enableDisableTextToSpeechQueue;

        public VTSPogActionEditorControlViewModel(VTSPogActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowTextToSpeechGrid)
            {
                this.TextToSpeechText = action.TextToSpeechText;
                this.textToSpeechCharacterLimit = action.TextToSpeechCharacterLimit;
                this.SelectedTextToSpeechProvider = action.TextToSpeechProvider;
                this.TextToSpeechVoice = action.TextToSpeechVoice;
            }
            else if (this.ShowAITextToSpeechGrid)
            {
                this.TextToSpeechText = action.TextToSpeechText;
                this.SelectedAITextToSpeechPromptType = action.AITextToSpeechPromptType;
                this.AITextToSpeechStoreInMemory = action.AITextToSpeechStoreInMemory;
            }
            else if (this.ShowPlayAudioFileGrid)
            {
                this.AudioFilePath = action.AudioFilePath;
                this.SelectedAudioOutputType = action.AudioOutputType;
            }
            else if (this.ShowEnableDisableTextToSpeechQueueGrid)
            {
                this.EnableDisableTextToSpeechQueue = action.EnableDisableTextToSpeechQueue;
            }
        }

        public VTSPogActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.ShowTextToSpeechGrid)
            {
                if (string.IsNullOrEmpty(this.TextToSpeechText))
                {
                    return Task.FromResult<Result>(new Result(Resources.VTSPogActionMissingText));
                }
            }
            else if (this.ShowAITextToSpeechGrid)
            {
                if (string.IsNullOrEmpty(this.TextToSpeechText))
                {
                    return Task.FromResult<Result>(new Result(Resources.VTSPogActionMissingText));
                }
            }
            else if (this.ShowPlayAudioFileGrid)
            {
                if (string.IsNullOrEmpty(this.AudioFilePath))
                {
                    return Task.FromResult<Result>(new Result(Resources.VTSPogActionMissingAudioFile));
                }
            }
            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowTextToSpeechGrid)
            {
                return Task.FromResult<ActionModelBase>(VTSPogActionModel.CreateForTextToSpeech(this.TextToSpeechText, this.textToSpeechCharacterLimit, this.SelectedTextToSpeechProvider, this.TextToSpeechVoice));
            }
            else if (this.ShowAITextToSpeechGrid)
            {
                return Task.FromResult<ActionModelBase>(VTSPogActionModel.CreateForAITextToSpeech(this.TextToSpeechText, this.SelectedAITextToSpeechPromptType, this.AITextToSpeechStoreInMemory));
            }
            else if (this.ShowPlayAudioFileGrid)
            {
                return Task.FromResult<ActionModelBase>(VTSPogActionModel.CreateForPlayAudioFile(this.AudioFilePath, this.SelectedAudioOutputType));
            }
            else if (this.ShowEnableDisableTextToSpeechQueueGrid)
            {
                return Task.FromResult<ActionModelBase>(VTSPogActionModel.CreateForEnableDisableTextToSpeechQueue(this.EnableDisableTextToSpeechQueue));
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new VTSPogActionModel(this.SelectedActionType));
            }
        }
    }
}