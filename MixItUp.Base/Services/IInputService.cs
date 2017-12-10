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
        MBUTTON = 4,
        //
        // Summary:
        //     Windows 2000/XP: X1 mouse button - NOT contiguous with LBUTTON and RBUTTON
        [Name("Side Mouse 1")]
        XBUTTON1 = 5,
        //
        // Summary:
        //     Windows 2000/XP: X2 mouse button - NOT contiguous with LBUTTON and RBUTTON
        [Name("Side Mouse 2")]
        XBUTTON2 = 6,
        //
        // Summary:
        //     BACKSPACE key
        BACK = 8,
        //
        // Summary:
        //     TAB key
        TAB = 9,
        //
        // Summary:
        //     CLEAR key
        CLEAR = 12,
        //
        // Summary:
        //     ENTER key
        RETURN = 13,
        //
        // Summary:
        //     SHIFT key
        SHIFT = 16,
        //
        // Summary:
        //     CTRL key
        CONTROL = 17,
        //
        // Summary:
        //     ALT key
        MENU = 18,
        //
        // Summary:
        //     PAUSE key
        PAUSE = 19,
        //
        // Summary:
        //     CAPS LOCK key
        CAPITAL = 20,
        //
        // Summary:
        //     ESC key
        ESCAPE = 27,
        //
        // Summary:
        //     SPACEBAR
        SPACE = 32,
        //
        // Summary:
        //     PAGE UP key
        PRIOR = 33,
        //
        // Summary:
        //     PAGE DOWN key
        NEXT = 34,
        //
        // Summary:
        //     END key
        END = 35,
        //
        // Summary:
        //     HOME key
        HOME = 36,
        //
        // Summary:
        //     LEFT ARROW key
        LEFT = 37,
        //
        // Summary:
        //     UP ARROW key
        UP = 38,
        //
        // Summary:
        //     RIGHT ARROW key
        RIGHT = 39,
        //
        // Summary:
        //     DOWN ARROW key
        DOWN = 40,
        //
        // Summary:
        //     INS key
        INSERT = 45,
        //
        // Summary:
        //     DEL key
        DELETE = 46,
        //
        // Summary:
        //     0 key
        NUM0 = 48,
        //
        // Summary:
        //     1 key
        NUM1 = 49,
        //
        // Summary:
        //     2 key
        NUM2 = 50,
        //
        // Summary:
        //     3 key
        NUM3 = 51,
        //
        // Summary:
        //     4 key
        NUM4 = 52,
        //
        // Summary:
        //     5 key
        NUM5 = 53,
        //
        // Summary:
        //     6 key
        NUM6 = 54,
        //
        // Summary:
        //     7 key
        NUM7 = 55,
        //
        // Summary:
        //     8 key
        NUM8 = 56,
        //
        // Summary:
        //     9 key
        NUM9 = 57,
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
        LWIN = 91,
        //
        // Summary:
        //     Right Windows key (Natural keyboard)
        RWIN = 92,
        //
        // Summary:
        //     Numeric keypad 0 key
        NUMPAD0 = 96,
        //
        // Summary:
        //     Numeric keypad 1 key
        NUMPAD1 = 97,
        //
        // Summary:
        //     Numeric keypad 2 key
        NUMPAD2 = 98,
        //
        // Summary:
        //     Numeric keypad 3 key
        NUMPAD3 = 99,
        //
        // Summary:
        //     Numeric keypad 4 key
        NUMPAD4 = 100,
        //
        // Summary:
        //     Numeric keypad 5 key
        NUMPAD5 = 101,
        //
        // Summary:
        //     Numeric keypad 6 key
        NUMPAD6 = 102,
        //
        // Summary:
        //     Numeric keypad 7 key
        NUMPAD7 = 103,
        //
        // Summary:
        //     Numeric keypad 8 key
        NUMPAD8 = 104,
        //
        // Summary:
        //     Numeric keypad 9 key
        NUMPAD9 = 105,
        //
        // Summary:
        //     Multiply key
        MULTIPLY = 106,
        //
        // Summary:
        //     Add key
        ADD = 107,
        //
        // Summary:
        //     Separator key
        SEPARATOR = 108,
        //
        // Summary:
        //     Subtract key
        SUBTRACT = 109,
        //
        // Summary:
        //     Decimal key
        DECIMAL = 110,
        //
        // Summary:
        //     Divide key
        DIVIDE = 111,
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
        NUMLOCK = 144,
        //
        // Summary:
        //     Left SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        LSHIFT = 160,
        //
        // Summary:
        //     Right SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        RSHIFT = 161,
        //
        // Summary:
        //     Left CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        LCONTROL = 162,
        //
        // Summary:
        //     Right CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
        RCONTROL = 163,
        //
        // Summary:
        //     Windows 2000/XP: For any country/region, the '+' key
        OEM_PLUS = 187,
        //
        // Summary:
        //     Windows 2000/XP: For any country/region, the ',' key
        OEM_COMMA = 188,
        //
        // Summary:
        //     Windows 2000/XP: For any country/region, the '-' key
        OEM_MINUS = 189,
        //
        // Summary:
        //     Windows 2000/XP: For any country/region, the '.' key
        OEM_PERIOD = 190,
    }

    public interface IInputService
    {
        Task SendInput(IEnumerable<InputTypeEnum> inputs);
    }
}
