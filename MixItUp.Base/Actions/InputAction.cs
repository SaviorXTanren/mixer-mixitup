using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum SimpleInputMouseEnum
    {
        [Name("Left Button")]
        LeftButton,
        [Name("Right Button")]
        RightButton,
        [Name("Middle Button")]
        MiddleButton,
    }

    [DataContract]
    public abstract class InputAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return InputAction.asyncSemaphore; } }

        public InputAction() : base(ActionTypeEnum.Input) { }
    }

    [DataContract]
    public class SimpleInputAction : InputAction
    {
        public InputKeyEnum? Key { get; set; }

        public SimpleInputMouseEnum? Mouse { get; set; }

        public bool Shift { get; set; }
        public bool Control { get; set; }
        public bool Alt { get; set; }

        public SimpleInputAction() { }

        public SimpleInputAction(InputKeyEnum key, bool shift, bool control, bool alt)
            : this(shift, control, alt)
        {
            this.Key = key;
        }

        public SimpleInputAction(SimpleInputMouseEnum mouse, bool shift, bool control, bool alt)
            : this(shift, control, alt)
        {
            this.Mouse = mouse;
        }

        private SimpleInputAction(bool shift, bool control, bool alt)
            : this()
        {
            this.Shift = shift;
            this.Control = control;
            this.Alt = alt;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            await ChannelSession.Services.InitializeInputService();

            await Task.Delay(3000);

            if (this.Shift) { ChannelSession.Services.InputService.KeyDown(InputKeyEnum.Shift); }
            if (this.Control) { ChannelSession.Services.InputService.KeyDown(InputKeyEnum.Control); }
            if (this.Alt) { ChannelSession.Services.InputService.KeyDown(InputKeyEnum.Alt); }

            await ChannelSession.Services.InputService.WaitForKeyToRegister();

            if (this.Key != null)
            {
                await ChannelSession.Services.InputService.KeyClick(this.Key.GetValueOrDefault());
            }
            else if (this.Mouse != null)
            {
                if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.LeftButton)
                {
                    await ChannelSession.Services.InputService.LeftMouseClick();
                }
                else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.RightButton)
                {
                    await ChannelSession.Services.InputService.RightMouseClick();
                }
                else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.MiddleButton)
                {
                    await ChannelSession.Services.InputService.MiddleMouseClick();
                }
            }

            await ChannelSession.Services.InputService.WaitForKeyToRegister();

            if (this.Shift) { ChannelSession.Services.InputService.KeyUp(InputKeyEnum.Shift); }
            if (this.Control) { ChannelSession.Services.InputService.KeyUp(InputKeyEnum.Control); }
            if (this.Alt) { ChannelSession.Services.InputService.KeyUp(InputKeyEnum.Alt); }
        }
    }
}
