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
        EnableDisableTextToSpeechQueue,
        ToggleTextToSpeechQueue,
        SkipCurrentAudio,
    }

    [DataContract]
    public class VTSPogActionModel : ActionModelBase
    {
        public static VTSPogActionModel CreateForPlayAudioFile(string audioFilePath, string petName) { return new VTSPogActionModel(VTSPogActionTypeEnum.PlayAudioFile) { AudioFilePath = audioFilePath, PetName = petName }; }

        public static VTSPogActionModel CreateForEnableDisableTextToSpeechQueue(bool state) { return new VTSPogActionModel(VTSPogActionTypeEnum.EnableDisableTextToSpeechQueue) { EnableDisableTextToSpeechQueue = state }; }

        [DataMember]
        public VTSPogActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string TextToSpeechText { get; set; }

        [DataMember]
        public string PetName { get; set; }

        [DataMember]
        public string AudioFilePath { get; set; }

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
                    //await ServiceManager.Get<VTSPogService>().TextToSpeech(text, parameters.User, );
                }
                else if (this.ActionType == VTSPogActionTypeEnum.AITextToSpeech)
                {
                    string text = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.TextToSpeechText, parameters);
                    //await ServiceManager.Get<VTSPogService>().AITextToSpeech(text, parameters.User, );
                }
                else if (this.ActionType == VTSPogActionTypeEnum.PlayAudioFile)
                {
                    string audioFilePath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.AudioFilePath, parameters);
                    string petName = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.PetName, parameters);
                    await ServiceManager.Get<VTSPogService>().PlayAudioFile(audioFilePath, petName);
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