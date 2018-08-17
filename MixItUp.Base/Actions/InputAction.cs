using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
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

    public enum InputActionTypeEnum
    {
        Click,
        Press,
        Release,
    }

    [DataContract]
    public class InputAction : ActionBase
    {
        [JsonProperty]
        public InputKeyEnum? Key { get; set; }

        [JsonProperty]
        public SimpleInputMouseEnum? Mouse { get; set; }

        [JsonProperty]
        public InputActionTypeEnum ActionType { get; set; }

        [JsonProperty]
        public bool Shift { get; set; }
        [JsonProperty]
        public bool Control { get; set; }
        [JsonProperty]
        public bool Alt { get; set; }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return InputAction.asyncSemaphore; } }

        public InputAction() : base(ActionTypeEnum.Input) { }

        public InputAction(InputKeyEnum key, InputActionTypeEnum actionType, bool shift, bool control, bool alt)
            : this(actionType, shift, control, alt)
        {
            this.Key = key;
        }

        public InputAction(SimpleInputMouseEnum mouse, InputActionTypeEnum actionType, bool shift, bool control, bool alt)
            : this(actionType, shift, control, alt)
        {
            this.Mouse = mouse;
        }

        private InputAction(InputActionTypeEnum actionType, bool shift, bool control, bool alt)
            : this()
        {
            this.ActionType = actionType;
            this.Shift = shift;
            this.Control = control;
            this.Alt = alt;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.Shift) { ChannelSession.Services.InputService.KeyDown(InputKeyEnum.LeftShift); }
            if (this.Control) { ChannelSession.Services.InputService.KeyDown(InputKeyEnum.LeftControl); }
            if (this.Alt) { ChannelSession.Services.InputService.KeyDown(InputKeyEnum.LeftAlt); }

            await ChannelSession.Services.InputService.WaitForKeyToRegister();

            if (this.Key != null)
            {
                if (this.ActionType == InputActionTypeEnum.Click)
                {
                    await ChannelSession.Services.InputService.KeyClick(this.Key.GetValueOrDefault());
                }
                else if (this.ActionType == InputActionTypeEnum.Press)
                {
                    ChannelSession.Services.InputService.KeyDown(this.Key.GetValueOrDefault());
                }
                else if (this.ActionType == InputActionTypeEnum.Release)
                {
                    ChannelSession.Services.InputService.KeyDown(this.Key.GetValueOrDefault());
                }
            }
            else if (this.Mouse != null)
            {
                if (this.ActionType == InputActionTypeEnum.Click)
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
                else if (this.ActionType == InputActionTypeEnum.Press)
                {
                    if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.LeftButton)
                    {
                        ChannelSession.Services.InputService.MouseEvent(InputMouseEnum.LeftDown);
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.RightButton)
                    {
                        ChannelSession.Services.InputService.MouseEvent(InputMouseEnum.RightDown);
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.MiddleButton)
                    {
                        ChannelSession.Services.InputService.MouseEvent(InputMouseEnum.MiddleDown);
                    }
                }
                else if (this.ActionType == InputActionTypeEnum.Release)
                {
                    if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.LeftButton)
                    {
                        ChannelSession.Services.InputService.MouseEvent(InputMouseEnum.LeftUp);
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.RightButton)
                    {
                        ChannelSession.Services.InputService.MouseEvent(InputMouseEnum.RightUp);
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.MiddleButton)
                    {
                        ChannelSession.Services.InputService.MouseEvent(InputMouseEnum.MiddleUp);
                    }
                }
            }

            await ChannelSession.Services.InputService.WaitForKeyToRegister();

            if (this.Shift) { ChannelSession.Services.InputService.KeyUp(InputKeyEnum.LeftShift); }
            if (this.Control) { ChannelSession.Services.InputService.KeyUp(InputKeyEnum.LeftControl); }
            if (this.Alt) { ChannelSession.Services.InputService.KeyUp(InputKeyEnum.LeftAlt); }
        }
    }
}
