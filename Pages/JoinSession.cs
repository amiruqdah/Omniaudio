using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Helpers;
using Omniaudio.Elements.Dialogs;

namespace Omniaudio.Pages
{
    class JoinSession : IPage
    {   
        
        // needed for double buffering
        private CHAR_INFO[,] rBuffer;
        private CHAR_INFO[,] jBuffer;

        private COORD dwBufferSize = new COORD((short)Console.BufferWidth, (short)Console.BufferHeight);
        private COORD dwBufferCoord = new COORD(0, 0);
        private SMALL_RECT rcRegion = new SMALL_RECT(0, 0, (short)(Console.BufferWidth), (short)(Console.BufferHeight));
        private IntPtr oHandle = ConsoleHelper.GetStdOut();

        private NotifyDialog notifyDialog;

        private string[] title;
        //Elements -- encapsulated 
        private ServerDialog sDialog; // grabs servers from Omniaudio server directory and lists them 
        private DirectConnectDialog dcDialog; // allows user to connect directly from whatever IP/PORT he/she specifies


        public JoinSession(ref CHAR_INFO[,] buffer)
        {
            rBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref rBuffer);
            jBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref jBuffer);
            ConsoleHelper.WriteConsoleOutput(oHandle, jBuffer, dwBufferSize, dwBufferCoord, ref rcRegion); 
            // write to clear sceren just in case there is some cruft left over

        }

        public void Init()
        {
            Logger.Instance.Log("log", "Initalizing " + this.ToString());
            sDialog = new ServerDialog((Console.BufferWidth - 120) / 2, 20, 120, 20, ref rBuffer, false);
            sDialog.connectionException += new ServerDialogConnectionException(sDialog_connectionException);
            sDialog.Init();


            
            Logger.Instance.Flush();
        }

        // update is both draw and "update"
        public void Update()
        {
            
            ConsoleHelper.WriteConsoleOutput(oHandle, jBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
            ConsoleHelper.CopyBuffer(ref rBuffer, ref jBuffer);
            ConsoleHelper.RenderASCII_Image(new COORD((short)((Console.BufferWidth - 100) / 2),2), @"
     ██╗ ██████╗ ██╗███╗   ██╗     █████╗     ███████╗███████╗██████╗ ██╗   ██╗███████╗██████╗ 
     ██║██╔═══██╗██║████╗  ██║    ██╔══██╗    ██╔════╝██╔════╝██╔══██╗██║   ██║██╔════╝██╔══██╗
     ██║██║   ██║██║██╔██╗ ██║    ███████║    ███████╗█████╗  ██████╔╝██║   ██║█████╗  ██████╔╝
██   ██║██║   ██║██║██║╚██╗██║    ██╔══██║    ╚════██║██╔══╝  ██╔══██╗╚██╗ ██╔╝██╔══╝  ██╔══██╗
╚█████╔╝╚██████╔╝██║██║ ╚████║    ██║  ██║    ███████║███████╗██║  ██║ ╚████╔╝ ███████╗██║  ██║
 ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝    ╚═╝  ╚═╝    ╚══════╝╚══════╝╚═╝  ╚═╝  ╚═══╝  ╚══════╝╚═╝  ╚═╝"
                , ref rBuffer);

            sDialog.Update();
            sDialog.Draw();

            if (notifyDialog != null)
            {
                notifyDialog.Draw();
                notifyDialog.Update();
            }

            if (Global.cki.Key == ConsoleKey.Backspace)
            {
                ConsoleHelper.ClearBuffer(ref jBuffer);
                Omniaudio.Managers.PageManager.Instance.Pop();
                Omniaudio.Managers.PageManager.Instance.AddPage(new Menu(ref jBuffer));
                Global.cki = new ConsoleKeyInfo();
            }

            if (Global.cki.Key == ConsoleKey.C)
            {
                dcDialog = new DirectConnectDialog((Console.BufferWidth - 50) / 2, 20, 50, 20, ref rBuffer, false);
                Global.cki = new ConsoleKeyInfo();
            }

            if (dcDialog != null)
            {
                dcDialog.Update();
                dcDialog.Draw();
            }


      
            // misc drawing

            // Blue Helper Tip
            ConsoleHelper.DrawRectangle(Console.BufferWidth - 35, Console.BufferHeight - 4, 35, 4, ref rBuffer, 0x0001);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(Console.BufferWidth - 27), (short)(Console.BufferHeight - 3)), "(C) Connect Directly", ref rBuffer, 0x0A | 0x0C | 0x0010);
        }


        public void Cleanup()
        {
            Logger.Instance.Log("log", "Cleaning up " + this.ToString());
            sDialog = null; // set to null for "early" GC
            Logger.Instance.Flush();
        }

        private void sDialog_connectionException()
        {
            notifyDialog = new NotifyDialog(5, 10, 160, 5, ref jBuffer, false, "test");
            notifyDialog.DialogDestroyed += new DialogDestroyed(Resume);
            notifyDialog.Message = "Server has failed to respond.";
        }

        private void Resume()
        {
            notifyDialog = null;
        }
        
    }

}
