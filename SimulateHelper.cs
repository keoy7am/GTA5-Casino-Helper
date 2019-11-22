using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GTA5_Casino_Helper
{
    public static class SimulateHelper
    {
        #region Dll Imports

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);
        [DllImport("user32.dll")]
        public static extern Boolean GetWindowRect(IntPtr hWnd, ref Rectangle bounds);

        [DllImport("user32.dll", SetLastError = true)]

        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindowEx(IntPtr parent, IntPtr child, string classname, string captionName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg,
            IntPtr wParam, string lParam);
        [DllImport("User32.dll")]
        public static extern Int32 SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);

        public delegate bool CallBack(IntPtr hwnd, int lParam);
        public static IntPtr FindWindowEx(IntPtr hwnd, string lpszWindow, bool searchByTitle, bool bChild)
        {
            IntPtr iResult = IntPtr.Zero;
            iResult = FindWindowEx(hwnd, IntPtr.Zero, null, lpszWindow);
            if (iResult != IntPtr.Zero) return iResult;

            if (!bChild) return iResult;

            int i = EnumChildWindows(
            hwnd,
            (h, l) =>
            {
                IntPtr f1 = IntPtr.Zero;
                if (searchByTitle)
                {
                    f1 = FindWindowEx(h, IntPtr.Zero, null, lpszWindow);
                }
                else
                {
                    f1 = FindWindowEx(h, IntPtr.Zero, lpszWindow, null);
                }
                if (f1 == IntPtr.Zero)
                    return true;
                else
                {
                    iResult = f1;
                    return false;
                }
            },
            0);

            return iResult;
        }
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        #endregion
        #region Const
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CLOSE = 0xF060;
        private const int WM_CLOSE = 16;
        private const int BN_CLICKED = 245;
        private const int WM_SETTEXT = 0x000C;
        private const int WM_GETTEXT = 0x000D;
        private const int WM_GETTEXTLENGTH = 0x000E;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        #endregion
        #region Methods
        public static void LeftClick(int PositionX, int PositionY)
        {
            SetCursorPos(PositionX, PositionY);
            mouse_event(MOUSEEVENTF_LEFTDOWN, PositionX, PositionY, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, PositionX, PositionY, 0, 0);
        }
        public static void PressLeft(int PositionX, int PositionY)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, PositionX, PositionY, 0, 0);
        }
        public static void ReleaseLeft(int PositionX, int PositionY)
        {
            mouse_event(MOUSEEVENTF_LEFTUP, PositionX, PositionY, 0, 0);
        }
        #endregion
    }
}
