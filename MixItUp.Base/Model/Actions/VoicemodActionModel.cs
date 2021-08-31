using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum VoicemodActionTypeEnum
    {
        VoiceChangerOnOff,
        SelectVoice,
        RandomVoice,
        BeepSoundOnOff,
        PlaySound,
        StopAllSounds,
    }

    [DataContract]
    public class VoicemodActionModel : ActionModelBase
    {
        public static VoicemodActionModel CreateForVoiceChangerOnOff(bool state) { return new VoicemodActionModel(VoicemodActionTypeEnum.VoiceChangerOnOff) { State = state }; }

        public static VoicemodActionModel CreateForSelectVoice(string voiceID) { return new VoicemodActionModel(VoicemodActionTypeEnum.SelectVoice) { VoiceID = voiceID }; }

        public static VoicemodActionModel CreateForRandomVoice(VoicemodRandomVoiceType randomVoiceType) { return new VoicemodActionModel(VoicemodActionTypeEnum.RandomVoice) { RandomVoiceType = randomVoiceType }; }

        public static VoicemodActionModel CreateForBeepSoundOnOff(bool state) { return new VoicemodActionModel(VoicemodActionTypeEnum.BeepSoundOnOff) { State = state }; }

        public static VoicemodActionModel CreateForPlaySound(string soundFileName) { return new VoicemodActionModel(VoicemodActionTypeEnum.PlaySound) { SoundFileName = soundFileName }; }

        public static VoicemodActionModel CreateForStopAllSounds() { return new VoicemodActionModel(VoicemodActionTypeEnum.StopAllSounds); }

        [DataMember]
        public VoicemodActionTypeEnum ActionType { get; set; }

        [DataMember]
        public bool State { get; set; }

        [DataMember]
        public string VoiceID { get; set; }
        
        [DataMember]
        public VoicemodRandomVoiceType RandomVoiceType { get; set; }

        [DataMember]
        public string SoundFileName { get; set; }

        public VoicemodActionModel(VoicemodActionTypeEnum actionType)
            : base(ActionTypeEnum.Voicemod)
        {
            this.ActionType = actionType;
        }

        private VoicemodActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.EnableVoicemodStudio && !ChannelSession.Services.Voicemod.IsConnected)
            {
                Result result = await ChannelSession.Services.Voicemod.Connect();
                if (!result.Success)
                {
                    return;
                }
            }

            if (ChannelSession.Services.Voicemod.IsConnected)
            {
                if (this.ActionType == VoicemodActionTypeEnum.VoiceChangerOnOff)
                {
                    await ChannelSession.Services.Voicemod.VoiceChangerOnOff(this.State);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.SelectVoice)
                {
                    await ChannelSession.Services.Voicemod.SelectVoice(this.VoiceID);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.RandomVoice)
                {
                    await ChannelSession.Services.Voicemod.RandomVoice(this.RandomVoiceType);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.BeepSoundOnOff)
                {
                    await ChannelSession.Services.Voicemod.BeepSoundOnOff(this.State);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.PlaySound)
                {
                    await ChannelSession.Services.Voicemod.PlayMemeSound(this.SoundFileName);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.StopAllSounds)
                {
                    await ChannelSession.Services.Voicemod.StopAllMemeSounds();
                }
            }
        }
    }
}