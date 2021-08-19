using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum VoicemodActionTypeEnum
    {
        VoiceChangerOnOff,
    }

    [DataContract]
    public class VoicemodActionModel : ActionModelBase
    {
        public static VoicemodActionModel CreateForVoiceChangerOnOff(bool state) { return new VoicemodActionModel(VoicemodActionTypeEnum.VoiceChangerOnOff) { State = state }; }

        [DataMember]
        public VoicemodActionTypeEnum ActionType { get; set; }

        [DataMember]
        public bool State { get; set; }

        [DataMember]
        public string HotKeyID { get; set; }

        public VoicemodActionModel(VoicemodActionTypeEnum actionType)
            : base(ActionTypeEnum.Voicemod)
        {
            this.ActionType = actionType;
        }

        private VoicemodActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Services.VTubeStudio.IsConnected)
            {
                if (this.ActionType == VoicemodActionTypeEnum.VoiceChangerOnOff)
                {
                    await ChannelSession.Services.Voicemod.VoiceChangerOnOff(this.State);
                }
            }
        }
    }
}