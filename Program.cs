using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using Omniaudio.Pages;
using Omniaudio.Core;
using Omniaudio.Managers;
using Omniaudio.Helpers;
using Omniaudio.Properties;

namespace Omniaudio
{
 
    class Program
    {
        public static volatile bool hasQuit = false;
        public static Thread inputWorker; // basically allows "real-time" input for the textual user interface
        public static HandlerRoutine routine = new HandlerRoutine(ConsoleCtrlCheck);
        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            ConsoleHelper.SetConsoleCtrlHandler(routine, true);
            Console.Clear();

            Logger.Instance.Log("log", "Initializing Application");

            ConsoleHelper.SetConsoleFont(ConsoleHelper.GetStdOut(), 4);
           // WindowHelper.MaximizeWindow();
            WindowHelper.SetConsoleTitle("Omniaudio");
            WindowHelper.SetWindowLong32(WindowHelper.GetConsoleWindow(), WindowStyles.WS_OVERLAPPED | WindowStyles.WS_MINIMIZEBOX | WindowStyles.WS_THICKFRAME,0 );
            WindowHelper.SetWindowPos(WindowHelper.GetConsoleWindow(), Global.HWND_TOPMOST, 50, 50, Console.WindowWidth, Console.WindowHeight, WindowHelper.SWP.FRAMECHANGED | WindowHelper.SWP.NOMOVE | WindowHelper.SWP.NOSIZE | WindowHelper.SWP.NOZORDER | WindowHelper.SWP.NOOWNERZORDER);
            ConsoleHelper.SetConsoleOutputCP((uint)1200);
 
            
            IntPtr handleOut = ConsoleHelper.GetStdOut();
            COORD sPos;
            sPos.x = 0;
            sPos.y = 0;

            ConsoleHelper.SetConsoleCursorPosition(handleOut, sPos);
            ConsoleHelper.SetConsoleTextAttribute(handleOut, 0x0004 | 0x0008);
          
            Console.OutputEncoding = Encoding.Unicode;
            Console.CursorVisible = false;
            Console.SetWindowSize(195,57);
            Console.BufferWidth = Console.WindowWidth;
            Console.BufferHeight = Console.WindowHeight;

            ConsoleHelper.SetConsoleOutputCP((uint)866);
            ConsoleHelper.SetConsoleCP((uint)866);

            Logger.Instance.Log("log", "Setting Window/Console Attributes");


            CHAR_INFO[,] buffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            
            //clear the logs before we begin
            if(System.IO.File.Exists(Environment.CurrentDirectory + "\\log.log"))
                System.IO.File.WriteAllText(Environment.CurrentDirectory + "\\log.log", string.Empty);
            if (System.IO.File.Exists(Environment.CurrentDirectory + "\\server.log"))
                System.IO.File.WriteAllText(Environment.CurrentDirectory + "\\server.log", string.Empty);

            // if the user has already accepted the terms then grant them access to the program, if not then they will not be able ot use the program
            if ((bool)Settings.Default["terms"])
            {
                IPage menu;
                menu = new Menu(ref buffer);
                PageManager.Instance.AddPage(menu);
            }
            else {
                IPage terms;
                terms = new Terms(ref buffer);
                PageManager.Instance.AddPage(terms);
            }

            Logger.Instance.Flush();
            MainLoop();

        }     
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // Set this to true to keep the process from quitting immediately
            hasQuit = true;
            PageManager.Instance.Pop();
            Logger.Instance.Log("log","Quiting Application ...");
            Logger.Instance.Flush();
        }

        private static void MainLoop()
        {
            inputWorker = new Thread(() =>
            {

                    while (!hasQuit && !Console.KeyAvailable)
                    {
                        Global.cki = Console.ReadKey(true);
                    }
                 
            });
            inputWorker.IsBackground = true;
            inputWorker.Priority = ThreadPriority.Normal;
            inputWorker.Start();
          

            Logger.Instance.Log("log", "Input Worker created on thread id: " + inputWorker.ManagedThreadId);
            Logger.Instance.Flush();
            
            while (!hasQuit)
            {                
                            Thread.Sleep(Global.INTERVAL);
                            PageManager.Instance.Draw();
                          
            }

            inputWorker.Abort();
            PageManager.Instance.Pop();
            Logger.Instance.Log("log", "Input Worker task aborted");
            Logger.Instance.Log("log","Quiting Application ...");
            Logger.Instance.Flush(); // flush all data to file
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // 5 seconds to clean up at OS level
            PageManager.Instance.Pop();
            inputWorker.Abort();
            Logger.Instance.Log("log", "Input Worker task aborted");
            Logger.Instance.Log("log", "Quiting Application ...");
            Logger.Instance.Flush();
           
            return true;
        }

    }
}
