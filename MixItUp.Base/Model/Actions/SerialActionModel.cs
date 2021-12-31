using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Serial;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
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

        [Obsolete]
        public SerialActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            SerialDeviceModel serialDevice = ChannelSession.Settings.SerialDevices.FirstOrDefault(sd => sd.PortName.Equals(this.PortName));
            if (serialDevice != null)
            {
                await ServiceManager.Get<SerialService>().SendMessage(serialDevice, await ReplaceStringWithSpecialModifiers(this.Message, parameters));
            }
        }
    }
}
