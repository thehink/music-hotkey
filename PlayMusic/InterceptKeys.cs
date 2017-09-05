using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PlayMusic;

public delegate void KeyEventHandler(KeypressEventArgs e);

class InterceptKeys
{
    private const int WH_MOUSE_LL = 14;
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    public static LowLevelKeyboardProc _proc = KeyboardHookCallback;
    public static LowLevelKeyboardProc _procMouse = MouseHookCallback;
    public static IntPtr _hookID = IntPtr.Zero;
    public static IntPtr _hookID_Mouse = IntPtr.Zero;

    public static event KeyEventHandler KeyEvent;

    public static void Hook()
    {
        _hookID = SetHook(_proc, WH_KEYBOARD_LL);
        _hookID_Mouse = SetHook(_procMouse, WH_MOUSE_LL);
    }

    public static void UnHook()
    {
        UnhookWindowsHookEx(_hookID);
        UnhookWindowsHookEx(_hookID_Mouse);
    }

    public static IntPtr SetHook(LowLevelKeyboardProc proc, int hookLL)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(hookLL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    public static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        hookEvent(nCode, wParam, lParam);
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    public static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            switch ((MouseMessages)wParam)
            {
                case MouseMessages.WM_LBUTTONDOWN:
                    KeyEvent(new KeypressEventArgs()
                    {
                        Key = Keys.LButton,
                        State = KeyState.Down
                    });
                    break;
                case MouseMessages.WM_LBUTTONUP:
                    KeyEvent(new KeypressEventArgs()
                    {
                        Key = Keys.LButton,
                        State = KeyState.Up
                    });
                    break;
            }

        }


        return CallNextHookEx(_hookID_Mouse, nCode, wParam, lParam);
    }

    public static void hookEvent(int nCode, IntPtr wParam, IntPtr lParam)
    {
        
        if (nCode >= 0 && wParam != (IntPtr)512)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            //Console.Write((Keys)vkCode);

            KeyEvent(new KeypressEventArgs()
            {
                Key = (Keys)vkCode,
                State = wParam == (IntPtr)WM_KEYDOWN ? KeyState.Down : KeyState.Up
            });
        }
    }

    private enum MouseMessages
    {
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_MOUSEMOVE = 0x0200,
        WM_MOUSEWHEEL = 0x020A,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}