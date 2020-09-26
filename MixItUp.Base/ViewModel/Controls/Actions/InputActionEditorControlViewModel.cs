using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public enum InputActionDeviceTypeEnum
    {
        Keyboard,
        Mouse
    }

    public class InputActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Input; } }

        public IEnumerable<InputActionDeviceTypeEnum> DeviceTypes { get { return EnumHelper.GetEnumList<InputActionDeviceTypeEnum>(); } }

        public InputActionDeviceTypeEnum SelectedDeviceType
        {
            get { return this.selectedDeviceType; }
            set
            {
                this.selectedDeviceType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowKeyboard");
                this.NotifyPropertyChanged("ShowMouse");
            }
        }
        private InputActionDeviceTypeEnum selectedDeviceType;

        public IEnumerable<InputKeyEnum> KeyboardKeys { get { return EnumHelper.GetEnumList<InputKeyEnum>(); } }

        public InputKeyEnum SelectedKeyboardKey
        {
            get { return this.selectedKeyboardKey; }
            set
            {
                this.selectedKeyboardKey = value;
                this.NotifyPropertyChanged();
            }
        }
        private InputKeyEnum selectedKeyboardKey;

        public bool ShowKeyboard { get { return this.SelectedDeviceType == InputActionDeviceTypeEnum.Keyboard; } }

        public IEnumerable<SimpleInputMouseEnum> MouseButtons { get { return EnumHelper.GetEnumList<SimpleInputMouseEnum>(); } }

        public SimpleInputMouseEnum SelectedMouseButton
        {
            get { return this.selectedMouseButton; }
            set
            {
                this.selectedMouseButton = value;
                this.NotifyPropertyChanged();
            }
        }
        private SimpleInputMouseEnum selectedMouseButton;

        public bool ShowMouse { get { return this.SelectedDeviceType == InputActionDeviceTypeEnum.Mouse; } }

        public IEnumerable<InputActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<InputActionTypeEnum>(); } }

        public InputActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
            }
        }
        private InputActionTypeEnum selectedActionType;

        public bool Shift
        {
            get { return this.shift; }
            set
            {
                this.shift = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool shift;

        public bool Control
        {
            get { return this.control; }
            set
            {
                this.control = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool control;

        public bool Alt
        {
            get { return this.alt; }
            set
            {
                this.alt = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool alt;

        public InputActionEditorControlViewModel(InputActionModel action)
            : base(action)
        {
            if (action.Key != null)
            {
                this.SelectedDeviceType = InputActionDeviceTypeEnum.Keyboard;
                this.SelectedKeyboardKey = action.Key.GetValueOrDefault();
            }
            else if (action.Mouse != null)
            {
                this.SelectedDeviceType = InputActionDeviceTypeEnum.Mouse;
                this.SelectedMouseButton = action.Mouse.GetValueOrDefault();
            }
            this.SelectedActionType = action.ActionType;
            this.Shift = action.Shift;
            this.Control = action.Control;
            this.Alt = action.Alt;
        }

        public InputActionEditorControlViewModel() : base() { }

        public override Task<ActionModelBase> GetAction()
        {
            if (this.SelectedDeviceType == InputActionDeviceTypeEnum.Keyboard)
            {
                return Task.FromResult<ActionModelBase>(new InputActionModel(this.SelectedKeyboardKey, this.SelectedActionType, this.Shift, this.Control, this.Alt));
            }
            else if (this.SelectedDeviceType == InputActionDeviceTypeEnum.Mouse)
            {
                return Task.FromResult<ActionModelBase>(new InputActionModel(this.SelectedMouseButton, this.SelectedActionType, this.Shift, this.Control, this.Alt));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
