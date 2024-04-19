using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum VTSPogActionTypeEnum
    {
        TextToSpeech,
        AITextToSpeech,
        PlayAudioFile,
        [Obsolete]
        EnableDisableTextToSpeechQueue,
        [Obsolete]
        ToggleTextToSpeechQueue,
        [Obsolete]
        SkipCurrentAudio,
    }

    [DataContract]
    public class VTSPogActionModel : ActionModelBase
    {
        public static VTSPogActionModel CreateForTextToSpeech(string text, int characterLimit, VTSPogTextToSpeechProvider provider, string voice)
        {
            return new VTSPogActionModel(VTSPogActionTypeEnum.TextToSpeech)
            {
                TextToSpeechText = text,
                TextToSpeechCharacterLimit = characterLimit,
                TextToSpeechProvider = provider,
                TextToSpeechVoice = voice,
            };
        }

        public static VTSPogActionModel CreateForAITextToSpeech(string text, VTSPogAITextToSpeechPromptTypeEnum type, bool storeInMemory)
        {
            return new VTSPogActionModel(VTSPogActionTypeEnum.AITextToSpeech)
            {
                TextToSpeechText = text,
                AITextToSpeechPromptType = type,
                AITextToSpeechStoreInMemory = storeInMemory,
            };
        }

        public static VTSPogActionModel CreateForPlayAudioFile(string audioFilePath, VTSPogAudioFileOutputType audioOutputType)
        {
            return new VTSPogActionModel(VTSPogActionTypeEnum.PlayAudioFile)
            {
                AudioFilePath = audioFilePath,
                AudioOutputType = audioOutputType,
            };
        }

        public static VTSPogActionModel CreateForEnableDisableTextToSpeechQueue(bool state)
        {
            return new VTSPogActionModel(VTSPogActionTypeEnum.EnableDisableTextToSpeechQueue)
            {
                EnableDisableTextToSpeechQueue = state
            };
        }

        [DataMember]
        public VTSPogActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string TextToSpeechText { get; set; }

        [DataMember]
        public int TextToSpeechCharacterLimit { get; set; }
        [DataMember]
        public VTSPogTextToSpeechProvider TextToSpeechProvider { get; set; }
        [DataMember]
        public string TextToSpeechVoice { get; set; }

        [DataMember]
        public bool AITextToSpeechStoreInMemory { get; set; }
        [DataMember]
        public VTSPogAITextToSpeechPromptTypeEnum AITextToSpeechPromptType { get; set; }

        [DataMember]
        public string AudioFilePath { get; set; }
        [DataMember]
        public VTSPogAudioFileOutputType AudioOutputType { get; set; }

        [DataMember]
        public bool EnableDisableTextToSpeechQueue { get; set; }

        public VTSPogActionModel(VTSPogActionTypeEnum actionType)
            : base(ActionTypeEnum.VTSPog)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public VTSPogActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.VTSPogEnabled && !ServiceManager.Get<VTSPogService>().IsConnected)
            {
                Result result = await ServiceManager.Get<VTSPogService>().Connect();
                if (!result.Success)
                {
                    return;
                }
            }

            if (ServiceManager.Get<VTSPogService>().IsConnected)
            {
                if (this.ActionType == VTSPogActionTypeEnum.TextToSpeech)
                {
                    string text = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.TextToSpeechText, parameters);
                    string voice = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.TextToSpeechVoice, parameters);
                    string provider = null;
                    switch (this.TextToSpeechProvider)
                    {
                        case VTSPogTextToSpeechProvider.AmazonPolly: provider = "amazon"; break;
                        case VTSPogTextToSpeechProvider.TTSMonster: provider = "monster"; break;
                        case VTSPogTextToSpeechProvider.StreamElements: provider = "SE"; break;
                        case VTSPogTextToSpeechProvider.Animalese: provider = "animalese"; break;
                        case VTSPogTextToSpeechProvider.WindowsTextToSpeech: provider = "windows"; break;
                        case VTSPogTextToSpeechProvider.TikTokTTS: provider = "tiktok"; break;
                        case VTSPogTextToSpeechProvider.SAMSoftwareAutomaticMouth: provider = "SAM"; break;
                        case VTSPogTextToSpeechProvider.Elevenlabs: provider = "elevenlabs"; break;
                        case VTSPogTextToSpeechProvider.Random: provider = "random"; break;
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        await ServiceManager.Get<VTSPogService>().TextToSpeech(text, parameters.User, this.TextToSpeechCharacterLimit, provider, voice);
                    }
                }
                else if (this.ActionType == VTSPogActionTypeEnum.AITextToSpeech)
                {
                    string text = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.TextToSpeechText, parameters);
                    if (!string.IsNullOrEmpty(text))
                    {
                        await ServiceManager.Get<VTSPogService>().AITextToSpeech(text, parameters.User, this.AITextToSpeechPromptType, this.AITextToSpeechStoreInMemory);
                    }
                }
                else if (this.ActionType == VTSPogActionTypeEnum.PlayAudioFile)
                {
                    string audioFilePath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.AudioFilePath, parameters);
                    await ServiceManager.Get<VTSPogService>().PlayAudioFile(audioFilePath, this.AudioOutputType);
                }
                else if (this.ActionType == VTSPogActionTypeEnum.EnableDisableTextToSpeechQueue)
                {
                    if (this.EnableDisableTextToSpeechQueue)
                    {
                        await ServiceManager.Get<VTSPogService>().EnableTTSQueue();
                    }
                    else
                    {
                        await ServiceManager.Get<VTSPogService>().DisableTTSQueue();
                    }
                }
                else if (this.ActionType == VTSPogActionTypeEnum.ToggleTextToSpeechQueue)
                {
                    await ServiceManager.Get<VTSPogService>().ToggleTTSQueue();
                }
                else if (this.ActionType == VTSPogActionTypeEnum.SkipCurrentAudio)
                {
                    await ServiceManager.Get<VTSPogService>().SkipAudio();
                }
            }
        }
    }
}