using MixItUp.Base.Services;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MousePoint
    {
        public int X;
        public int Y;

        public static implicit operator Point(MousePoint point)
        {
            return new Point(point.X, point.Y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct InputContainer
    {
        public uint Type;
        public GenericInput Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct GenericInput
    {
        [FieldOffset(0)]
        public HardwareInput Hardware;
        [FieldOffset(0)]
        public KeyboardInput Keyboard;
        [FieldOffset(0)]
        public MouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HardwareInput
    {
        public uint Msg;
        public ushort ParamL;
        public ushort ParamH;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyboardInput
    {
        public ushort Vk;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    public class WindowsInputService : IInputService
    {
        private const int INPUT_KEYBOARD = 1;

        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int KEYEVENTF_SCANCODE = 0x0008;

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint numberOfInputs, InputContainer[] inputs, int sizeOfInputStructure);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out MousePoint lpPoint);

        public void KeyDown(InputKeyEnum key)
        {
            InputContainer input = new InputContainer { Type = INPUT_KEYBOARD };
            input.Data.Keyboard = new KeyboardInput() { Scan = (ushort)key, Flags = KEYEVENTF_SCANCODE };
            InputContainer[] inputs = new InputContainer[] { input };
            uint result = SendInput(1, inputs, Marshal.SizeOf(typeof(InputContainer)));
        }

        public void KeyUp(InputKeyEnum key)
        {
            InputContainer input = new InputContainer { Type = INPUT_KEYBOARD };
            input.Data.Keyboard = new KeyboardInput() { Scan = (ushort)key, Flags = KEYEVENTF_KEYUP | KEYEVENTF_SCANCODE };
            InputContainer[] inputs = new InputContainer[] { input };
            uint result = SendInput(1, inputs, Marshal.SizeOf(typeof(InputContainer)));
        }

        public async Task KeyClick(InputKeyEnum key)
        {
            this.KeyDown(key);
            await this.WaitForKeyToRegister();
            this.KeyUp(key);
        }

        public void MouseEvent(InputMouseEnum mouse)
        {
            mouse_event((int)mouse, 0, 0, 0, 0);
        }

        public async Task LeftMouseClick()
        {
            this.MouseEvent(InputMouseEnum.LeftDown);
            await this.WaitForKeyToRegister();
            this.MouseEvent(InputMouseEnum.LeftUp);
        }

        public async Task RightMouseClick()
        {
            this.MouseEvent(InputMouseEnum.RightDown);
            await this.WaitForKeyToRegister();
            this.MouseEvent(InputMouseEnum.RightUp);
        }

        public async Task MiddleMouseClick()
        {
            this.MouseEvent(InputMouseEnum.MiddleDown);
            await this.WaitForKeyToRegister();
            this.MouseEvent(InputMouseEnum.MiddleUp);
        }

        public void MoveMouse(int xDelta, int yDelta)
        {
            mouse_event((int)InputMouseEnum.Move, xDelta, yDelta, 0, 0);
        }

        public async Task WaitForKeyToRegister()
        {
            await Task.Delay(100);
        }

        private MousePoint GetMousePosition()
        {
            MousePoint point;
            GetCursorPos(out point);
            return point;
        }
    }
}
