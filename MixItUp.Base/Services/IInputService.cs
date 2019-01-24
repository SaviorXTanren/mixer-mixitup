using Mixer.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum InputMouseEnum : uint
    {
        Move = 0x0001,

        [Name("Left Mouse Down")]
        LeftDown = 0x00000002,
        [Name("Left Mouse Up")]
        LeftUp = 0x0000004,

        [Name("Right Mouse Down")]
        RightDown = 0x00000008,
        [Name("Right Mouse Up")]
        RightUp = 0x00000010,

        [Name("Middle Mouse Down")]
        MiddleDown = 0x00000020,
        [Name("Middle Mouse Up")]
        MiddleUp = 0x00000040,

        [Name("Extra Mouse Down")]
        XDown = 0x00000080,
        [Name("Extra Mouse Up")]
        XUp = 0x00000100,

        Wheel = 0x00000800,

        Absolute = 0x00008000,
    }

    public enum InputMouseXButtonsEnum : uint
    {
        XButton1 = 0x00000001,
        XButton2 = 0x00000002,
    }

    public enum InputKeyEnum : ushort
    {
        [Name("0")]
        Digit0 = 0x0B,
        [Name("1")]
        Digit1 = 0x02,
        [Name("2")]
        Digit2 = 0x03,
        [Name("3")]
        Digit3 = 0x04,
        [Name("4")]
        Digit4 = 0x05,
        [Name("5")]
        Digit5 = 0x06,
        [Name("6")]
        Digit6 = 0x07,
        [Name("7")]
        Digit7 = 0x08,
        [Name("8")]
        Digit8 = 0x09,
        [Name("9")]
        Digit9 = 0x0A,

        A = 0x1E,
        B = 0x30,
        C = 0x2E,
        D = 0x20,
        E = 0x12,
        F = 0x21,
        G = 0x22,
        H = 0x23,
        I = 0x17,
        J = 0x24,
        K = 0x25,
        L = 0x26,
        M = 0x32,
        N = 0x31,
        O = 0x18,
        P = 0x19,
        Q = 0x10,
        R = 0x13,
        S = 0x1F,
        T = 0x14,
        U = 0x16,
        V = 0x2F,
        W = 0x11,
        X = 0x2D,
        Y = 0x15,
        Z = 0x2C,

        Semicolon = 0x27,
        Apostrophe = 0x28,
        Accent = 0x29,
        Backslash = 0x2B,
        Comma = 0x33,
        Period = 0x34,
        Slash = 0x35,
        Escape = 0x01,
        Minus = 0x0C,
        Equals = 0x0D,
        Backspace = 0x0E,
        Tab = 0x0F,
        Space = 0x39,
        Capital = 0x3A,
        Enter = 0x1C,

        F1 = 0x3B,
        F2 = 0x3C,
        F3 = 0x3D,
        F4 = 0x3E,
        F5 = 0x3F,
        F6 = 0x40,
        F7 = 0x41,
        F8 = 0x42,
        F9 = 0x43,
        F10 = 0x44,

        Insert = 0xD2,
        Delete = 0xD3,
        Home = 0xC7,
        End = 0xCF,
        [Name("Page Up")]
        PageUp = 0xC9,
        [Name("Page Down")]
        PageDown = 0xD1,

        [Name("Num Pad 0")]
        NumPad0 = 0x52,
        [Name("Num Pad 1")]
        NumPad1 = 0x4F,
        [Name("Num Pad 2")]
        NumPad2 = 0x50,
        [Name("Num Pad 3")]
        NumPad3 = 0x51,
        [Name("Num Pad 4")]
        NumPad4 = 0x4B,
        [Name("Num Pad 5")]
        NumPad5 = 0x4C,
        [Name("Num Pad 6")]
        NumPad6 = 0x4D,
        [Name("Num Pad 7")]
        NumPad7 = 0x47,
        [Name("Num Pad 8")]
        NumPad8 = 0x48,
        [Name("Num Pad 9")]
        NumPad9 = 0x49,
        [Name("Num Pad Add")]
        NumPadAdd = 0x4E,
        [Name("Num Pad Subtract")]
        NumPadSubtract = 0x4A,
        [Name("Num Pad Multiply")]
        NumPadMultiply = 0x37,
        [Name("Num Pad Divide")]
        NumPadDivide = 0xB5,
        [Name("Num Pad Decimal")]
        NumPadDecimal = 0x53,
        [Name("Num Pad Enter")]
        NumPadEnter = 0x9C,

        [Name("Left Bracket")]
        LeftBracket = 0x1A,
        [Name("Right Bracket")]
        RightBacket = 0x1B,
        [Name("Left Control")]
        LeftControl = 0x1D,
        [Name("Right Control")]
        RightControl = 0x9D,
        [Name("Left Shift")]
        LeftShift = 0x2A,
        [Name("Right Shift")]
        RightShift = 0x36,
        [Name("Left Alt")]
        LeftAlt = 0x38,
        [Name("Right Alt")]
        RightAlt = 0xB8,
        [Name("Left Windows")]
        LeftWindows = 0xDB,
        [Name("Right Windows")]
        RightWindows = 0xDC,

        [Name("Up Arrow")]
        Up = 0xC8,
        [Name("Down Arrow")]
        Down = 0xD0,
        [Name("Left Arrow")]
        Right = 0xCD,
        [Name("Right Arrow")]
        Left = 0xCB,
    }

    public enum HotKeyModifiersEnum
    {
        None = 0x0000,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Windows = 0x0008
    }

    public class HotKey
    {
        public HotKeyModifiersEnum Modifiers { get; set; }
        public InputKeyEnum Key { get; set; }

        public HotKey(HotKeyModifiersEnum modifiers, InputKeyEnum key)
        {
            this.Modifiers = modifiers;
            this.Key = key;
        }
    }

    public interface IInputService
    {
        event EventHandler<HotKey> HotKeyPressed;

        void Initialize(IntPtr windowHandle);

        void KeyDown(InputKeyEnum key);
        void KeyUp(InputKeyEnum key);
        Task KeyClick(InputKeyEnum key);

        void MouseEvent(InputMouseEnum mouse);
        Task LeftMouseClick();
        Task RightMouseClick();
        Task MiddleMouseClick();

        void MoveMouse(int xDelta, int yDelta);

        Task WaitForKeyToRegister();

        bool RegisterHotKey(HotKeyModifiersEnum modifiers, InputKeyEnum key);
        bool UnregisterHotKey(HotKeyModifiersEnum modifiers, InputKeyEnum key);
    }
}
