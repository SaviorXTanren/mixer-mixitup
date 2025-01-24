using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

    /// <summary>
    /// OLD REFERENCE: http://download.microsoft.com/download/1/6/1/161ba512-40e2-4cc9-843a-923143f3456c/scancode.doc
    /// </summary>

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    /// </summary>
    public enum VirtualKeyEnum
    {
        Escape = 0x1B,
        Enter = 0x0D,
        Space = 0x20,
        Backspace = 0x08,
        Tab = 0x09,
        CapsLock = 0x14,
        Select = 0x29,
        PrintScreen = 0x2C,

        Minus = 0xBD,
        Equal = 0xBB,
        Comma = 0xBC,
        Period = 0xBE,
        Semicolon = 0xBA,
        Accent = 0xC0,
        Apostrophe = 0xDE,
        ForwardSlash = 0xBF,
        Backslash = 0xDC,
        LeftBracket = 0xDB,
        RightBracket = 0xDD,

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
        F13 = 0x7C,
        F14 = 0x7D,
        F15 = 0x7E,
        F16 = 0x7F,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 0x82,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,

        Digit0 = 0x30,
        Digit1 = 0x31,
        Digit2 = 0x32,
        Digit3 = 0x33,
        Digit4 = 0x34,
        Digit5 = 0x35,
        Digit6 = 0x36,
        Digit7 = 0x37,
        Digit8 = 0x38,
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

        Insert = 0x2D,
        Delete = 0x2E,
        Home = 0x24,
        End = 0x23,
        PageUp = 0x21,
        PageDown = 0x22,

        LeftArrow = 0x25,
        UpArrow = 0x26,
        RightArrow = 0x27,
        DownArrow = 0x28,

        NumPad0 = 0x60,
        NumPad1 = 0x61,
        NumPad2 = 0x62,
        NumPad3 = 0x63,
        NumPad4 = 0x64,
        NumPad5 = 0x65,
        NumPad6 = 0x66,
        NumPad7 = 0x67,
        NumPad8 = 0x68,
        NumPad9 = 0x69,
        NumPadMultiply = 0x6A,
        NumPadAdd = 0x6B,
        NumPadEnter = 0x6C,
        NumPadSubtract = 0x6D,
        NumPadDecimal = 0x6E,
        NumPadDivide = 0x6F,

        LeftWindows = 0x5B,
        RightWindows = 0x5C,
        LeftShift = 0xA0,
        RightShift = 0xA1,
        LeftControl = 0xA2,
        RightControl = 0xA3,
        LeftAlt = 0xA4,
        RightAlt = 0xA5,

        VolumeUp = 0xAF,
        VolumeDown = 0xAE,
        VolumeMute = 0xAD,
        MediaNext = 0xB0,
        MediaPrevious = 0xB1,
        MediaStop = 0xB2,
        MediaPlayPause = 0xB3,
    }

    public enum HotKeyModifiersEnum
    {
        None = 0x0000,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Windows = 0x0008
    }

    [DataContract]
    public class HotKey : IEquatable<HotKey>
    {
        [DataMember]
        public HotKeyModifiersEnum Modifiers { get; set; }
        [DataMember]
        public VirtualKeyEnum VirtualKey { get; set; }

        public HotKey() { }

        public HotKey(HotKeyModifiersEnum modifiers, VirtualKeyEnum key)
        {
            this.Modifiers = modifiers;
            this.VirtualKey = key;
        }

        public override string ToString()
        {
            List<string> keys = new List<string>();
            if (this.Modifiers != HotKeyModifiersEnum.None)
            {
                if (this.Modifiers.HasFlag(HotKeyModifiersEnum.Control)) { keys.Add(EnumLocalizationHelper.GetLocalizedName(HotKeyModifiersEnum.Control)); }
                if (this.Modifiers.HasFlag(HotKeyModifiersEnum.Alt)) { keys.Add(EnumLocalizationHelper.GetLocalizedName(HotKeyModifiersEnum.Alt)); }
                if (this.Modifiers.HasFlag(HotKeyModifiersEnum.Shift)) { keys.Add(EnumLocalizationHelper.GetLocalizedName(HotKeyModifiersEnum.Shift)); }
            }
            keys.Add(EnumLocalizationHelper.GetLocalizedName(this.VirtualKey));
            return string.Join(" + ", keys);
        }

        public override bool Equals(object obj)
        {
            if (obj is HotKey)
            {
                return this.Equals((HotKey)obj);
            }
            return false;
        }

        public bool Equals(HotKey other) { return this.Modifiers == other.Modifiers && this.VirtualKey == other.VirtualKey; }

        public override int GetHashCode() { return this.Modifiers.GetHashCode() + this.VirtualKey.GetHashCode(); }
    }

    [DataContract]
    public class HotKeyConfiguration : HotKey, IEquatable<HotKeyConfiguration>, IComparable, IComparable<HotKeyConfiguration>
    {
        [DataMember]
        public Guid CommandID { get; set; }

        public HotKeyConfiguration() { }

        public HotKeyConfiguration(HotKeyModifiersEnum modifiers, VirtualKeyEnum key, Guid commandID)
            : base(modifiers, key)
        {
            this.CommandID = commandID;
        }

        public override bool Equals(object obj)
        {
            if (obj is HotKeyConfiguration)
            {
                return this.Equals((HotKeyConfiguration)obj);
            }
            return false;
        }

        public bool Equals(HotKeyConfiguration other) { return this.Modifiers == other.Modifiers && this.VirtualKey == other.VirtualKey; }

        public override int GetHashCode() { return base.GetHashCode(); }

        public int CompareTo(object obj)
        {
            if (obj is HotKeyConfiguration)
            {
                return this.CompareTo((HotKeyConfiguration)obj);
            }
            return 0;
        }

        public int CompareTo(HotKeyConfiguration other) { return this.VirtualKey.CompareTo(other.VirtualKey); }
    }

    public interface IInputService
    {
        event EventHandler<HotKey> HotKeyPressed;

        void Initialize(IntPtr windowHandle);

        void KeyDown(VirtualKeyEnum key);
        void KeyUp(VirtualKeyEnum key);
        Task KeyClick(VirtualKeyEnum key);

        void MouseEvent(InputMouseEnum mouse);
        Task LeftMouseClick();
        Task RightMouseClick();
        Task MiddleMouseClick();

        void MoveMouse(int xDelta, int yDelta);

        Task WaitForKeyToRegister();

        bool RegisterHotKey(HotKeyModifiersEnum modifiers, VirtualKeyEnum key);
        bool UnregisterHotKey(HotKeyModifiersEnum modifiers, VirtualKeyEnum key);
    }
}
