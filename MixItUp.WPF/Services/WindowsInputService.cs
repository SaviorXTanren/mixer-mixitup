using MixItUp.Base.Services;
using MixItUp.Base.Util;
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
