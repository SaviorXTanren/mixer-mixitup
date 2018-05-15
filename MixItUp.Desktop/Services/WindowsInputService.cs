using MixItUp.Base.Services;
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

    public class WindowsInputService : IInputService
    {
        private const int KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out MousePoint lpPoint);

        public void KeyDown(InputKeyEnum key)
        {
            keybd_event((byte)key, 0, 0, 0);
        }

        public void KeyUp(InputKeyEnum key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, 0);
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
