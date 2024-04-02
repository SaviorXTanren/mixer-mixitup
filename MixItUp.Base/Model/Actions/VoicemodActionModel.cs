using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
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
        HearMyselfOnOff,
        MuteOnOff
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

        public static VoicemodActionModel CreateForHearMyselfOnOff(bool state) { return new VoicemodActionModel(VoicemodActionTypeEnum.HearMyselfOnOff) { State = state }; }

        public static VoicemodActionModel CreateForMuteOnOff(bool state) { return new VoicemodActionModel(VoicemodActionTypeEnum.MuteOnOff) { State = state }; }

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

        [Obsolete]
        public VoicemodActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.EnableVoicemodStudio && !ServiceManager.Get<IVoicemodService>().IsConnected)
            {
                await ServiceManager.Get<IVoicemodService>().Connect();
            }

            if (ServiceManager.Get<IVoicemodService>().IsConnected)
            {
                if (this.ActionType == VoicemodActionTypeEnum.VoiceChangerOnOff)
                {
                    await ServiceManager.Get<IVoicemodService>().VoiceChangerOnOff(this.State);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.SelectVoice)
                {
                    await ServiceManager.Get<IVoicemodService>().SelectVoice(this.VoiceID);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.RandomVoice)
                {
                    await ServiceManager.Get<IVoicemodService>().RandomVoice(this.RandomVoiceType);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.BeepSoundOnOff)
                {
                    await ServiceManager.Get<IVoicemodService>().BeepSoundOnOff(this.State);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.PlaySound)
                {
                    await ServiceManager.Get<IVoicemodService>().PlayMemeSound(this.SoundFileName);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.StopAllSounds)
                {
                    await ServiceManager.Get<IVoicemodService>().StopAllMemeSounds();
                }
                else if (this.ActionType == VoicemodActionTypeEnum.HearMyselfOnOff)
                {
                    await ServiceManager.Get<IVoicemodService>().HearMyselfOnOff(this.State);
                }
                else if (this.ActionType == VoicemodActionTypeEnum.MuteOnOff)
                {
                    await ServiceManager.Get<IVoicemodService>().MuteOnOff(this.State);
                }
            }
        }
    }
}