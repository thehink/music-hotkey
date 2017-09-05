using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlayMusic.Keybinder
{
    class KeyboardEvent
    {
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public static void SendKey(Keys key, KeyState state)
        {
            uint dwFlags = KeyState.Down == state ? KEYEVENTF_EXTENDEDKEY : KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
            keybd_event((byte)key, 0x45, dwFlags, 0);
        }
    }
}
