using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Elements;
using Omniaudio.Elements.Dialogs;
using Omniaudio.Helpers;

namespace Omniaudio.Pages
{
    class Menu : IPage
    {
        private Title _title;
        private CHAR_INFO [,] rBuffer;
        private MenuDialog _basicDialog;
        private NotifyDialog _nd;
        private COORD dwBufferSize = new COORD((short)Console.BufferWidth,(short)Console.BufferHeight);
        private COORD dwBufferCoord;
        private SMALL_RECT rcRegion = new SMALL_RECT(0,0,(short)(Console.BufferWidth),(short)(Console.BufferHeight));
        private IntPtr oHandle = ConsoleHelper.GetStdOut();
        private CHAR_INFO[,] mBuffer;

        private bool canUpdate;
        
        public Menu(ref CHAR_INFO [,] buffer)
        {
            this.rBuffer = buffer;
            
            mBuffer =  new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            rBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref mBuffer); // clearing to be safe
            ConsoleHelper.ClearBuffer(ref rBuffer); // clearing to be safe
            dwBufferCoord.x = 0;
            dwBufferCoord.y = 0;

            canUpdate = true;
        }

        public void Init()
        {
            Logger.Instance.Log("log", "Initializing " + this.ToString()); 
             _title = new Title((Console.BufferWidth - 65) / 2, 10,ref mBuffer, false); // does not need to be redrawn, only "moved"
            
            _basicDialog = new MenuDialog((Console.BufferWidth  - 30) / 2,25,64,30,ref rBuffer, false);
            _basicDialog.AddOptions("View Playlists", "Create Session", "Join Session", "Settings", "Quit");
            _basicDialog.Padding = 5;
           
        }

        public void Update()
        {

            ConsoleHelper.WriteConsoleOutput(oHandle, mBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
            ConsoleHelper.CopyBuffer(ref rBuffer, ref mBuffer);


            if (canUpdate)
            {
                _title.Draw();
                _title.Update();


                _basicDialog.Draw();
                _basicDialog.Update();

                ConsoleHelper.RenderASCII_Image(new COORD((short)0, (short)(Console.BufferHeight - 5)), " __\n( ->\n/ )\\\n<_/_/\n \" \"", ref rBuffer, 0x0001 | 0x0008 | 0x0002);
                ConsoleHelper.WriteLineInBuffer(new COORD(0, 0), "Version O.1A", ref mBuffer, 0x0A | 0x0C);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)5, (short)(Console.BufferHeight - 3)), "@_CodeAssassin", ref rBuffer, 0x0001 | 0x0008 | 0x0002);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(Console.BufferWidth / 2 - 23), 17), "listen to music together, anytime, and everywhere", ref rBuffer,  0x0001 | 0x0002 | 0x0004 | 0x0008);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)((Console.BufferWidth - 23) / 2), (short)(Console.BufferHeight - 2)), "(C) Code Asssassin 2014 ♪", ref rBuffer, 0x0001 | 0x0002 | 0x0004 | 0x0008);
            }

            if (_nd != null)
            {
                _nd.Draw();
                _nd.Update();
            }
        }

        public void Cleanup()
        {
            Logger.Instance.Log("log","Cleaning up " + this.ToString()); 
            Logger.Instance.Flush();

            _title = null;

            _basicDialog = null;

            ConsoleHelper.ClearBuffer(ref rBuffer);
            ConsoleHelper.ClearBuffer(ref mBuffer);
            ConsoleHelper.WriteConsoleOutput(oHandle, mBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
        }


        private void Hold()
        {
            canUpdate = false;
        }

        private void Resume()
        {
            canUpdate = true;
            _nd = null;
        }
    }
}
