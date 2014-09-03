using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Properties;
using Omniaudio.Managers;

namespace Omniaudio.Pages
{
    // Fires only once, makes sure that users will abide by terms
    class Terms : IPage
    {
        private CHAR_INFO[,] rBuffer;

        private COORD dwBufferSize = new COORD((short)Console.BufferWidth, (short)Console.BufferHeight);
        private COORD dwBufferCoord;
        private SMALL_RECT rcRegion = new SMALL_RECT(0, 0, (short)(Console.BufferWidth), (short)(Console.BufferHeight));
        private IntPtr oHandle = ConsoleHelper.GetStdOut();
        private CHAR_INFO[,] tBuffer;

        public Terms(ref CHAR_INFO[,] buffer)
        {
            this.rBuffer = buffer;

            tBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref rBuffer); // clearing to be safe
            ConsoleHelper.ClearBuffer(ref tBuffer); // clearing to be safe
            dwBufferCoord.x = 0;
            dwBufferCoord.y = 0;
        }
        
        public void Init()
        {
 
        }

        public void Update()
        {

            //this is actually suppoused to be an entire element but I'm lazy right now, fix later on

            if (Global.cki.Key == ConsoleKey.Y)
            {
                Settings.Default["terms"] = true;
                Settings.Default.Save();
                Global.cki = new ConsoleKeyInfo();
                CHAR_INFO [,]pBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
                ConsoleHelper.WriteConsoleOutput(oHandle, pBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
                PageManager.Instance.Pop();
                PageManager.Instance.AddPage(new Menu(ref pBuffer));

            }

            if (Global.cki.Key == ConsoleKey.N)
            {
                Settings.Default["terms"] = false;
                Settings.Default.Save(); // save to be safe
                ConsoleHelper.ClearBuffer(ref rBuffer);
                ConsoleHelper.ClearBuffer(ref tBuffer);
                ConsoleHelper.WriteConsoleOutput(oHandle, rBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
                PageManager.Instance.Pop();
            }

            ConsoleHelper.WriteConsoleOutput(oHandle, tBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
            ConsoleHelper.CopyBuffer(ref rBuffer, ref tBuffer);
            ConsoleHelper.RenderASCII_Image(new COORD(0, (short)19), @"                                                                      
                                                            
                                                            
                        ▒▓█████████▓▒                       
                    ▒██████▓▓▓▓▓▓▓██████░                   
                  ▓███▓              ░▓███▓                 
                ▓██▓                     ▓██▒               
              ░██▓                         ▓██              
             ▓██                       ▓███  ██▒            
            ██▓                 ▒▓██   ████   ██▓           
           ██▒            ▒▓▓   ▓████  ████▓   ██▒          
          ▓█▒         ░███████░  ██▓██░█▓▒██    ██          
          ██   ░▓▓▓░   ██▓  ██▓  ██▓ ███▒ ██▒    ██   ░▒▓█▓ 
         ██ ▓████████  ▓██ ▒██   ░██  ██  ███    ▒█▓███████ 
         █▓ ▓██░  ▒███ ░███████░  ██▓     ░▓▓▓████████████▓ 
        ▓█   ██▒   ▒██  ██▓  ███▓ ▒█▓ ░▒▓███████████▓▒      
        ██   ███   ▒██  ▓██   ▒▓▓▒▒▓█████████▓▓▒░  █░       
        ██   ▒██▒ ▓██▓   ▓▓▒▒▓▓█████████▓▒▒▒▓█     █▓       
        ██    █████▓░ ░▒▓█████████▓▒▒▒░ ▒████▓     █▓       
        ██    ▒▓▒▒▒▓█████████▓▓▒▒▒▓███▓ ░██  ▒▓    █▓       
        ▓█ ░▒▓██████████▓▒▒▒▒░   ██▒     ██████   ░█▒       
      ▒▓█████████▓▓▒▒▒  ███████  ██▓▓██░ ▒██   ░  ▓█        
  ████████████▓░▒▒▓███  ██▒ ░██  ▒██▓▒    ██████▒ ██        
  ██████▓▓█     ███░    ▓██▓██▓   ██░░▓█▓ ███▓▓  ▒█░        
  ▓▓▒     ██    ▒██▓███  ███▒███░ ██████▒        ██         
          ░█▓    ███▓▒░  ███  ███  ▒░           ██          
           ▓█▓   ▓██     ▒██                   ██░          
            ▓██   ██░                         ██░           
             ▒██  ▒▒                        ▒██             
               ██▓                        ░███              
                ▒███░                   ▒███░               
                  ▒████▒            ░▓████░                 
                     ▓████████▓████████▒                    
                         ▒▓███████▓▒                        
                                                             ", ref rBuffer,  0x0008);

            ConsoleHelper.DrawRectangle((Console.BufferWidth - 67) / 2, (Console.BufferHeight - 50) / 2, 67, 50, ref rBuffer);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)((Console.BufferWidth - 67) / 2 + 2d), (short)((Console.BufferHeight - 50) / 2 + 2)),
                "Dear Consumer,\n\nThank you for choosing Omniaudio.\n\nOmniaudio is a free,\nopen-source program that allows the streaming/broadcasting and \ndownloading of music. However, users of this program must\nrepsect all terms/agreements of the music licenses from \nwhich they have downloaded(from a third party distributor) or \nare streaming.\n\nOmniaudio does not intend to be an illegal or illegtimate\nfile sharing program, thus every user must honor this code.\nOmniaudio will not claim responsibility for those who \nviolate their respective copyright and intellectual property laws,\n of your state. Omniaudio is intended to be used as a legal\nmusic appreciation program that can be applied in most settings\nsuch as work enviornments, parties, and other social gatherings\n or purely for your personal enjoyment.\n\nFor your own safety(and sake) , only play DRM free \nmusic/audio files when sharing/streaming music.\n\nHappy listening!\n\n\n\n\n\n - The Omniaudio Team \n\n\n\n\n\n\n  P.S Don't forget to spread love and appreciation for music!"
                , ref tBuffer, 0x0010 | 0x0020 | 0x0080 | 0x0040 | 0x0080 );
            ConsoleHelper.DrawRectangle((Console.BufferWidth - 67) / 2, (Console.BufferHeight - 50) / 2 + 47, 8, 3, ref rBuffer, 0x0A | 0x0002);
            ConsoleHelper.DrawRectangle((Console.BufferWidth - 67) / 2 + 59, (Console.BufferHeight - 50) / 2 + 47, 8, 3, ref rBuffer, 0x0C);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)((Console.BufferWidth - 67) / 2 + 1), (short)((Console.BufferHeight - 50) / 2 + 47 + 1)), "(Y)es", ref rBuffer, 0x0020 | 0x0080 );
            ConsoleHelper.WriteLineInBuffer(new COORD((short)((Console.BufferWidth - 67) / 2 + 59 + 2), (short)((Console.BufferHeight - 50) / 2 + 47 + 1)), "(N)o", ref rBuffer, 0x0040 | 0x0080);       
        }

        public void Cleanup()
        {
            if (Global.cki.Key == ConsoleKey.N)
            {
                Environment.Exit(0);
            }
            else {
                Global.cki= new ConsoleKeyInfo();
            }

        }

        
    }
}
