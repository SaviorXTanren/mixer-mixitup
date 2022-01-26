using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace MixItUp.WPF.Services
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

    public class WindowsInputService : IMessageFilter, IInputService, IDisposable
    {
        private const int INPUT_KEYBOARD = 1;

        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int KEYEVENTF_SCANCODE = 0x0008;

        private const int WINDOWS_MESSAGE_HOTKEY = 0x0312;

        private const int MAPVK_VK_TO_VSC_EX = 4;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs, InputContainer[] inputs, int sizeOfInputStructure);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetCursorPos(out MousePoint lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyA(uint uCode, uint uMapType);

        private static HashSet<VirtualKeyEnum> extendedVirtualKeys = new HashSet<VirtualKeyEnum>()
        {
            VirtualKeyEnum.UpArrow, VirtualKeyEnum.DownArrow, VirtualKeyEnum.LeftArrow, VirtualKeyEnum.RightArrow,
            VirtualKeyEnum.Insert, VirtualKeyEnum.Delete, VirtualKeyEnum.Home, VirtualKeyEnum.End,
            VirtualKeyEnum.PageUp, VirtualKeyEnum.PageDown
        };

        private static HashSet<VirtualKeyEnum> keybdEventVirtualKeys = new HashSet<VirtualKeyEnum>()
        {
            VirtualKeyEnum.VolumeDown, VirtualKeyEnum.VolumeUp, VirtualKeyEnum.VolumeMute,
            VirtualKeyEnum.MediaPrevious, VirtualKeyEnum.MediaNext, VirtualKeyEnum.MediaPlayPause, VirtualKeyEnum.MediaStop
        };

        public event EventHandler<HotKey> HotKeyPressed;

        private IntPtr windowHandle;
        private HwndSource windowSource;

        private HashSet<int> registeredHotKeys = new HashSet<int>();

        public void Initialize(IntPtr windowHandle)
        {
            this.windowHandle = windowHandle;
            this.windowSource = HwndSource.FromHwnd(this.windowHandle);
            this.windowSource.AddHook(HwndHook);

            System.Windows.Forms.Application.AddMessageFilter(this);
        }

        public void KeyDown(VirtualKeyEnum key)
        {
            if (keybdEventVirtualKeys.Contains(key))
            {
                keybd_event((byte)key, (byte)this.ConvertVirtualKeyToScanCode(key), KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
            }
            else
            {
                InputContainer input = new InputContainer { Type = INPUT_KEYBOARD };
                input.Data.Keyboard = new KeyboardInput() { Scan = (ushort)this.ConvertVirtualKeyToScanCode(key), Flags = KEYEVENTF_SCANCODE };
                if (extendedVirtualKeys.Contains(key))
                {
                    input.Data.Keyboard.Flags |= KEYEVENTF_EXTENDEDKEY;
                }
                InputContainer[] inputs = new InputContainer[] { input };
                uint result = SendInput(1, inputs, Marshal.SizeOf(typeof(InputContainer)));
            }
        }

        public void KeyUp(VirtualKeyEnum key)
        {
            if (keybdEventVirtualKeys.Contains(key))
            {
                keybd_event((byte)key, (byte)this.ConvertVirtualKeyToScanCode(key), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, IntPtr.Zero);
            }
            else
            {
                InputContainer input = new InputContainer { Type = INPUT_KEYBOARD };
                input.Data.Keyboard = new KeyboardInput() { Scan = (ushort)this.ConvertVirtualKeyToScanCode(key), Flags = KEYEVENTF_KEYUP | KEYEVENTF_SCANCODE };
                if (extendedVirtualKeys.Contains(key))
                {
                    input.Data.Keyboard.Flags |= KEYEVENTF_EXTENDEDKEY;
                }
                InputContainer[] inputs = new InputContainer[] { input };
                uint result = SendInput(1, inputs, Marshal.SizeOf(typeof(InputContainer)));
            }
        }

        public async Task KeyClick(VirtualKeyEnum key)
        {
            this.KeyDown(key);
            await this.WaitForKeyToRegister();
            this.KeyUp(key);
        }

        public void MouseEvent(InputMouseEnum mouse)
        {
            mouse_event((int)mouse, 0, 0, 0, (IntPtr)0);
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
            mouse_event((int)InputMouseEnum.Move, xDelta, yDelta, 0, (IntPtr)0);
        }

        public async Task WaitForKeyToRegister()
        {
            await Task.Delay(250);
        }

        public bool RegisterHotKey(HotKeyModifiersEnum modifiers, VirtualKeyEnum key)
        {
            if (this.windowHandle != null)
            {
                int hotKeyID = this.GetHotKeyID(modifiers, key);
                if (!this.registeredHotKeys.Contains(hotKeyID))
                {
                    this.registeredHotKeys.Add(hotKeyID);
                    return RegisterHotKey(this.windowHandle, hotKeyID, (uint)modifiers, (uint)key);
                }
            }
            return false;
        }

        public bool UnregisterHotKey(HotKeyModifiersEnum modifiers, VirtualKeyEnum key)
        {
            if (this.windowHandle != null)
            {
                int hotKeyID = this.GetHotKeyID(modifiers, key);
                if (this.registeredHotKeys.Contains(hotKeyID))
                {
                    this.registeredHotKeys.Remove(hotKeyID);
                    return UnregisterHotKey(this.windowHandle, hotKeyID);
                }
            }
            return false;
        }

        [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
        public bool PreFilterMessage(ref Message message)
        {
            if (message.Msg == WINDOWS_MESSAGE_HOTKEY && message.HWnd == this.windowHandle)
            {
                int hotKeyID = message.WParam.ToInt32();
                if (this.registeredHotKeys.Contains(hotKeyID))
                {
                    return true;
                }
            }
            return false;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        public VirtualKeyEnum ConvertOldKeyEnum(InputKeyEnum key)
        {
            switch (key)
            {
                case InputKeyEnum.Digit0: return VirtualKeyEnum.Digit0;
                case InputKeyEnum.Digit1: return VirtualKeyEnum.Digit1;
                case InputKeyEnum.Digit2: return VirtualKeyEnum.Digit2;
                case InputKeyEnum.Digit3: return VirtualKeyEnum.Digit3;
                case InputKeyEnum.Digit4: return VirtualKeyEnum.Digit4;
                case InputKeyEnum.Digit5: return VirtualKeyEnum.Digit5;
                case InputKeyEnum.Digit6: return VirtualKeyEnum.Digit6;
                case InputKeyEnum.Digit7: return VirtualKeyEnum.Digit7;
                case InputKeyEnum.Digit8: return VirtualKeyEnum.Digit8;
                case InputKeyEnum.Digit9: return VirtualKeyEnum.Digit9;

                case InputKeyEnum.A: return VirtualKeyEnum.A;
                case InputKeyEnum.B: return VirtualKeyEnum.B;
                case InputKeyEnum.C: return VirtualKeyEnum.C;
                case InputKeyEnum.D: return VirtualKeyEnum.D;
                case InputKeyEnum.E: return VirtualKeyEnum.E;
                case InputKeyEnum.F: return VirtualKeyEnum.F;
                case InputKeyEnum.G: return VirtualKeyEnum.G;
                case InputKeyEnum.H: return VirtualKeyEnum.H;
                case InputKeyEnum.I: return VirtualKeyEnum.I;
                case InputKeyEnum.J: return VirtualKeyEnum.J;
                case InputKeyEnum.K: return VirtualKeyEnum.K;
                case InputKeyEnum.L: return VirtualKeyEnum.L;
                case InputKeyEnum.M: return VirtualKeyEnum.M;
                case InputKeyEnum.N: return VirtualKeyEnum.N;
                case InputKeyEnum.O: return VirtualKeyEnum.O;
                case InputKeyEnum.P: return VirtualKeyEnum.P;
                case InputKeyEnum.Q: return VirtualKeyEnum.Q;
                case InputKeyEnum.R: return VirtualKeyEnum.R;
                case InputKeyEnum.S: return VirtualKeyEnum.S;
                case InputKeyEnum.T: return VirtualKeyEnum.T;
                case InputKeyEnum.U: return VirtualKeyEnum.U;
                case InputKeyEnum.V: return VirtualKeyEnum.V;
                case InputKeyEnum.W: return VirtualKeyEnum.W;
                case InputKeyEnum.X: return VirtualKeyEnum.X;
                case InputKeyEnum.Y: return VirtualKeyEnum.Y;
                case InputKeyEnum.Z: return VirtualKeyEnum.Z;

                case InputKeyEnum.Semicolon: return VirtualKeyEnum.Semicolon;
                case InputKeyEnum.Apostrophe: return VirtualKeyEnum.Apostrophe;
                case InputKeyEnum.Accent: return VirtualKeyEnum.Accent;
                case InputKeyEnum.Backslash: return VirtualKeyEnum.Backslash;
                case InputKeyEnum.Comma: return VirtualKeyEnum.Comma;
                case InputKeyEnum.Period: return VirtualKeyEnum.Period;
                case InputKeyEnum.Forwardslash: return VirtualKeyEnum.ForwardSlash;
                case InputKeyEnum.Escape: return VirtualKeyEnum.Escape;
                case InputKeyEnum.Minus: return VirtualKeyEnum.Minus;
                case InputKeyEnum.Equal: return VirtualKeyEnum.Equal;
                case InputKeyEnum.Backspace: return VirtualKeyEnum.Backspace;
                case InputKeyEnum.Tab: return VirtualKeyEnum.Tab;
                case InputKeyEnum.Space: return VirtualKeyEnum.Space;
                case InputKeyEnum.Capslock: return VirtualKeyEnum.CapsLock;
                case InputKeyEnum.Enter: return VirtualKeyEnum.Enter;

                case InputKeyEnum.F1: return VirtualKeyEnum.F1;
                case InputKeyEnum.F2: return VirtualKeyEnum.F2;
                case InputKeyEnum.F3: return VirtualKeyEnum.F3;
                case InputKeyEnum.F4: return VirtualKeyEnum.F4;
                case InputKeyEnum.F5: return VirtualKeyEnum.F5;
                case InputKeyEnum.F6: return VirtualKeyEnum.F6;
                case InputKeyEnum.F7: return VirtualKeyEnum.F7;
                case InputKeyEnum.F8: return VirtualKeyEnum.F8;
                case InputKeyEnum.F9: return VirtualKeyEnum.F9;
                case InputKeyEnum.F10: return VirtualKeyEnum.F10;
                case InputKeyEnum.F11: return VirtualKeyEnum.F11;
                case InputKeyEnum.F12: return VirtualKeyEnum.F12;
                case InputKeyEnum.F13: return VirtualKeyEnum.F13;
                case InputKeyEnum.F14: return VirtualKeyEnum.F14;
                case InputKeyEnum.F15: return VirtualKeyEnum.F15;
                case InputKeyEnum.F16: return VirtualKeyEnum.F16;
                case InputKeyEnum.F17: return VirtualKeyEnum.F17;
                case InputKeyEnum.F18: return VirtualKeyEnum.F18;
                case InputKeyEnum.F19: return VirtualKeyEnum.F19;
                case InputKeyEnum.F20: return VirtualKeyEnum.F20;
                case InputKeyEnum.F21: return VirtualKeyEnum.F21;
                case InputKeyEnum.F22: return VirtualKeyEnum.F22;
                case InputKeyEnum.F23: return VirtualKeyEnum.F23;
                case InputKeyEnum.F24: return VirtualKeyEnum.F24;

                case InputKeyEnum.Insert: return VirtualKeyEnum.Insert;
                case InputKeyEnum.Delete: return VirtualKeyEnum.Delete;
                case InputKeyEnum.Home: return VirtualKeyEnum.Home;
                case InputKeyEnum.End: return VirtualKeyEnum.End;
                case InputKeyEnum.PageUp: return VirtualKeyEnum.PageUp;
                case InputKeyEnum.PageDown: return VirtualKeyEnum.PageDown;

                case InputKeyEnum.NumPad0: return VirtualKeyEnum.NumPad0;
                case InputKeyEnum.NumPad1: return VirtualKeyEnum.NumPad1;
                case InputKeyEnum.NumPad2: return VirtualKeyEnum.NumPad2;
                case InputKeyEnum.NumPad3: return VirtualKeyEnum.NumPad3;
                case InputKeyEnum.NumPad4: return VirtualKeyEnum.NumPad4;
                case InputKeyEnum.NumPad5: return VirtualKeyEnum.NumPad5;
                case InputKeyEnum.NumPad6: return VirtualKeyEnum.NumPad6;
                case InputKeyEnum.NumPad7: return VirtualKeyEnum.NumPad7;
                case InputKeyEnum.NumPad8: return VirtualKeyEnum.NumPad8;
                case InputKeyEnum.NumPad9: return VirtualKeyEnum.NumPad9;
                case InputKeyEnum.NumPadAdd: return VirtualKeyEnum.NumPadAdd;
                case InputKeyEnum.NumPadSubtract: return VirtualKeyEnum.NumPadSubtract;
                case InputKeyEnum.NumPadMultiply: return VirtualKeyEnum.NumPadMultiply;
                case InputKeyEnum.NumPadDivide: return VirtualKeyEnum.NumPadDivide;
                case InputKeyEnum.NumPadDecimal: return VirtualKeyEnum.NumPadDecimal;
                case InputKeyEnum.NumPadEnter: return VirtualKeyEnum.NumPadEnter;

                case InputKeyEnum.LeftBracket: return VirtualKeyEnum.LeftBracket;
                case InputKeyEnum.RightBacket: return VirtualKeyEnum.RightBracket;
                case InputKeyEnum.LeftControl: return VirtualKeyEnum.LeftControl;
                case InputKeyEnum.RightControl: return VirtualKeyEnum.RightControl;
                case InputKeyEnum.LeftShift: return VirtualKeyEnum.LeftShift;
                case InputKeyEnum.RightShift: return VirtualKeyEnum.RightShift;
                case InputKeyEnum.LeftAlt: return VirtualKeyEnum.LeftAlt;
                case InputKeyEnum.RightAlt: return VirtualKeyEnum.RightAlt;
                case InputKeyEnum.LeftWindows: return VirtualKeyEnum.LeftWindows;
                case InputKeyEnum.RightWindows: return VirtualKeyEnum.RightWindows;

                case InputKeyEnum.UpArrow: return VirtualKeyEnum.UpArrow;
                case InputKeyEnum.DownArrow: return VirtualKeyEnum.DownArrow;
                case InputKeyEnum.RightArrow: return VirtualKeyEnum.RightArrow;
                case InputKeyEnum.LeftArrow: return VirtualKeyEnum.LeftArrow;
            }
            return 0;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                if (msg == WINDOWS_MESSAGE_HOTKEY)
                {
                    int hotKeyID = wParam.ToInt32();
                    if (this.registeredHotKeys.Contains(hotKeyID))
                    {
                        VirtualKeyEnum virtualKey = (VirtualKeyEnum)(((int)lParam >> 16) & 0xFFFF);
                        HotKeyModifiersEnum modifiers = (HotKeyModifiersEnum)((int)lParam & 0xFFFF);

                        this.HotKeyPressed?.Invoke(this, new HotKey(modifiers, virtualKey));
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return IntPtr.Zero;
        }

        private MousePoint GetMousePosition()
        {
            MousePoint point;
            GetCursorPos(out point);
            return point;
        }

        private int GetHotKeyID(HotKeyModifiersEnum modifiers, VirtualKeyEnum key) { return (((int)key) << 16) | (int)modifiers; }

        private uint ConvertVirtualKeyToScanCode(VirtualKeyEnum virtualKey) { return MapVirtualKeyA((uint)virtualKey, MAPVK_VK_TO_VSC_EX); }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    if (this.windowSource != null)
                    {
                        this.windowSource.RemoveHook(HwndHook);
                    }

                    if (this.windowHandle != null)
                    {
                        foreach (int hotKeyID in this.registeredHotKeys)
                        {
                            UnregisterHotKey(this.windowHandle, hotKeyID);
                        }
                        this.registeredHotKeys.Clear();

                        System.Windows.Forms.Application.RemoveMessageFilter(this);
                    }
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
