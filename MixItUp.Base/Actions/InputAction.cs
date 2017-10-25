using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class InputAction : ActionBase
    {
        [DataMember]
        public List<InputTypeEnum> Inputs { get; set; }

        public InputAction() { }

        public InputAction(IEnumerable<InputTypeEnum> inputs)
            : base(ActionTypeEnum.Input)
        {
            this.Inputs = new List<InputTypeEnum>(inputs);
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            await ChannelSession.Services.InitializeInputService();

            await ChannelSession.Services.InputService.SendInput(this.Inputs);
        }
    }
}
