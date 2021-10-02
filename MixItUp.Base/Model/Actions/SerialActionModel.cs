using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Serial;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class SerialActionModel : ActionModelBase
    {
        [DataMember]
        public string PortName { get; set; }

        [DataMember]
        public string Message { get; set; }

        public SerialActionModel(string portName, string message)
            : base(ActionTypeEnum.Serial)
        {
            this.PortName = portName;
            this.Message = message;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal SerialActionModel(MixItUp.Base.Actions.SerialAction action)
            : base(ActionTypeEnum.Serial)
        {
            this.PortName = action.PortName;
            this.Message = action.Message;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private SerialActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            SerialDeviceModel serialDevice = ChannelSession.Settings.SerialDevices.FirstOrDefault(sd => sd.PortName.Equals(this.PortName));
            if (serialDevice != null)
            {
                await ChannelSession.Services.SerialService.SendMessage(serialDevice, await ReplaceStringWithSpecialModifiers(this.Message, parameters));
            }
        }
    }
}
