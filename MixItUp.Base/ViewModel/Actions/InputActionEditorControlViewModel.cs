using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public enum InputActionDeviceTypeEnum
    {
        Keyboard,
        Mouse
    }

    public class InputActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        private static WindowModel noneWindow = new WindowModel() { Title = "None" };
        private static IEnumerable<VirtualKeyEnum> keyboardKeys = EnumHelper.GetEnumList<VirtualKeyEnum>().OrderBy(k => EnumLocalizationHelper.GetLocalizedName(k));

        public override ActionTypeEnum Type { get { return ActionTypeEnum.Input; } }

        public IEnumerable<InputActionDeviceTypeEnum> DeviceTypes { get { return EnumHelper.GetEnumList<InputActionDeviceTypeEnum>(); } }

        public InputActionDeviceTypeEnum SelectedDeviceType
        {
            get { return this.selectedDeviceType; }
            set
            {
                this.selectedDeviceType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowKeyboard));
                this.NotifyPropertyChanged(nameof(this.ShowMouse));
                this.NotifyPropertyChanged(nameof(this.ShowWindows));
            }
        }
        private InputActionDeviceTypeEnum selectedDeviceType;

        public bool ShowWindows { get { return this.SelectedDeviceType == InputActionDeviceTypeEnum.Keyboard; } }

        public ObservableCollection<WindowModel> Windows { get; private set; } = new ObservableCollection<WindowModel>();

        public WindowModel SelectedWindow
        {
            get { return this.selectedWindow; }
            set
            {
                this.selectedWindow = value;
                this.NotifyPropertyChanged();
            }
        }
        private WindowModel selectedWindow;

        public IEnumerable<VirtualKeyEnum> KeyboardKeys { get { return keyboardKeys; } }

        public VirtualKeyEnum SelectedKeyboardKey
        {
            get { return this.selectedKeyboardKey; }
            set
            {
                this.selectedKeyboardKey = value;
                this.NotifyPropertyChanged();
            }
        }
        private VirtualKeyEnum selectedKeyboardKey;

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

        private WindowModel oldWindow;

        public InputActionEditorControlViewModel(InputActionModel action)
            : base(action)
        {
            if (action.VirtualKey != null)
            {
                this.SelectedDeviceType = InputActionDeviceTypeEnum.Keyboard;
                this.SelectedKeyboardKey = action.VirtualKey.GetValueOrDefault();
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

            this.oldWindow = action.Window;
        }

        public InputActionEditorControlViewModel() : base() { }

        protected override async Task OnOpenInternal()
        {
            this.Windows.Add(noneWindow);
            foreach (WindowModel window in ServiceManager.Get<IInputService>().GetWindows())
            {
                this.Windows.Add(window);
            }

            if (this.oldWindow != null)
            {
                this.SelectedWindow = this.Windows.FirstOrDefault(w => string.Equals(w.Title, this.oldWindow.Title));
                if (this.SelectedWindow == null)
                {
                    this.SelectedWindow = this.Windows.FirstOrDefault(w => string.Equals(w.Executable, this.oldWindow.Executable));
                }
            }
            
            if (this.SelectedWindow == null)
            {
                this.SelectedWindow = noneWindow;
            }

            await base.OnOpenInternal();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.SelectedDeviceType == InputActionDeviceTypeEnum.Keyboard)
            {
                WindowModel window = null;
                if (this.SelectedWindow != noneWindow)
                {
                    window = this.SelectedWindow;
                }
                return Task.FromResult<ActionModelBase>(new InputActionModel(this.SelectedKeyboardKey, this.SelectedActionType, this.Shift, this.Control, this.Alt, window));
            }
            else if (this.SelectedDeviceType == InputActionDeviceTypeEnum.Mouse)
            {
                return Task.FromResult<ActionModelBase>(new InputActionModel(this.SelectedMouseButton, this.SelectedActionType, this.Shift, this.Control, this.Alt));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
