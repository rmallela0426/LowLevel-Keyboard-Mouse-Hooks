using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace Test_Csharp
{
    class Hook
    {
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr LParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hookId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hookId,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        /* Keyboard & Mouse Identifiers */
        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        private static IntPtr KeyboardHookId = IntPtr.Zero;
        private static IntPtr MouseHookId = IntPtr.Zero;

        private static HookProc KeyboardProc = KeyboardcallbackFunc;
        private static HookProc MouseProc = MousecallbackFunc;

        private static IntPtr HookEx(int idHook, HookProc lpfn)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(idHook, lpfn, GetModuleHandle(curModule.ModuleName), 0/* IntPtr.Zero, (uint)GetCurrentThreadId()*/);
            }
        }

        private static IntPtr KeyboardcallbackFunc(int nCode, IntPtr Wparam, IntPtr LParam)
        {
            if (nCode >= 0 && (Wparam == (IntPtr)WM_KEYDOWN || Wparam == (IntPtr)WM_SYSKEYDOWN))
            {
                Console.WriteLine("Keyboard Callback \n");
            }
            return CallNextHookEx(KeyboardHookId, nCode, Wparam, LParam);
        }

        private static IntPtr MousecallbackFunc(int nCode, IntPtr Wparam, IntPtr LParam)
        {
            if (nCode >= 0 && (Wparam == (IntPtr)WM_LBUTTONDOWN || Wparam == (IntPtr)WM_RBUTTONDOWN
                || Wparam == (IntPtr)WM_MOUSEWHEEL || Wparam == (IntPtr)WM_MOUSEMOVE
                || Wparam == (IntPtr)WM_MOUSEHWHEEL))
            {
                Console.WriteLine("Mouse Callback \n");
            }
            return CallNextHookEx(MouseHookId, nCode, Wparam, LParam);
        }

        private static void Main()
        {
            KeyboardHookId = HookEx(WH_KEYBOARD_LL, KeyboardProc);
            if (KeyboardHookId == IntPtr.Zero)
            {
                Console.WriteLine(string.Format("SetWindowsHookEx failed to hook keyboard with error : {0}", Marshal.GetLastWin32Error()));
                return;
            }

            MouseHookId = HookEx(WH_MOUSE_LL, MouseProc);
            if (MouseHookId == IntPtr.Zero)
            {
                Console.WriteLine(string.Format("SetWindowsHookEx failed to hook mouse with error : {0}", Marshal.GetLastWin32Error()));
                return;
            }

            do
            {
                while (!Console.KeyAvailable)
                {
                    Application.DoEvents();
                }

            } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

            if (!UnhookWindowsHookEx(MouseHookId))
            {
                Console.WriteLine(string.Format("UnhookEx failed to unhook mouse with error: {0}", Marshal.GetLastWin32Error()));
            }

            if (!UnhookWindowsHookEx(KeyboardHookId))
            {
                Console.WriteLine(string.Format("UnhookEx failed to unhook keyboard with error: {0}", Marshal.GetLastWin32Error()));
            }
        }
    }
}
