using Mixer.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum InputTypeEnum
    {
        [Name("Left Mouse")]
        LeftMouse = 1,
        [Name("Right Mouse")]
        RightMouse = 2,
        //
        // Summary:
        //     Middle mouse button (three-button mouse) - NOT contiguous with LBUTTON and RBUTTON
        [Name("Middle Mouse")]
        MiddleMouse = 4,
        //
        // Summary:
        //     Windows 2000/XP: X1 mouse button - NOT contiguous with LBUTTON and RBUTTON
        [Name("Side Mouse 1")]
        SideMouse1 = 5,
        //
        // Summary:
        //     Windows 2000/XP: X2 mouse button - NOT contiguous with LBUTTON and RBUTTON
        [Name("Side Mouse 2")]
        SideMouse2 = 6,
        //
        // Summary:
        //     BACKSPACE key
        Backspace = 8,
        //
        // Summary:
        //     TAB key
        Tab = 9,
        //
        // Summary:
        //     CLEAR key
        Clear = 12,
        //
        // Summary:
        //     ENTER key
        Enter = 13,
        //
        // Summary:
        //     SHIFT key
        Shift = 16,
        //
        // Summary:
        //     CTRL key
        Control = 17,
        //
        // Summary:
        //     ALT key
        Alt = 18,
        //
        // Summary:
        //     PAUSE key
        Pause = 19,
        //
        // Summary:
        //     CAPS LOCK key
        [Name("Caps Lock")]
        CapsLock = 20,
        //
        // Summary:
        //     ESC key
        Escape = 27,
        //
        // Summary:
        //     SPACEBAR
        Space = 32,
        //
        // Summary:
        //     PAGE UP key
        [Name("Page Up")]
        PageUp = 33,
        //
        // Summary:
        //     PAGE DOWN key
        [Name("Page Down")]
        PageDown = 34,
        //
        // Summary:
        //     END key
        End = 35,
        //
        // Summary:
        //     HOME key
        Home = 36,
        //
        // Summary:
        //     LEFT ARROW key
        Left = 37,
        //
        // Summary:
        //     UP ARROW key
        Up = 38,
        //
        // Summary:
        //     RIGHT ARROW key
        Right = 39,
        //
        // Summary:
        //     DOWN ARROW key
        Down = 40,
        //
        // Summary:
        //     INS key
        Insert = 45,
        //
        // Summary:
        //     DEL key
        Delete = 46,
        //
        // Summary:
        //     0 key
        [Name("0")]
        Num0 = 48,
        //
        // Summary:
        //     1 key
        [Name("1")]
        Num1 = 49,
        //
        // Summary:
        //     2 key
        [Name("2")]
        Num2 = 50,
        //
        // Summary:
        //     3 key
        [Name("3")]
        Num3 = 51,
        //
        // Summary:
        //     4 key
        [Name("4")]
        Num4 = 52,
        //
        // Summary:
        //     5 key
        [Name("5")]
        Num5 = 53,
        //
        // Summary:
        //     6 key
        [Name("6")]
        Num6 = 54,
        //
        // Summary:
        //     7 key
        [Name("7")]
        Num7 = 55,
        //
        // Summary:
        //     8 key
        [Name("8")]
        Num8 = 56,
        //
        // Summary:
        //     9 key
        [Name("9")]
        Num9 = 57,
        //
        // Summary:
        //     A key
        A = 65,
        //
        // Summary:
        //     B key
        B = 66,
        //
        // Summary:
        //     C key
        C = 67,
        //
        // Summary:
        //     D key
        D = 68,
        //
        // Summary:
        //     E key
        E = 69,
        //
        // Summary:
        //     F key
        F = 70,
        //
        // Summary:
        //     G key
        G = 71,
        //
        // Summary:
        //     H key
        H = 72,
        //
        // Summary:
        //     I key
        I = 73,
        //
        // Summary:
        //     J key
        J = 74,
        //
        // Summary:
        //     K key
        K = 75,
        //
        // Summary:
        //     L key
        L = 76,
        //
        // Summary:
        //     M key
        M = 77,
        //
        // Summary:
        //     N key
        N = 78,
        //
        // Summary:
        //     O key
        O = 79,
        //
        // Summary:
        //     P key
        P = 80,
        //
        // Summary:
        //     Q key
        Q = 81,
        //
        // Summary:
        //     R key
        R = 82,
        //
        // Summary:
        //     S key
        S = 83,
        //
        // Summary:
        //     T key
        T = 84,
        //
        // Summary:
        //     U key
        U = 85,
        //
        // Summary:
        //     V key
        V = 86,
        //
        // Summary:
        //     W key
        W = 87,
        //
        // Summary:
        //     X key
        X = 88,
        //
        // Summary:
        //     Y key
        Y = 89,
        //
        // Summary:
        //     Z key
        Z = 90,
        //
        // Summary:
        //     Left Windows key (Microsoft Natural keyboard)
        [Name("Left Windows")]
        LeftWindows = 91,
        //
        // Summary:
        //     Right Windows key (Natural keyboard)
        [Name("Right Window")]
        RightWindow = 92,
        //
        // Summary:
        //     Numeric keypad 0 key
        [Name("Num Pad 0")]
        NUMPAD0 = 96,
        //
        // Summary:
        //     Numeric keypad 1 key
        [Name("Num Pad 1")]
        NUMPAD1 = 97,
        //
        // Summary:
        //     Numeric keypad 2 key
        [Name("Num Pad 2")]
        NUMPAD2 = 98,
        //
        // Summary:
        //     Numeric keypad 3 key
        [Name("Num Pad 3")]
        NUMPAD3 = 99,
        //
        // Summary:
        //     Numeric keypad 4 key
        [Name("Num Pad 4")]
        NUMPAD4 = 100,
        //
        // Summary:
        //     Numeric keypad 5 key
        [Name("Num Pad 5")]
        NUMPAD5 = 101,
        //
        // Summary:
        //     Numeric keypad 6 key
        [Name("Num Pad 6")]
        NUMPAD6 = 102,
        //
        // Summary:
        //     Numeric keypad 7 key
        [Name("Num Pad 7")]
        NUMPAD7 = 103,
        //
        // Summary:
        //     Numeric keypad 8 key
        [Name("Num Pad 8")]
        NUMPAD8 = 104,
        //
        // Summary:
        //     Numeric keypad 9 key
        [Name("Num Pad 9")]
        NUMPAD9 = 105,
        //
        // Summary:
        //     Multiply key
        Multiply = 106,
        //
        // Summary:
        //     Add key
        Add = 107,
        //
        // Summary:
        //     Separator key
        Separator = 108,
        //
        // Summary:
        //     Subtract key
        Subtract = 109,
        //
        // Summary:
        //     Decimal key
        Decimal = 110,
        //
        // Summary:
        //     Divide key
        Divide = 111,
        //
        // Summary:
        //     F1 key
        F1 = 112,
        //
        // Summary:
        //     F2 key
        F2 = 113,
        //
        // Summary:
        //     F3 key
        F3 = 114,
        //
        // Summary:
        //     F4 key
        F4 = 115,
        //
        // Summary:
        //     F5 key
        F5 = 116,
        //
        // Summary:
        //     F6 key
        F6 = 117,
        //
        // Summary:
        //     F7 key
        F7 = 118,
        //
        // Summary:
        //     F8 key
        F8 = 119,
        //
        // Summary:
        //     F9 key
        F9 = 120,
        //
        // Summary:
        //     F10 key
        F10 = 121,
        //
        // Summary:
        //     F11 key
        F11 = 122,
        //
        // Summary:
        //     F12 key
        F12 = 123,
        //
        // Summary:
        //     NUM LOCK key
        [Name("Num Lock")]
        NumLock = 144,
        //
        // Summary:
        //     Left SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        [Name("Left Shift")]
        LeftShift = 160,
        //
        // Summary:
        //     Right SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        [Name("Right Shift")]
        RightShift = 161,
        //
        // Summary:
        //     Left CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        [Name("Left Control")]
        LeftControl = 162,
        //
        // Summary:
        //     Right CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        [Name("Right Control")]
        RightControl = 163,
        //
        // Summary:
        //     Left MENU key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        [Name("Left Alt")]
        LeftAlt = 164,
        //
        // Summary:
        //     Right MENU key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        [Name("Right Alt")]
        RightAlt = 165,
        //
        // Summary:
        //     Windows 2000/XP: For any country/region, the '+' key
        [Name("Plus")]
        OEM_PLUS = 187,
        //
        // Summary:
        //     Windows 2000/XP: For any country/region, the ',' key
        [Name("Comma")]
        OEM_COMMA = 188,
        //
        // Summary:
        //     Windows 2000/XP: For any country/region, the '-' key
        [Name("Dash")]
        OEM_MINUS = 189,
        //
        // Summary:
        //     Windows 2000/XP: For any country/region, the '.' key
        [Name("Period")]
        OEM_PERIOD = 190,
    }

    public interface IInputService
    {
        Task SendInput(IEnumerable<InputTypeEnum> inputs);

        Task<IEnumerable<InputTypeEnum>> GetCurrentInputs();
    }
}
