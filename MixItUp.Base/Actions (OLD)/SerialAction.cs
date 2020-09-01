using MixItUp.Base.Model.Serial;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class SerialAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SerialAction.asyncSemaphore; } }

        [DataMember]
        public string PortName { get; set; }

        [DataMember]
        public string Message { get; set; }

        public SerialAction() : base(ActionTypeEnum.Serial) { }

        public SerialAction(string portName, string message)
            : this()
        {
            this.PortName = portName;
            this.Message = message;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            SerialDeviceModel serialDevice = ChannelSession.Settings.SerialDevices.FirstOrDefault(sd => sd.PortName.Equals(this.PortName));
            if (serialDevice != null)
            {
                await ChannelSession.Services.SerialService.SendMessage(serialDevice, this.Message);
            }
        }
    }
}
