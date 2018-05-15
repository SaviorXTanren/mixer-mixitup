using Mixer.Base.Util;
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

    public enum InputKeyEnum
    {
        Backspace = 0x08,
        Tab = 0x09,
        Enter = 0x0D,
        Escape = 0x1B,
        Space = 0x20,

        [Name("0")]
        Digit0 = 0x30,
        [Name("1")]
        Digit1 = 0x31,
        [Name("2")]
        Digit2 = 0x32,
        [Name("3")]
        Digit3 = 0x33,
        [Name("4")]
        Digit4 = 0x34,
        [Name("5")]
        Digit5 = 0x35,
        [Name("6")]
        Digit6 = 0x36,
        [Name("7")]
        Digit7 = 0x37,
        [Name("8")]
        Digit8 = 0x38,
        [Name("9")]
        Digit9 = 0x39,

        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,

        [Name("Num Pad 0")]
        NumPad0 = 0x60,
        [Name("Num Pad 1")]
        NumPad1 = 0x61,
        [Name("Num Pad 2")]
        NumPad2 = 0x62,
        [Name("Num Pad 3")]
        NumPad3 = 0x63,
        [Name("Num Pad 4")]
        NumPad4 = 0x64,
        [Name("Num Pad 5")]
        NumPad5 = 0x65,
        [Name("Num Pad 6")]
        NumPad6 = 0x66,
        [Name("Num Pad 7")]
        NumPad7 = 0x67,
        [Name("Num Pad 8")]
        NumPad8 = 0x68,
        [Name("Num Pad 9")]
        NumPad9 = 0x69,

        Multiply = 0x6A,
        Add = 0x6B,
        Separator = 0x6C,
        Subtract = 0x6D,
        Decimal = 0x6E,
        Divide = 0x6F,

        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,

        Insert = 0x2D,
        Delete = 0x2E,
        Home = 0x24,
        End = 0x23,
        [Name("Page Up")]
        PageUp = 0x21,
        [Name("Page Down")]
        PageDown = 0x22,

        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,

        Shift = 0x10,
        Control = 0x11,
        Alt = 0x12,
        [Name("Left Shift")]
        LeftShift = 0xA0,
        [Name("Right Shift")]
        RightShift = 0xA1,
        [Name("Left Control")]
        LeftControl = 0xA2,
        [Name("Right Control")]
        RightControl = 0xA3,
        [Name("Left Alt")]
        LeftAlt = 0xA4,
        [Name("Right Alt")]
        RightAlt = 0xA5,
        [Name("Left Windows")]
        LeftWindows = 0x5B,
        [Name("Right Windows")]
        RightWindows = 0x5C,
    }

    public interface IInputService
    {
        void KeyDown(InputKeyEnum key);
        void KeyUp(InputKeyEnum key);
        Task KeyClick(InputKeyEnum key);

        void MouseEvent(InputMouseEnum mouse);
        Task LeftMouseClick();
        Task RightMouseClick();
        Task MiddleMouseClick();

        void MoveMouse(int xDelta, int yDelta);

        Task WaitForKeyToRegister();
    }
}
