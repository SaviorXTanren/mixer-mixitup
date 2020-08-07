using MixItUp.Base.Model.Serial;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class SerialActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SerialActionModel.asyncSemaphore; } }

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

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            SerialDeviceModel serialDevice = ChannelSession.Settings.SerialDevices.FirstOrDefault(sd => sd.PortName.Equals(this.PortName));
            if (serialDevice != null)
            {
                await ChannelSession.Services.SerialService.SendMessage(serialDevice, await this.ReplaceStringWithSpecialModifiers(this.Message, user, platform, arguments, specialIdentifiers));
            }
        }
    }
}
