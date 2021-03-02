using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum SimpleInputMouseEnum
    {
        LeftButton,
        RightButton,
        MiddleButton,
    }

    [Obsolete]
    public enum InputActionTypeEnum
    {
        Click,
        Press,
        Release,
    }

    [Obsolete]
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
            if (this.Shift) { ServiceManager.Get<IInputService>().KeyDown(InputKeyEnum.LeftShift); }
            if (this.Control) { ServiceManager.Get<IInputService>().KeyDown(InputKeyEnum.LeftControl); }
            if (this.Alt) { ServiceManager.Get<IInputService>().KeyDown(InputKeyEnum.LeftAlt); }

            await ServiceManager.Get<IInputService>().WaitForKeyToRegister();

            if (this.Key != null)
            {
                if (this.ActionType == InputActionTypeEnum.Click)
                {
                    await ServiceManager.Get<IInputService>().KeyClick(this.Key.GetValueOrDefault());
                }
                else if (this.ActionType == InputActionTypeEnum.Press)
                {
                    ServiceManager.Get<IInputService>().KeyDown(this.Key.GetValueOrDefault());
                }
                else if (this.ActionType == InputActionTypeEnum.Release)
                {
                    ServiceManager.Get<IInputService>().KeyDown(this.Key.GetValueOrDefault());
                }
            }
            else if (this.Mouse != null)
            {
                if (this.ActionType == InputActionTypeEnum.Click)
                {
                    if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.LeftButton)
                    {
                        await ServiceManager.Get<IInputService>().LeftMouseClick();
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.RightButton)
                    {
                        await ServiceManager.Get<IInputService>().RightMouseClick();
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.MiddleButton)
                    {
                        await ServiceManager.Get<IInputService>().MiddleMouseClick();
                    }
                }
                else if (this.ActionType == InputActionTypeEnum.Press)
                {
                    if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.LeftButton)
                    {
                        ServiceManager.Get<IInputService>().MouseEvent(InputMouseEnum.LeftDown);
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.RightButton)
                    {
                        ServiceManager.Get<IInputService>().MouseEvent(InputMouseEnum.RightDown);
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.MiddleButton)
                    {
                        ServiceManager.Get<IInputService>().MouseEvent(InputMouseEnum.MiddleDown);
                    }
                }
                else if (this.ActionType == InputActionTypeEnum.Release)
                {
                    if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.LeftButton)
                    {
                        ServiceManager.Get<IInputService>().MouseEvent(InputMouseEnum.LeftUp);
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.RightButton)
                    {
                        ServiceManager.Get<IInputService>().MouseEvent(InputMouseEnum.RightUp);
                    }
                    else if (this.Mouse.GetValueOrDefault() == SimpleInputMouseEnum.MiddleButton)
                    {
                        ServiceManager.Get<IInputService>().MouseEvent(InputMouseEnum.MiddleUp);
                    }
                }
            }

            await ServiceManager.Get<IInputService>().WaitForKeyToRegister();

            if (this.Shift) { ServiceManager.Get<IInputService>().KeyUp(InputKeyEnum.LeftShift); }
            if (this.Control) { ServiceManager.Get<IInputService>().KeyUp(InputKeyEnum.LeftControl); }
            if (this.Alt) { ServiceManager.Get<IInputService>().KeyUp(InputKeyEnum.LeftAlt); }
        }
    }
}
