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

        internal SerialActionModel(MixItUp.Base.Actions.SerialAction action)
            : base(ActionTypeEnum.Serial)
        {
            this.PortName = action.PortName;
            this.Message = action.Message;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            SerialDeviceModel serialDevice = ChannelSession.Settings.SerialDevices.FirstOrDefault(sd => sd.PortName.Equals(this.PortName));
            if (serialDevice != null)
            {
                await ChannelSession.Services.SerialService.SendMessage(serialDevice, await this.ReplaceStringWithSpecialModifiers(this.Message, parameters));
            }
        }
    }
}
