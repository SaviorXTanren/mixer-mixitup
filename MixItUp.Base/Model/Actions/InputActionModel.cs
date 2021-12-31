using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum SimpleInputMouseEnum
    {
        LeftButton,
        RightButton,
        MiddleButton,
    }

    public enum InputActionTypeEnum
    {
        Click,
        Press,
        Release,
    }

    [DataContract]
    public class InputActionModel : ActionModelBase
    {
        [DataMember]
        public InputKeyEnum? Key { get; set; }

        [DataMember]
        public SimpleInputMouseEnum? Mouse { get; set; }

        [DataMember]
        public InputActionTypeEnum ActionType { get; set; }

        [DataMember]
        public bool Shift { get; set; }
        [DataMember]
        public bool Control { get; set; }
        [DataMember]
        public bool Alt { get; set; }

        public InputActionModel(InputKeyEnum key, InputActionTypeEnum actionType, bool shift, bool control, bool alt)
            : this(actionType, shift, control, alt)
        {
            this.Key = key;
        }

        public InputActionModel(SimpleInputMouseEnum mouse, InputActionTypeEnum actionType, bool shift, bool control, bool alt)
            : this(actionType, shift, control, alt)
        {
            this.Mouse = mouse;
        }

        private InputActionModel(InputActionTypeEnum actionType, bool shift, bool control, bool alt)
            : base(ActionTypeEnum.Input)
        {
            this.ActionType = actionType;
            this.Shift = shift;
            this.Control = control;
            this.Alt = alt;
        }

        [Obsolete]
        public InputActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.ActionType == InputActionTypeEnum.Press || this.ActionType == InputActionTypeEnum.Click)
            {
                if (this.Shift) { ServiceManager.Get<IInputService>().KeyDown(InputKeyEnum.LeftShift); }
                if (this.Control) { ServiceManager.Get<IInputService>().KeyDown(InputKeyEnum.LeftControl); }
                if (this.Alt) { ServiceManager.Get<IInputService>().KeyDown(InputKeyEnum.LeftAlt); }
            }

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
                    ServiceManager.Get<IInputService>().KeyUp(this.Key.GetValueOrDefault());
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

            if (this.ActionType == InputActionTypeEnum.Release || this.ActionType == InputActionTypeEnum.Click)
            {
                if (this.Shift) { ServiceManager.Get<IInputService>().KeyUp(InputKeyEnum.LeftShift); }
                if (this.Control) { ServiceManager.Get<IInputService>().KeyUp(InputKeyEnum.LeftControl); }
                if (this.Alt) { ServiceManager.Get<IInputService>().KeyUp(InputKeyEnum.LeftAlt); }
            }
        }
    }
}
