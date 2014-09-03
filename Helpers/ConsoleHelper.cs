using System;
using System.Runtime.InteropServices;

namespace Omniaudio
{
    public delegate bool HandlerRoutine(CtrlTypes CtrlType);

    public enum CtrlTypes
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT,
        CTRL_CLOSE_EVENT,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        internal short x;
        internal short y;

        internal COORD(short x, short y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CHAR_INFO
    {
        [FieldOffset(0)]
        internal char UnicodeChar;
        [FieldOffset(0)]
        internal char AsciiChar;
        [FieldOffset(2)]
        internal UInt16 Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SMALL_RECT
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;

        public SMALL_RECT(short LEFT, short TOP, short RIGHT, short BOTTOM)
        {
            this.Left = LEFT;
            this.Top = TOP;
            this.Right = RIGHT;
            this.Bottom = BOTTOM;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ConsoleFont
    {
        public uint Index;
        public short SizeX, SizeY;
    }


    public static class ConsoleHelper
    {

        public const int TMPF_TRUETYPE = 4;
        public const int LF_FACESIZE = 32;
        public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public const int STD_OUTPUT_HANDLE = -11;


        [DllImport("kernel32")]
        public static extern bool SetConsoleFont(IntPtr hOutput, uint index);

        /* Grab a few low-level Win32 funcs */
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput,
          COORD dwCursorPosition);
        // Sets Draw POsition

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput,
           ushort wAttributes);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern bool SetConsoleActiveScreenBuffer(System.IntPtr hConsoleOutput);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", ThrowOnUnmappableChar = true, CharSet=CharSet.Auto)]
        private static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, short nNumberOfCharsToWrite, out short lpNumberOfCharsWritten, IntPtr lpReserved);

        [DllImport("kernel32.dll", EntryPoint = "WriteConsoleOutputW", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool WriteConsoleOutput(
            IntPtr hConsoleOutput,
            [MarshalAs(UnmanagedType.LPArray), In] CHAR_INFO[,] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpWriteRegion);

        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleOutputW", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ReadConsoleOutput(
            IntPtr hConsoleOutput,
            [MarshalAs(UnmanagedType.LPArray), Out] CHAR_INFO[,] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpReadRegion);

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr CreateConsoleScreenBuffer(
             UInt32 dwDesiredAccess,
             UInt32 dwShareMode,
             IntPtr securityAttributes,
             UInt32 flags,
             [MarshalAs(UnmanagedType.U4)] UInt32 screenBufferData
             );

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleOutputCP(uint wCodePageID);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCP(uint wCodePageID);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetConsoleCP();

        [DllImport("user32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, [MarshalAs(UnmanagedType.Bool)] bool bShow);


        public static IntPtr GetStdOut()
        {

            const int STD_OUTPUT_HANDLE = -11;
            IntPtr iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            return iStdOut;

        }
        public static short WriteConsoleLine ( string sLine )
        {

          short cchWritten;
          if ( ! WriteConsole( GetStdOut(),
                       sLine,
                       (short) sLine.Length,
                       out cchWritten,
                       (IntPtr) 0 ) )
            return -1;
          return cchWritten;

        }
        public static void MoveElement(int x, int y, int acrossX, int acrossY, int targX, int targY)
        {
            Console.MoveBufferArea(x, y, acrossX, acrossY, targX, targY);
        }
        public static void ClearCurrentConsoleLine(int x, int y)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(x, y);
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor);
        }
        public static CHAR_INFO[,] GrabBufferData(int x, int y, int width, int height, CHAR_INFO [,]buffer)
        {
            CHAR_INFO [,]tempBuffer = new CHAR_INFO[height, width];
            for (int i = 0; i < x; i++)
            {
                for (int k = 0; k < y; k++)
                {
                    tempBuffer[y, x] = buffer[y, x];
                }
            }

            return tempBuffer;
 
        }
        public static void ClearRect(int x, int y, int width, int height)
        {
            int curTop = Console.CursorTop;
            int curLeft = Console.CursorLeft;
            for (; height > 0; )
            {
                ConsoleHelper.SetConsoleCursorPosition(ConsoleHelper.GetStdOut(), new COORD((short)x, (short)(y + --height)));
                ConsoleHelper.WriteConsoleLine(new string(' ', width));
            }
            Console.SetCursorPosition(curLeft, curTop);
        }
        public static void ClearRectInBuffer(int x, int y, int width, int height, ref CHAR_INFO[,] rBuffer)
        {
            int startY = y;
            int startX = x;
            for (; height > -1; )
            {
                for (int w = width; w > -1; w--)
                {
                    rBuffer[y + height, x + w].UnicodeChar = ' ';
                    rBuffer[y + height, x + w].AsciiChar = ' ';
                }
                
                    --height;
            }
        }
        public static void WriteLineInBuffer(COORD pos, string str, ref CHAR_INFO[,] rBuffer, ushort Attr = 0x0C)
        {
            short startX = pos.x;
            int xLimit = rBuffer.GetUpperBound(1);
            for (int sp = 0; sp < str.Length; sp++)
            {

                rBuffer[pos.y, pos.x].UnicodeChar = str[sp];
                rBuffer[pos.y, pos.x].Attributes = Attr;
                pos.x += 1;
                if (pos.x > xLimit)
                {
                    pos.x = 0;
                    pos.y += 1;
                }
  
                if (sp < str.Length - 1)
                    if (str[sp] == '\n' || str[sp] == '\0')
                    {

                        rBuffer[pos.y, pos.x].UnicodeChar = ' ';
                        rBuffer[pos.y, pos.x - 1].UnicodeChar = ' ';
                        rBuffer[pos.y, pos.x].Attributes = Attr;
                        pos.y += 1;
                        pos.x = startX;
                        
                    }
                    else
                    {
                        rBuffer[pos.y, pos.x].UnicodeChar = str[sp];
                        rBuffer[pos.y, pos.x].Attributes = Attr;
                    }
                
            }
        }
        public static void WriteLineInBufferWithExtent(COORD pos, string str, ref CHAR_INFO[,] rBuffer, int wExtent, ushort Attr = 0x0C)
        {
            short startX = pos.x;
            int xLimit = rBuffer.GetUpperBound(1);
            for (int sp = 0; sp < str.Length; sp++)
            {

                rBuffer[pos.y, pos.x].UnicodeChar = str[sp];
                rBuffer[pos.y, pos.x].Attributes = Attr;
                pos.x += 1;
                if (pos.x > wExtent)
                {
                    pos.x = startX;
                    pos.y += 1;
                }

                if (sp < str.Length - 1)
                    if (str[sp] == '\n' || str[sp] == '\0')
                    {

                        rBuffer[pos.y, pos.x].UnicodeChar = ' ';
                        rBuffer[pos.y, pos.x - 1].UnicodeChar = ' ';
                        rBuffer[pos.y, pos.x].Attributes = Attr;
                        pos.y += 1;
                        pos.x = startX;

                    }
                    else
                    {
                        rBuffer[pos.y, pos.x].UnicodeChar = str[sp];
                        rBuffer[pos.y, pos.x].Attributes = Attr;
                    }

            }
        }
        public static void RenderASCII_Image(COORD pos, string str, ref CHAR_INFO[,] rBuffer, ushort Attr = 0x0A | 0x0B)
        {
            short startX = pos.x;
            int xLimit = rBuffer.GetUpperBound(1); 

            for (int sp = 0; sp < str.Length; sp++)
            {
               
                pos.x += 1;
                if (pos.x > xLimit)
                {
                    pos.x = 0;
                    pos.y += 1;
                }

                if(sp < str.Length - 1)
                    if (str[sp] == '\n' || str[sp] == '\0')
                    {
                        pos.x = startX;
                        pos.y += 1;
                    }


                if (str[sp] == '\n' || str[sp] == '\0')
                {
                    rBuffer[pos.y, pos.x].UnicodeChar = ' ';
                    rBuffer[pos.y, pos.x].Attributes = Attr;
                }
                else
                    rBuffer[pos.y, pos.x].UnicodeChar = str[sp];
                rBuffer[pos.y, pos.x].Attributes = Attr;
                

            }

        }
        public static void DrawRectangle(int x, int y, int w, int h, ref CHAR_INFO[,] rBuffer, ushort attr = 0x0001 | 0x0002 | 0x0004 | 0x0008)
        {
            int startX = x;

            for (int k = 0; k < h; k++)
            {
                for (int b = 0; b < w; b++)
                {
                    rBuffer[y, x].UnicodeChar = '█';
                    rBuffer[y, x].Attributes = attr;
                    x += 1;
                }
                x = startX;
                y += 1;
            }


        }
        public static void ClearBuffer(ref CHAR_INFO[,] rBuffer)
        {
            for (int y = 0; y < rBuffer.GetUpperBound(0); y++)
            {
                for (int x = 0; x < rBuffer.GetUpperBound(1); x++)
                {
                    rBuffer[y, x].UnicodeChar = ' ';
                    rBuffer[y, x].AsciiChar = ' ';
                    rBuffer[y,x].Attributes = 0; // not this time biatch
                }
            }
        }
        public static void CopyBuffer(ref CHAR_INFO[,] srcBuffer, ref CHAR_INFO[,] destBuffer)
        {
            for (int y = 0; y < destBuffer.GetUpperBound(0); ++y)
            {
                for (int x = 0; x < destBuffer.GetUpperBound(1); ++x)
                {
                     if (destBuffer[y, x].UnicodeChar != ' ' || destBuffer[y,x].UnicodeChar != '\0')
                    {
                        destBuffer[y, x]= srcBuffer[y, x];
                    }
                    
                }
            }
        }
     }
}