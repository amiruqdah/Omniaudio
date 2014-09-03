using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Omniaudio
{
    static class WindowHelper
    {

        public static class SWP
        {
            public static readonly uint
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }

     
        
        private const int SWP_NOSIZE = 0x0001;
        private const int SW_MAXIMIZE = 3;
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool SetConsoleTitle(String lpConsoleTitle);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);
      
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] Omniaudio.Global.OpenFileName ofn);


        [DllImport("user32.dll",SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public static void MaximizeWindow()
        {
            Process p = Process.GetCurrentProcess();

            ShowWindow(p.MainWindowHandle, SW_MAXIMIZE); 
        }

        public static void SetWindowPosition(int x, int y, int width, int height, int HWND_FLAG = 0)
        {
            IntPtr thisConsole = GetConsoleWindow();
            SetWindowPos(thisConsole, HWND_FLAG , x, y, width, height, SWP_NOSIZE);
        }


    }
}
