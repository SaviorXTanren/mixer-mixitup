using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class InputAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return InputAction.asyncSemaphore; } }

        [DataMember]
        public List<InputTypeEnum> Inputs { get; set; }

        public InputAction() : base(ActionTypeEnum.Input) { }

        public InputAction(IEnumerable<InputTypeEnum> inputs)
            : this()
        {
            this.Inputs = new List<InputTypeEnum>(inputs);
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            await ChannelSession.Services.InitializeInputService();

            await ChannelSession.Services.InputService.SendInput(this.Inputs);
        }
    }
}
