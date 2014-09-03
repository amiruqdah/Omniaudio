using System;
using System.Runtime.InteropServices;
namespace Omniaudio
{

  
    static class Global
    {
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        static readonly public int INTERVAL = 10;
        
        static readonly public IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly public IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly public IntPtr HWND_TOP = new IntPtr(0);
        static readonly public IntPtr HWND_BOTTOM = new IntPtr(1);
        
        

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int lstructSize;
            public  int hwndOwner;
            public  int hInstance;
            public string lpstrFilter = null;
            public string lpstrCustomFilter = null;
            public int lMaxCustomFilter;
            public int lFilterIndex;
            public string lpstrFile = null;
            public int lMaxFile = 0;
            public string lpstrFileTitle = null;
            public int lMaxFileTitle = 0;
            public string lpstrInitialDir = null;
            public string lpstrTitle = null;
            public int lFlags;
            public ushort nFileOffset;
            public ushort nFileExtension;
            public string lpstrDefExt = null;
            public int lCustData;
            public int lpfHook;
            public int lpTemplateName;
        }
        public static ConsoleKeyInfo cki;

    }
}
